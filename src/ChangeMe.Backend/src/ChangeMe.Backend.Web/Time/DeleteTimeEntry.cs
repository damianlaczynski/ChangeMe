using ChangeMe.Backend.UseCases.Time;

namespace ChangeMe.Backend.Web.Time;

public class DeleteTimeEntry(IMediator mediator) : BaseEndpoint<DeleteTimeEntryCommand, Guid>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Delete("/time/entries/{Id}");
    Summary(s => s.Summary = "Delete time entry");
  }
}

public sealed class DeleteTimeEntryCommandValidator : Validator<DeleteTimeEntryCommand>
{
  public DeleteTimeEntryCommandValidator()
  {
    RuleFor(x => x.Id).NotEmpty();
  }
}
