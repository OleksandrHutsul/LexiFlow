using FluentValidation;
using LexiFlow.Models.SettingModels;

namespace LexiFlow.Validators;

public class UserSettingsValidator : AbstractValidator<UserSettings>
{
    public UserSettingsValidator()
    {
        RuleFor(x => x.DailyGoal).GreaterThan(0);

        RuleFor(x => x.DefaultPronunciation)
            .NotEmpty()
            .Must(value => value is "US" or "UK")
            .WithMessage("Choose US or UK pronunciation.");
    }
}