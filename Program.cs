using FluentValidation;
using LexiFlow.Components;
using LexiFlow.Configuration;
using LexiFlow.Models.SettingModels;
using LexiFlow.Services.AuthApiClient;
using LexiFlow.Services.Authentication;
using LexiFlow.Services.AuthTokenStore;
using LexiFlow.Services.DictionaryApiClient;
using LexiFlow.Services.Favorites;
using LexiFlow.Services.FeedbackApiClient;
using LexiFlow.Services.Http;
using LexiFlow.Services.Learning;
using LexiFlow.Services.LearningActivity;
using LexiFlow.Services.LearningCollectionsApiClient;
using LexiFlow.Services.LocalStorage;
using LexiFlow.Services.NotificationsApiClient;
using LexiFlow.Services.PhotoTranslationApiClient;
using LexiFlow.Services.State;
using LexiFlow.Services.Statistics;
using LexiFlow.Services.UserSettings;
using LexiFlow.Services.VocabularyListsApiClient;
using LexiFlow.Validators;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

builder.Services.Configure<AppContactOptions>(builder.Configuration.GetSection(AppContactOptions.SectionName));

builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("Developer", policy => policy.RequireRole("Admin"));
});

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<IValidator<UserSettings>, UserSettingsValidator>();

var apiBaseUrl = builder.Configuration["DictionaryApi:BaseUrl"] ?? "https://localhost:7130";

builder.Services.AddScoped(_ => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl),
    Timeout = TimeSpan.FromMinutes(2)
});

builder.Services.AddScoped<IHttpService, HttpService>();

builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<IAuthTokenStore, AuthTokenStore>();

builder.Services.AddScoped<LexiFlowAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<LexiFlowAuthenticationStateProvider>());

builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IFavoritesService, FavoritesService>();
builder.Services.AddScoped<ILearningService, LearningService>();
builder.Services.AddScoped<ILearningActivityService, LearningActivityService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IUserSettingsService, UserSettingsService>();

builder.Services.AddScoped<IAuthApiClient, AuthApiClient>();
builder.Services.AddScoped<IDictionaryApiClient, DictionaryApiClient>();
builder.Services.AddScoped<IFeedbackApiClient, FeedbackApiClient>();
builder.Services.AddScoped<ILearningCollectionsApiClient, LearningCollectionsApiClient>();
builder.Services.AddScoped<INotificationsApiClient, NotificationsApiClient>();
builder.Services.AddScoped<IPhotoTranslationApiClient, PhotoTranslationApiClient>();
builder.Services.AddScoped<IVocabularyListsApiClient, VocabularyListsApiClient>();

builder.Services.AddScoped<StatisticsChangeNotifier>();

await builder.Build().RunAsync();
