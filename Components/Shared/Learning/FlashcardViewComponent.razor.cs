using LexiFlow.Enums.LearningEnums;
using LexiFlow.Models.LearningModels;
using Microsoft.AspNetCore.Components;

namespace LexiFlow.Components.Shared.Learning;

public partial class FlashcardViewComponent
{
    [Parameter, EditorRequired] public LearningWord Word { get; set; } = new();

    private bool _flipped;

    private string StateLabel
    {
        get
        {
            return Word.State switch
            {
                LearningState.New => "New",
                LearningState.Learning => "Learning",
                LearningState.Review => "Review",
                LearningState.Mastered => "Learned",
                _ => Word.State.ToString()
            };
        }
    }

    private void Flip()
    {
        _flipped = !_flipped;
    }
}
