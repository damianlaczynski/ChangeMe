using ChangeMe.Backend.UseCases.Issues;

namespace ChangeMe.Backend.Web.Issues;

public class DeleteIssueAttachment(IMediator mediator) : BaseEndpoint<DeleteIssueAttachmentCommand, Guid>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Delete("/issues/{IssueId}/attachments/{AttachmentId}");
    Summary(s => s.Summary = "Delete issue attachment");
  }
}

public sealed class DeleteIssueAttachmentCommandValidator : Validator<DeleteIssueAttachmentCommand>
{
  public DeleteIssueAttachmentCommandValidator()
  {
    RuleFor(x => x.IssueId)
      .NotEmpty();

    RuleFor(x => x.AttachmentId)
      .NotEmpty();
  }
}
