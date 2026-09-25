using LexiFlow.Enums.TranslationEnums;
using LexiFlow.Models.TranslationModels;
using LexiFlow.Models.VocabularyModels;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;

namespace LexiFlow.Components.Pages.PhotoTranslate;

public partial class PhotoTranslateComponent
{
    private async Task HandleFileAsync(InputFileChangeEventArgs args)
    {
        if (_processing) return;

        var file = args.File;

        if (!IsSupported(file.ContentType, file.Name))
        {
            _error = "Use a JPG, JPEG, PNG, or WebP image.";
            _result = null;
            return;
        }

        if (file.Size > MaxBytes)
        {
            _error = "Images must be smaller than 10 MB.";
            _result = null;
            return;
        }

        await BeginProcessingUiAsync();

        try
        {
            await using var stream = file.OpenReadStream(MaxBytes);
            using var memory = new MemoryStream();

            await stream.CopyToAsync(memory, _cts?.Token ?? CancellationToken.None);
            await ProcessAsync(memory.ToArray(), file.Name, file.ContentType, true);
        }
        catch (OperationCanceledException)
        {
            _processing = false;
            await InvokeAsync(StateHasChanged);
        }
        catch
        {
            _processing = false;
            _error = "The image could not be read.";

            await InvokeAsync(StateHasChanged);
        }
    }

    [JSInvokable]
    public async Task FailImageUpload(string message)
    {
        _processing = false;
        _dragging = false;
        _error = string.IsNullOrWhiteSpace(message) ? "The image could not be read." : message;

        await InvokeAsync(StateHasChanged);
    }

    [JSInvokable]
    public void SetDragging(bool value)
    {
        _dragging = value;
        _ = InvokeAsync(StateHasChanged);
    }

    private async Task BeginProcessingUiAsync()
    {
        _cts?.Cancel();
        _cts?.Dispose();

        _cts = new CancellationTokenSource();
        _processing = true;
        _dragging = false;
        _error = "";
        _translationNotice = "";
        _result = null;
        _tokens = [];
        _reviewItems = [];
        _importComplete = false;

        ClearSelectionState();

        await InvokeAsync(StateHasChanged);
    }

    private async Task ProcessAsync(byte[] bytes, string name, string type, bool uiAlreadyShown = false)
    {
        if (_processing && !uiAlreadyShown) return;

        if (bytes.Length == 0)
        {
            _processing = false;
            _error = "The image is empty.";

            await InvokeAsync(StateHasChanged);
            return;
        }

        if (bytes.Length > MaxBytes || !IsSupported(type, name))
        {
            _processing = false;
            _error = bytes.Length > MaxBytes
                ? "Images must be smaller than 10 MB."
                : "Use a JPG, JPEG, PNG, or WebP image.";

            await InvokeAsync(StateHasChanged);
            return;
        }

        if (!uiAlreadyShown)
            await BeginProcessingUiAsync();

        try
        {
            var result = await PhotoApi.TranslateAsync(bytes, name, type, _mode, _cts!.Token);

            if (result.IsSuccess && result.Value is not null)
                ApplyTranslationResult(result.Value);
            else
                _error = result.Error ?? "The image could not be processed.";
        }
        catch (OperationCanceledException)
        {
        }
        catch
        {
            _error = "The image could not be processed.";
        }
        finally
        {
            _processing = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private void ApplyTranslationResult(PhotoTranslationResult result)
    {
        _result = result;
        _tokens = Tokenize(result.OriginalText);
        _reviewItems = _mode == PhotoTranslationMode.WordList ? BuildReviewItems(result) : [];
        _translationNotice = "";
        _error = "";
        _inspectorPhrase = "";

        ClearSelectionState();
    }

    private async Task RetryTranslationAsync()
    {
        if (_result is null || string.IsNullOrWhiteSpace(_result.OriginalText) || _retryingTranslation) return;

        _retryingTranslation = true;
        _translationNotice = "";

        try
        {
            var result = await PhotoApi.TranslateTextAsync(_result.OriginalText, _mode, CancellationToken.None);

            if (!result.IsSuccess || result.Value is null)
            {
                _translationNotice = result.Error ?? "Translation retry failed.";
                return;
            }

            ApplyTranslationResult(result.Value);

            if (!string.IsNullOrWhiteSpace(result.Value.TranslationError))
            {
                _translationNotice = result.Value.TranslationError;
            }
            else if (string.IsNullOrWhiteSpace(result.Value.TranslatedText) && _mode == PhotoTranslationMode.Text)
            {
                _translationNotice = "Translation is still unavailable. Recognized text was kept.";
            }
            else
            {
                Snackbar.Add("Translation updated.", Severity.Success);
            }
        }
        catch
        {
            _translationNotice = "Translation retry failed.";
        }
        finally
        {
            _retryingTranslation = false;
        }
    }

    private static bool IsSupported(string type, string name)
    {
        string[] contentTypes = ["image/jpeg", "image/png", "image/webp"];
        string[] extensions = [".jpg", ".jpeg", ".png", ".webp"];

        return contentTypes.Contains(type, StringComparer.OrdinalIgnoreCase) || extensions.Contains(Path.GetExtension(name), StringComparer.OrdinalIgnoreCase);
    }

    private static List<TextToken> Tokenize(string text)
    {
        var matches = System.Text.RegularExpressions.Regex.Matches(text, @"[\p{L}\p{M}]+(?:['’\-][\p{L}\p{M}]+)*");
        var result = new List<TextToken>();
        var index = 0;

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            if (match.Index > index)
                result.Add(new TextToken(text[index..match.Index], false, result.Count));

            result.Add(new TextToken(match.Value, true, result.Count));
            index = match.Index + match.Length;
        }

        if (index < text.Length)
            result.Add(new TextToken(text[index..], false, result.Count));

        return result;
    }
}
