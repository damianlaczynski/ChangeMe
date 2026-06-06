using ChangeMe.Backend.UseCases.Time;

namespace ChangeMe.Backend.Web.Time;

public class DiscardRunningTimer(IMediator mediator)
  : BaseEndpointWithoutRequest<DiscardRunningTimerCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Delete("/time/running-timer");
    Summary(s => s.Summary = "Discard running timer");
  }
}
