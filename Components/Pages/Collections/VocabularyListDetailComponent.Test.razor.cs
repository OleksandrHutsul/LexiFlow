using LexiFlow.Models.QuizModels;
using LexiFlow.Models.VocabularyModels;
using Microsoft.JSInterop;
using MudBlazor;
using System.Diagnostics;

namespace LexiFlow.Components.Pages.Collections;

public partial class VocabularyListDetailComponent
{
    private readonly Stopwatch _testTimer = new();

    private string _typedAnswer = "";
    private bool _testActive;
    private bool _testComplete;
    private bool _answering;
    private int _questionIndex;
    private int _correct;
    private List<TestQuestion> _questions = [];
    private List<TestAnswer> _answers = [];

    private double TestProgress
    {
        get
        {
            if (_questions.Count == 0)
                return 0;

            return 100d * _questionIndex / _questions.Count;
        }
    }

    private async Task StartTestAsync()
    {
        if (_list is null) return;

        var usableWords = _list.Words
            .Where(word => !string.IsNullOrWhiteSpace(word.Translation))
            .ToList();

        if (usableWords.Count == 0) return;

        _questions = usableWords
            .Select((word, index) => BuildQuestion(word, usableWords, index))
            .OrderBy(_ => Random.Shared.Next())
            .ToList();

        _answers = [];
        _questionIndex = 0;
        _correct = 0;
        _testComplete = false;
        _testActive = true;
        _typedAnswer = "";

        _testTimer.Restart();

        await JS.InvokeVoidAsync("lexiFlow.testGuard.enable");
    }

    private static TestQuestion BuildQuestion(VocabularyListWord word, List<VocabularyListWord> allWords, int index)
    {
        if (index % 2 != 0)
            return new TestQuestion($"Type the English word for “{word.Translation}”", word.Word, []);

        var distractors = allWords
            .Where(item => item.Id != word.Id)
            .Select(item => item.Translation)
            .Where(translation => !string.IsNullOrWhiteSpace(translation) && !string.Equals(translation, word.Translation, StringComparison.OrdinalIgnoreCase))
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(_ => Random.Shared.Next())
            .Take(3)
            .ToList();

        var choices = distractors
            .Append(word.Translation!)
            .OrderBy(_ => Random.Shared.Next())
            .ToList();

        return new TestQuestion($"Choose the translation of “{word.Word}”", word.Translation!, choices);
    }

    private async Task AnswerAsync(string given)
    {
        if (!_testActive || _answering || string.IsNullOrWhiteSpace(given)) return;

        _answering = true;

        try
        {
            var question = _questions[_questionIndex];
            var correct = string.Equals(given.Trim(), question.Answer.Trim(), StringComparison.OrdinalIgnoreCase);

            if (correct)
                _correct++;

            _answers.Add(new TestAnswer(question.Prompt, given, question.Answer, correct));
            _typedAnswer = "";
            _questionIndex++;

            if (_questionIndex < _questions.Count) return;

            _testActive = false;
            _testComplete = true;
            _testTimer.Stop();

            await JS.InvokeVoidAsync("lexiFlow.testGuard.disable");

            if (_list is not null)
            {
                var result = await ListsApi.SaveTestResultAsync(_list.Id, new()
                {
                    CorrectAnswers = _correct,
                    TotalQuestions = _questions.Count
                });

                if (!result.IsSuccess)
                    Snackbar.Add("Result shown, but the API could not save it.", Severity.Warning);
            }

            await Activity.RecordPracticeAsync(_correct, _questions.Count, _testTimer.Elapsed);
        }
        finally
        {
            _answering = false;
        }
    }
}
