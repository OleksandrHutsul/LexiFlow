using LexiFlow.Enums.LearningEnums;
using LexiFlow.Models.DictionaryModels;
using LexiFlow.Models.LearningModels;
using LexiFlow.Services.LearningActivity;
using LexiFlow.Services.LearningCollectionsApiClient;
using LexiFlow.Services.LocalStorage;
using LexiFlow.Services.State;

namespace LexiFlow.Services.Learning;

public class LearningService : ILearningService
{
    private const string Key = "lexiflow:collections";
    private static readonly string[] Accents = ["#35d39d", "#7c5cff", "#ffb020", "#4aa8ff", "#ff5c8a"];

    private readonly ILearningCollectionsApiClient _api;
    private readonly ILocalStorageService _storage;
    private readonly ILearningActivityService _activityService;
    private readonly StatisticsChangeNotifier _notifier;

    public LearningService(ILearningCollectionsApiClient api, ILocalStorageService storage, ILearningActivityService activityService, StatisticsChangeNotifier notifier)
    {
        _api = api;
        _storage = storage;
        _activityService = activityService;
        _notifier = notifier;
    }

    public async Task<List<LearningCollection>> GetCollectionsAsync()
    {
        var result = await _api.GetAsync();

        if (!result.IsSuccess || result.Value is null)
            throw new InvalidOperationException(result.Error ?? "Learning collections could not be loaded.");

        if (result.Value.Initialized)
            return result.Value.Collections;

        var collections = await _storage.GetAsync<List<LearningCollection>>(Key) ?? CreateDefaultCollections();

        Normalize(collections);

        var migrated = await _api.SaveAsync(collections);

        if (!migrated.IsSuccess || migrated.Value is null)
            throw new InvalidOperationException(migrated.Error ?? "Learning collections could not be migrated to the database.");

        await _storage.RemoveAsync(Key);

        return migrated.Value.Collections;
    }

    public async Task<LearningCollection?> GetDefaultCollectionAsync()
    {
        var collections = await GetCollectionsAsync();
        return collections.SingleOrDefault(collection => collection.IsDefault);
    }

    public async Task<LearningCollection> CreateCollectionAsync(string name, string? accent = null, bool makeDefault = false)
    {
        var collections = await GetCollectionsAsync();
        var cleanName = ValidateName(name, collections);

        var collection = new LearningCollection
        {
            Id = Guid.NewGuid(),
            Name = cleanName,
            Accent = string.IsNullOrWhiteSpace(accent) ? Accents[collections.Count % Accents.Length] : accent,
            IsDefault = makeDefault
        };

        if (makeDefault)
            collections.ForEach(item => item.IsDefault = false);

        collections.Add(collection);

        await SaveCollectionsAsync(collections);

        return collection;
    }

    public async Task<LearningCollection> UpdateCollectionAsync(Guid id, string name, string accent, bool makeDefault)
    {
        var collections = await GetCollectionsAsync();
        var collection = collections.SingleOrDefault(item => item.Id == id)
            ?? throw new InvalidOperationException("Collection not found.");

        var cleanName = ValidateName(name, collections, id);

        collection.Name = cleanName;
        collection.Accent = string.IsNullOrWhiteSpace(accent) ? collection.Accent : accent;

        foreach (var word in collection.Words)
            word.CollectionName = cleanName;

        if (makeDefault)
            collections.ForEach(item => item.IsDefault = item.Id == id);
        else
            collection.IsDefault = false;

        await SaveCollectionsAsync(collections);

        return collection;
    }

    public async Task<DeleteLearningCollectionResult> DeleteCollectionAsync(Guid id)
    {
        try
        {
            var collections = await GetCollectionsAsync();
            var collection = collections.SingleOrDefault(item => item.Id == id);

            if (collection is null)
            {
                Console.Error.WriteLine($"Collection {id} was not found in persistent storage.");
                return DeleteLearningCollectionResult.Failure("The collection could not be found. Refresh the page and try again.");
            }

            var deletedDefault = collection.IsDefault;
            var deleted = await _api.DeleteAsync(id);

            if (!deleted.IsSuccess)
            {
                Console.Error.WriteLine($"Failed to delete learning collection {id}: {deleted.Error}");
                return DeleteLearningCollectionResult.Failure(deleted.Error ?? "The collection could not be deleted.");
            }

            var verified = await _api.GetAsync();

            if (!verified.IsSuccess || verified.Value is null || verified.Value.Collections.Any(item => item.Id == id))
                return DeleteLearningCollectionResult.Failure("The database could not verify collection deletion.");

            _notifier.NotifyChanged();

            return DeleteLearningCollectionResult.Success(deletedDefault);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Failed to delete learning collection {id}: {exception}");
            return DeleteLearningCollectionResult.Failure("Collection deletion failed. Please try again.");
        }
    }

    public async Task<bool> SetDefaultCollectionAsync(Guid id)
    {
        var collections = await GetCollectionsAsync();

        if (collections.All(item => item.Id != id))
            return false;

        collections.ForEach(item => item.IsDefault = item.Id == id);

        await SaveCollectionsAsync(collections);

        return true;
    }

