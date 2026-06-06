using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.Web.Billing;

public class GetMyAvailabilityCalendar(IMediator mediator)
  : BaseEndpoint<GetMyAvailabilityCalendarQuery, AvailabilityCalendarResultDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/billing/my/availability/calendar");
    RequirePermission(PermissionCodes.BillingViewOwn);
    Summary(s => s.Summary = "Get signed-in user availability calendar");
  }
}

public class GetTeamAvailabilityCalendar(IMediator mediator)
  : BaseEndpoint<GetTeamAvailabilityCalendarQuery, AvailabilityCalendarResultDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/billing/availability/calendar");
    RequirePermission(PermissionCodes.BillingViewAny);
    Summary(s => s.Summary = "Get team availability calendar");
  }
}

public class GetMyAvailabilityDay(IMediator mediator)
  : BaseEndpoint<GetMyAvailabilityDayQuery, AvailabilityDayResultDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/billing/my/availability/days/{Date}");
    RequirePermission(PermissionCodes.BillingViewOwn);
    Summary(s => s.Summary = "Get signed-in user availability for a day");
  }
}

public class GetUserAvailabilityDay(IMediator mediator)
  : BaseEndpoint<GetUserAvailabilityDayQuery, AvailabilityDayResultDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/billing/users/{UserId}/availability/days/{Date}");
    RequirePermission(PermissionCodes.BillingViewAny);
    Summary(s => s.Summary = "Get user availability for a day");
  }
}

public class GetMyWeeklyPattern(IMediator mediator)
  : BaseEndpointWithoutRequest<GetMyWeeklyPatternQuery, WeeklyRecurringPatternDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/billing/my/availability/pattern");
    RequirePermission(PermissionCodes.BillingViewOwn);
    Summary(s => s.Summary = "Get signed-in user weekly availability pattern");
  }
}

public class GetUserWeeklyPattern(IMediator mediator)
  : BaseEndpoint<GetUserWeeklyPatternQuery, WeeklyRecurringPatternDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/billing/users/{UserId}/availability/pattern");
    RequirePermission(PermissionCodes.BillingViewAny);
    Summary(s => s.Summary = "Get user weekly availability pattern");
  }
}

public class SaveMyWeeklyPattern(IMediator mediator)
  : BaseEndpoint<SaveMyWeeklyPatternCommand, WeeklyRecurringPatternDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Put("/billing/my/availability/pattern");
    RequirePermission(PermissionCodes.BillingManageOwnAvailability);
    Summary(s => s.Summary = "Save signed-in user weekly availability pattern");
  }
}

public class SaveUserWeeklyPattern(IMediator mediator)
  : BaseEndpoint<SaveUserWeeklyPatternCommand, WeeklyRecurringPatternDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Put("/billing/users/{UserId}/availability/pattern");
    RequirePermission(PermissionCodes.BillingManageAvailability);
    Summary(s => s.Summary = "Save user weekly availability pattern");
  }
}

public class ResetMyWeeklyPattern(IMediator mediator)
  : BaseEndpointWithoutRequest<ResetMyWeeklyPatternCommand, WeeklyRecurringPatternDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/billing/my/availability/pattern/reset");
    RequirePermission(PermissionCodes.BillingManageOwnAvailability);
    Summary(s => s.Summary = "Reset signed-in user weekly pattern to organization defaults");
  }
}

public class ResetUserWeeklyPattern(IMediator mediator)
  : BaseEndpoint<ResetUserWeeklyPatternCommand, WeeklyRecurringPatternDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/billing/users/{UserId}/availability/pattern/reset");
    RequirePermission(PermissionCodes.BillingManageAvailability);
    Summary(s => s.Summary = "Reset user weekly pattern to organization defaults");
  }
}

public class CreateMyAvailabilityEntry(IMediator mediator)
  : BaseEndpoint<CreateMyAvailabilityEntryCommand, AvailabilityEntryDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/billing/my/availability/entries");
    RequirePermission(PermissionCodes.BillingManageOwnAvailability);
    Summary(s => s.Summary = "Create signed-in user manual availability entry");
  }
}

public class CreateUserAvailabilityEntry(IMediator mediator)
  : BaseEndpoint<CreateUserAvailabilityEntryCommand, AvailabilityEntryDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/billing/availability/entries");
    RequirePermission(PermissionCodes.BillingManageAvailability);
    Summary(s => s.Summary = "Create manual availability entry for a user");
  }
}

public class UpdateAvailabilityEntry(IMediator mediator)
  : BaseEndpoint<UpdateAvailabilityEntryCommand, AvailabilityEntryDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Put("/billing/availability/entries/{Id}");
    Summary(s => s.Summary = "Update manual availability entry");
  }
}

public class DeleteAvailabilityEntry(IMediator mediator)
  : BaseEndpoint<DeleteAvailabilityEntryCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Delete("/billing/availability/entries/{Id}");
    Summary(s => s.Summary = "Delete manual availability entry");
  }
}
