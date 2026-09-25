using LexiFlow.Enums.LearningEnums;
using LexiFlow.Models.DictionaryModels;
using LexiFlow.Models.LearningModels;

namespace LexiFlow.Services.Learning;

public interface ILearningService
{
    Task<List<LearningCollection>> GetCollectionsAsync();
    Task<LearningCollection?> GetDefaultCollectionAsync();
    Task<LearningCollection> CreateCollectionAsync(string name, string? accent = null, bool makeDefault = false);
    Task<LearningCollection> UpdateCollectionAsync(Guid id, string name, string accent, bool makeDefault);
    Task<DeleteLearningCollectionResult> DeleteCollectionAsync(Guid id);
    Task<bool> SetDefaultCollectionAsync(Guid id);
    Task<AddLearningWordResult> AddWordAsync(DictionaryEntry entry, Guid? collectionId = null);
    Task<BulkLearningWordResult> AddWordsAsync(IEnumerable<DictionaryEntry> entries, Guid collectionId);
    Task UpdateWordAsync(LearningWord word, LearningState state, TimeSpan? activeTime = null);
    Task<List<LearningWord>> GetWordsAsync();
    Task<LearningCollection?> GetCollectionAsync(Guid id);
    Task<List<LearningWord>> GetWordsAsync(Guid collectionId);
}
