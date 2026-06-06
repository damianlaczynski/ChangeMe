using ChangeMe.Backend.UseCases.Time;
using ChangeMe.Backend.UseCases.Time.Dtos;

namespace ChangeMe.Backend.Web.Time;

public class GetMyTimeEntries(IMediator mediator)
  : BaseEndpoint<GetMyTimeEntriesQuery, MyTimeEntriesResultDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/time/my-entries");
    Summary(s => s.Summary = "Get my time entries");
  }
}
