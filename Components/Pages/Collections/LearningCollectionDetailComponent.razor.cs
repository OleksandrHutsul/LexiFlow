using LexiFlow.Enums.LearningEnums;
using LexiFlow.Models.LearningModels;
using LexiFlow.Services.Learning;
using Microsoft.AspNetCore.Components;

namespace LexiFlow.Components.Pages.Collections;

public partial class LearningCollectionDetailComponent
{
    [Inject] public required ILearningService LearningStore { get; set; }

    [Parameter] public Guid CollectionId { get; set; }

    private LearningCollection? _collection;
    private bool _loading = true;

    protected override async Task OnParametersSetAsync()
    {
        _loading = true;

        try
        {
            _collection = await LearningStore.GetCollectionAsync(CollectionId);
        }
        finally
        {
            _loading = false;
        }
    }

    private static string StateLabel(LearningState state)
    {
        return state switch
        {
            LearningState.New => "New",
            LearningState.Learning => "Learning",
            LearningState.Review => "Review",
            LearningState.Mastered => "Learned",
            _ => state.ToString()
        };
    }
}
