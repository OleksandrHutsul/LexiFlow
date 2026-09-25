using Microsoft.JSInterop;
using MudBlazor;

namespace LexiFlow.Components.Pages.Collections;

public partial class VocabularyListDetailComponent
{
    private bool _flipped;
    private int _cardIndex;

    private void FlipCard()
    {
        _flipped = !_flipped;
    }

    private void PreviousCard()
    {
        if (_cardIndex == 0) return;

        _cardIndex--;
        _flipped = false;
    }

    private void NextCard()
    {
        if (_list is null || _cardIndex >= _list.Words.Count - 1) return;

        _cardIndex++;
        _flipped = false;
    }

    private async Task PlayAudioAsync(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;

        try
        {
            await JS.InvokeVoidAsync("lexiFlow.audio.playExclusive", url);
        }
        catch (JSException)
        {
            Snackbar.Add("Pronunciation audio could not be played.", Severity.Warning);
        }
    }
}
