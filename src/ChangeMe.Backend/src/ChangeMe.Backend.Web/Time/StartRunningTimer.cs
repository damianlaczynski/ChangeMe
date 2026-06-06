using ChangeMe.Backend.UseCases.Time;
using ChangeMe.Backend.UseCases.Time.Dtos;

namespace ChangeMe.Backend.Web.Time;

public class StartRunningTimer(IMediator mediator) : BaseEndpoint<StartRunningTimerCommand, RunningTimerDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/time/running-timer");
    Summary(s => s.Summary = "Start running timer");
  }
}
