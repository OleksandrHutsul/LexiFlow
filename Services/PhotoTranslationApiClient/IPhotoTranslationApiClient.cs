using LexiFlow.Enums.TranslationEnums;
using LexiFlow.Models;
using LexiFlow.Models.TranslationModels;

namespace LexiFlow.Services.PhotoTranslationApiClient;

public interface IPhotoTranslationApiClient
{
    Task<ApiResult<PhotoTranslationResult>> TranslateAsync(byte[] image, string fileName, string contentType, PhotoTranslationMode mode, CancellationToken cancellationToken);
    Task<ApiResult<PhotoTranslationResult>> TranslateTextAsync(string text, PhotoTranslationMode mode, CancellationToken cancellationToken);
}