    public async Task<AddLearningWordResult> AddWordAsync(DictionaryEntry entry, Guid? collectionId = null)
    {
        var collections = await GetCollectionsAsync();
        var collection = collectionId.HasValue
            ? collections.SingleOrDefault(item => item.Id == collectionId.Value)
            : collections.SingleOrDefault(item => item.IsDefault);

        if (collection is null)
            return new(AddLearningWordStatus.CollectionRequired);

        if (collection.Words.Any(word => word.Word.Equals(entry.Word, StringComparison.OrdinalIgnoreCase)))
            return new(AddLearningWordStatus.AlreadyExists, collection.Name);

        collection.Words.Insert(0, ToLearningWord(entry, collection.Name));

        await SaveCollectionsAsync(collections);

        return new(AddLearningWordStatus.Added, collection.Name);
    }

    public async Task<BulkLearningWordResult> AddWordsAsync(IEnumerable<DictionaryEntry> entries, Guid collectionId)
    {
        var collections = await GetCollectionsAsync();
        var collection = collections.SingleOrDefault(item => item.Id == collectionId)
            ?? throw new InvalidOperationException("Choose a collection before importing words.");

        var added = 0;
        var duplicates = 0;

        foreach (var entry in entries.Where(entry => !string.IsNullOrWhiteSpace(entry.Word)).DistinctBy(entry => entry.Word, StringComparer.OrdinalIgnoreCase))
        {
            if (collection.Words.Any(word => word.Word.Equals(entry.Word, StringComparison.OrdinalIgnoreCase)))
            {
                duplicates++;
                continue;
            }

            collection.Words.Insert(0, ToLearningWord(entry, collection.Name));
            added++;
        }

        if (added > 0)
            await SaveCollectionsAsync(collections);

        return new(added, duplicates, collection.Name);
    }

    public async Task UpdateWordAsync(LearningWord word, LearningState state, TimeSpan? activeTime = null)
    {
        var collections = await GetCollectionsAsync();
        var target = collections.SelectMany(collection => collection.Words).FirstOrDefault(item => item.Id == word.Id);

        if (target is null) return;

        target.State = state;
        target.LastReviewedAt = DateTimeOffset.UtcNow;

        switch (state)
        {
            case LearningState.Mastered:
                target.EasyCount++;
                break;

            case LearningState.Learning:
            case LearningState.Review:
                target.HardCount++;
                break;

            default:
                target.AgainCount++;
                break;
        }

        await SaveCollectionsAsync(collections);
        await _activityService.RecordPracticeAsync(state == LearningState.Mastered ? 1 : 0, 1, activeTime ?? TimeSpan.Zero);
    }

    public async Task<List<LearningWord>> GetWordsAsync()
    {
        var collections = await GetCollectionsAsync();
        return collections.SelectMany(collection => collection.Words).ToList();
    }

    public async Task<LearningCollection?> GetCollectionAsync(Guid id)
    {
        var collections = await GetCollectionsAsync();
        return collections.SingleOrDefault(collection => collection.Id == id);
    }

    public async Task<List<LearningWord>> GetWordsAsync(Guid collectionId)
    {
        var collection = await GetCollectionAsync(collectionId);
        return collection?.Words.ToList() ?? [];
    }

    private async Task SaveCollectionsAsync(List<LearningCollection> collections)
    {
        Normalize(collections);

        var result = await _api.SaveAsync(collections);

        if (!result.IsSuccess)
            throw new InvalidOperationException(result.Error ?? "Learning collections could not be saved.");

        _notifier.NotifyChanged();
    }

    private static List<LearningCollection> CreateDefaultCollections()
    {
        return
        [
            new() { Name = "A2", Accent = "#35d39d", IsDefault = true },
            new() { Name = "Work", Accent = "#7c5cff" },
            new() { Name = "Travel", Accent = "#ffb020" },
            new() { Name = "Programming", Accent = "#4aa8ff" },
            new() { Name = "IELTS", Accent = "#ff5c8a" }
        ];
    }

    private static LearningWord ToLearningWord(DictionaryEntry entry, string collectionName)
    {
        var meaning = entry.Meanings
            .SelectMany(part => part.GuideWordGroups)
            .SelectMany(group => group.Meanings)
            .FirstOrDefault();

        return new LearningWord
        {
            Id = Guid.NewGuid(),
            Word = entry.Word.Trim(),
            CollectionName = collectionName,
            Level = meaning?.CefrLevel,
            Definition = meaning?.Definition,
            Translation = entry.Translations.FirstOrDefault()?.Text
        };
    }

    private static string ValidateName(string name, IEnumerable<LearningCollection> collections, Guid? currentId = null)
    {
        var cleanName = name.Trim();

        if (string.IsNullOrWhiteSpace(cleanName))
            throw new InvalidOperationException("Collection name is required.");

        if (cleanName.Length > 60)
            throw new InvalidOperationException("Collection names can contain up to 60 characters.");

        if (collections.Any(item => item.Id != currentId && item.Name.Equals(cleanName, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("A collection with this name already exists.");

        return cleanName;
    }

    private static void Normalize(List<LearningCollection> collections)
    {
        var defaultFound = false;

        foreach (var collection in collections)
        {
            if (collection.Id == Guid.Empty)
                collection.Id = Guid.NewGuid();

            if (collection.IsDefault)
            {
                if (defaultFound)
                    collection.IsDefault = false;
                else
                    defaultFound = true;
            }

            foreach (var word in collection.Words)
            {
                if (word.CollectionName != collection.Name)
                    word.CollectionName = collection.Name;

                if (word.Id == Guid.Empty)
                    word.Id = Guid.NewGuid();
            }
        }
    }
}
