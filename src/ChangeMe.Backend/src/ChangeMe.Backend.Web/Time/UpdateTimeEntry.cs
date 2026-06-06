using ChangeMe.Backend.Domain.Aggregates.Time;
using ChangeMe.Backend.UseCases.Time;
using ChangeMe.Backend.UseCases.Time.Dtos;

namespace ChangeMe.Backend.Web.Time;

public class UpdateTimeEntry(IMediator mediator) : BaseEndpoint<UpdateTimeEntryCommand, TimeEntryDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Put("/time/entries/{Id}");
    Summary(s => s.Summary = "Update time entry");
  }
}

public sealed class UpdateTimeEntryCommandValidator : Validator<UpdateTimeEntryCommand>
{
  public UpdateTimeEntryCommandValidator()
  {
    RuleFor(x => x.Id).NotEmpty();
    RuleFor(x => x.ProjectId).NotEmpty();
    RuleFor(x => x.DurationMinutes)
      .InclusiveBetween(TimeConstraints.MinDurationMinutes, TimeConstraints.MaxDurationMinutes);
    RuleFor(x => x.Description)
      .MaximumLength(TimeConstraints.DescriptionMaxLength)
      .When(x => !string.IsNullOrWhiteSpace(x.Description));
  }
}
