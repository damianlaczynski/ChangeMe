using ChangeMe.Backend.UseCases.Time;
using ChangeMe.Backend.UseCases.Time.Dtos;

namespace ChangeMe.Backend.Web.Time;

public class GetRunningTimer(IMediator mediator)
  : BaseEndpointWithoutRequest<GetRunningTimerQuery, RunningTimerStateDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/time/running-timer");
    Summary(s => s.Summary = "Get running timer");
  }
}
