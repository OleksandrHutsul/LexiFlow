using System.Diagnostics;
using LexiFlow.Models.LearningModels;
using LexiFlow.Models.QuizModels;
using LexiFlow.Services.Learning;
using LexiFlow.Services.LearningActivity;
using Microsoft.AspNetCore.Components;

namespace LexiFlow.Components.Pages.Learning;

public partial class QuizComponent
{
    [Inject] public required ILearningService Learning { get; set; }
    [Inject] public required ILearningActivityService Activity { get; set; }

    private readonly Stopwatch _sessionTimer = new();

    private List<QuizQuestion> _questions = [];
    private List<LearningWord> _words = [];
    private int _index;
    private int _correct;
    private bool _finished;
    private bool _loading = true;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        _words = await Learning.GetWordsAsync();
        _questions = BuildQuestions(_words);
        _loading = false;

        if (_questions.Count > 0)
            _sessionTimer.Start();

        StateHasChanged();
    }

    private static List<QuizQuestion> BuildQuestions(List<LearningWord> words)
    {
        var pool = words
            .Where(word => !string.IsNullOrWhiteSpace(word.Definition) || !string.IsNullOrWhiteSpace(word.Translation))
            .OrderBy(_ => Random.Shared.Next())
            .Take(8)
            .ToList();

        var questions = new List<QuizQuestion>();

        foreach (var word in pool)
        {
            var answer = word.Translation ?? word.Definition ?? word.Word;

            var distractors = pool
                .Where(item => item.Word != word.Word)
                .Select(item => item.Translation ?? item.Definition ?? item.Word)
                .Where(value => !string.IsNullOrWhiteSpace(value) && value != answer)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(_ => Random.Shared.Next())
                .Take(3)
                .ToList();

            var choices = distractors
                .Append(answer)
                .OrderBy(_ => Random.Shared.Next())
                .ToList();

            questions.Add(new QuizQuestion
            {
                Type = "Choose translation",
                Prompt = word.Word,
                Answer = answer,
                Choices = choices
            });
        }

        return questions;
    }

    private async Task AnswerAsync(string choice)
    {
        if (choice == _questions[_index].Answer)
            _correct++;

        if (_index < _questions.Count - 1)
        {
            _index++;
            return;
        }

        _finished = true;
        _sessionTimer.Stop();

        await Activity.RecordPracticeAsync(_correct, _questions.Count, _sessionTimer.Elapsed);
    }

    private void Restart()
    {
        _questions = BuildQuestions(_words);
        _index = 0;
        _correct = 0;
        _finished = false;

        _sessionTimer.Restart();
    }
}
