using System.Diagnostics;
using LexiFlow.Enums.LearningEnums;
using LexiFlow.Models.LearningModels;
using LexiFlow.Services.Learning;
using Microsoft.AspNetCore.Components;

namespace LexiFlow.Components.Pages.Learning;

public partial class FlashcardsComponent
{
    [Inject] public required ILearningService Learning { get; set; }

    [SupplyParameterFromQuery(Name = "collectionId")] public Guid? CollectionId { get; set; }

    private readonly Stopwatch _cardTimer = new();

    private List<LearningWord> _allWords = [];
    private List<LearningWord> _words = [];
    private string? _collectionName;
    private int _index;
    private bool _loading = true;
    private StudyFocus _focus = StudyFocus.ToLearn;

    private int ToLearnCount => _allWords.Count(word => word.State != LearningState.Mastered);

    protected override async Task OnParametersSetAsync()
    {
        _loading = true;
        _index = 0;

        try
        {
            if (CollectionId is Guid collectionId)
            {
                var collection = await Learning.GetCollectionAsync(collectionId);

                _collectionName = collection?.Name;
                _allWords = collection?.Words.ToList() ?? [];
            }
            else
            {
                _collectionName = null;
                _allWords = await Learning.GetWordsAsync();
            }

            ApplyFocus();
            _cardTimer.Restart();
        }
        finally
        {
            _loading = false;
        }
    }

    private void SetFocus(StudyFocus focus)
    {
        _focus = focus;
        _index = 0;

        ApplyFocus();
        _cardTimer.Restart();
    }

    private void ApplyFocus()
    {
        _words = _focus == StudyFocus.ToLearn
            ? _allWords.Where(word => word.State != LearningState.Mastered).ToList()
            : _allWords.ToList();

        if (_index >= _words.Count)
            _index = 0;
    }

    private async Task MarkAsync(LearningState state)
    {
        if (_words.Count == 0) return;

        _cardTimer.Stop();

        var current = _words[_index];
        current.State = state;

        await Learning.UpdateWordAsync(current, state, _cardTimer.Elapsed);

        if (_focus == StudyFocus.ToLearn && state == LearningState.Mastered)
        {
            _words.RemoveAt(_index);

            if (_index >= _words.Count)
                _index = 0;
        }
        else
        {
            _index = (_index + 1) % _words.Count;
        }

        _cardTimer.Restart();
    }
}
