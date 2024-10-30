using Application.ControlledUpdates;
using FluentValidation;

namespace Application.Validators
{
    public class NewCampaignRequestValidator : AbstractValidator<NewCampaignRequest>
    {
        public NewCampaignRequestValidator()
        {
            RuleFor(x => x.Begin)
                .LessThanOrEqualTo(x => x.End)
                .WithMessage("End date should be later than begin date.")
                .GreaterThan(DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("Begin date should be a future date.");

            RuleFor(x => x.Warehouses).NotEmpty();

            RuleFor(x => x.Scopes).NotEmpty();
        }
    }
}
