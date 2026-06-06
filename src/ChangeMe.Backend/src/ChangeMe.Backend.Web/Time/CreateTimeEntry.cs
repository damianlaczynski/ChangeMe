using ChangeMe.Backend.Domain.Aggregates.Time;
using ChangeMe.Backend.UseCases.Time;
using ChangeMe.Backend.UseCases.Time.Dtos;

namespace ChangeMe.Backend.Web.Time;

public class CreateTimeEntry(IMediator mediator) : BaseEndpoint<CreateTimeEntryCommand, TimeEntryDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/time/entries");
    Summary(s => s.Summary = "Create time entry");
  }
}

public sealed class CreateTimeEntryCommandValidator : Validator<CreateTimeEntryCommand>
{
  public CreateTimeEntryCommandValidator()
  {
    RuleFor(x => x.ProjectId).NotEmpty();
    RuleFor(x => x.DurationMinutes)
      .InclusiveBetween(TimeConstraints.MinDurationMinutes, TimeConstraints.MaxDurationMinutes);
    RuleFor(x => x.Description)
      .MaximumLength(TimeConstraints.DescriptionMaxLength)
      .When(x => !string.IsNullOrWhiteSpace(x.Description));
  }
}
