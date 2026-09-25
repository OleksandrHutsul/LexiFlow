using System.Net.Http.Headers;
using LexiFlow.Configuration;
using LexiFlow.Enums.TranslationEnums;
using LexiFlow.Models;
using LexiFlow.Models.TranslationModels;
using LexiFlow.Services.Http;

namespace LexiFlow.Services.PhotoTranslationApiClient;

public class PhotoTranslationApiClient : IPhotoTranslationApiClient
{
    private readonly IHttpService _httpService;

    public PhotoTranslationApiClient(IHttpService httpService)
    {
        _httpService = httpService;
    }

    public async Task<ApiResult<PhotoTranslationResult>> TranslateAsync(byte[] image, string fileName, string contentType, PhotoTranslationMode mode, CancellationToken cancellationToken)
    {
        using var content = new MultipartFormDataContent();
        using var imageContent = new ByteArrayContent(image);

        imageContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        content.Add(imageContent, "image", fileName);
        content.Add(new StringContent(mode.ToString()), "mode");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/photo-translate")
        {
            Content = content
        };

        return await _httpService.SendAsync<PhotoTranslationResult>(request, new ApiRequestOptions
        {
            RequiresAuthentication = true,
            ErrorMessage = "Photo translation is temporarily unavailable."
        }, cancellationToken);
    }

    public Task<ApiResult<PhotoTranslationResult>> TranslateTextAsync(string text, PhotoTranslationMode mode, CancellationToken cancellationToken)
    {
        return _httpService.PostAsync<PhotoTranslationResult>("/api/photo-translate/text", new { text, mode }, new ApiRequestOptions
        {
            RequiresAuthentication = true,
            ErrorMessage = "Translation is temporarily unavailable."
        }, cancellationToken);
    }
}
