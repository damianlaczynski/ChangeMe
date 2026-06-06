using Bogus;
using ChangeMe.Backend.DataGenerator.Options;
using ChangeMe.Backend.DataGenerator.Persistence;
using ChangeMe.Backend.Domain.Aggregates.Billing;
using ChangeMe.Backend.Domain.Aggregates.Billing.Entities;
using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;
using ChangeMe.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using DomainUser = global::ChangeMe.Backend.Domain.Aggregates.Users.User;

namespace ChangeMe.Backend.DataGenerator.Generators;

internal sealed class BillingGenerator(
  ApplicationDbContext dbContext,
  IOptions<DataGeneratorOptions> options)
{
  private const int RecurringHorizonDays = 365;

  private static readonly (string Name, string Department)[] DemoPositions =
  [
    ("Software Developer", "Engineering"),
    ("QA Engineer", "Engineering"),
    ("Project Manager", "Delivery"),
    ("UX Designer", "Design"),
  ];

  public async Task<BillingGenerationResult> GenerateAsync(
    IReadOnlyList<DomainUser> demoUsers,
    CancellationToken cancellationToken)
  {
    if (demoUsers.Count == 0 || !options.Value.GenerateBilling)
      return BillingGenerationResult.Empty;

    var config = options.Value;
    var faker = new Faker { Random = new Randomizer(config.Seed + 4) };
    var actorId = demoUsers[0].Id;
    var today = DateOnly.FromDateTime(DateTime.UtcNow);

    var settings = await dbContext.BillingSettings
      .FirstOrDefaultAsync(s => s.Id == BillingSettings.SingletonId, cancellationToken);
    if (settings is null)
      return BillingGenerationResult.Empty;

    var leaveTypes = await dbContext.LeaveTypes.AsNoTracking().ToListAsync(cancellationToken);
    var vacationType = leaveTypes.FirstOrDefault(t => t.Code == "VAC")
      ?? leaveTypes.FirstOrDefault();
    var sickType = leaveTypes.FirstOrDefault(t => t.Code == "SICK")
      ?? leaveTypes.FirstOrDefault();

    if (vacationType is null || sickType is null)
      return BillingGenerationResult.Empty;

    var approverId = await ResolveApproverUserIdAsync(config.EmailDomain, cancellationToken);
    var positions = await EnsureDemoPositionsAsync(actorId, cancellationToken);

    var profileCount = 0;
    var contractCount = 0;
    var patternCount = 0;
    var recurringEntryCount = 0;
    var manualEntryCount = 0;
    var leaveRequestCount = 0;

    for (var index = 0; index < demoUsers.Count; index++)
    {
      var user = demoUsers[index];
      var position = positions[index % positions.Count];
      var fte = index % 4 == 2 ? 0.5m : 1.0m;

      var profileResult = EmploymentProfile.Create(
        user.Id,
        employeeId: $"EMP-{index + 1:D3}",
        nationalId: null,
        taxId: null,
        bankAccount: null,
        notes: "Demo employment profile");
      if (!profileResult.IsSuccess)
        throw new InvalidOperationException($"Failed to create employment profile: {string.Join(", ", profileResult.ValidationErrors.Select(e => e.ErrorMessage))}");

      var profile = profileResult.Value;
      EntityAudit.Apply(profile, actorId);
      await dbContext.EmploymentProfiles.AddAsync(profile, cancellationToken);
      profileCount++;

      var contractResult = EmploymentContract.Create(
        user.Id,
        position.Id,
        index % 3 == 0 ? ContractType.B2B : ContractType.Employment,
        today.AddMonths(-6),
        endDate: null,
        fte,
        monthlyHoursNormMinutes: 9600,
        hourlyRate: null,
        monthlySalary: faker.Random.Decimal(8000, 16000),
        notes: "Demo contract");
      if (!contractResult.IsSuccess)
        throw new InvalidOperationException($"Failed to create employment contract: {string.Join(", ", contractResult.ValidationErrors.Select(e => e.ErrorMessage))}");

      var contract = contractResult.Value;
      EntityAudit.Apply(contract, actorId);
      await dbContext.EmploymentContracts.AddAsync(contract, cancellationToken);
      contractCount++;

      var pattern = WeeklyRecurringPattern.CreateDefault(user.Id, settings, fte);
      ApplyPatternVariation(pattern, settings, index);
      EntityAudit.Apply(pattern, actorId);
      EntityAudit.Apply(pattern.Days, actorId);
      await dbContext.WeeklyRecurringPatterns.AddAsync(pattern, cancellationToken);
      patternCount++;

      recurringEntryCount += await RegenerateRecurringEntriesAsync(
        user.Id,
        pattern,
        today,
        cancellationToken);

      manualEntryCount += await GenerateManualEntriesAsync(
        user.Id,
        actorId,
        today,
        faker,
        cancellationToken);
    }

    leaveRequestCount = await GenerateLeaveRequestsAsync(
      demoUsers,
      approverId,
      actorId,
      vacationType,
      sickType,
      settings,
      today,
      faker,
      cancellationToken);

    await dbContext.SaveChangesAsync(cancellationToken);

    return new BillingGenerationResult(
      profileCount,
      contractCount,
      patternCount,
      recurringEntryCount,
      manualEntryCount,
      leaveRequestCount);
  }

  private async Task<List<Position>> EnsureDemoPositionsAsync(
    Guid actorId,
    CancellationToken cancellationToken)
  {
    var positions = new List<Position>();

    foreach (var (name, department) in DemoPositions)
    {
      var normalizedName = Position.NormalizeName(name);
      var existing = await dbContext.Positions
        .FirstOrDefaultAsync(p => p.NormalizedName == normalizedName, cancellationToken);
      if (existing is not null)
      {
        positions.Add(existing);
        continue;
      }

      var positionResult = Position.Create(name, department, $"Demo {name.ToLowerInvariant()} role");
      if (!positionResult.IsSuccess)
        throw new InvalidOperationException($"Failed to create position: {string.Join(", ", positionResult.ValidationErrors.Select(e => e.ErrorMessage))}");

      var position = positionResult.Value;
      EntityAudit.Apply(position, actorId);
      await dbContext.Positions.AddAsync(position, cancellationToken);
      positions.Add(position);
    }

    await dbContext.SaveChangesAsync(cancellationToken);
    return positions;
  }

  private static void ApplyPatternVariation(
    WeeklyRecurringPattern pattern,
    BillingSettings settings,
    int userIndex)
  {
    switch (userIndex % 5)
    {
      case 1:
        ReplacePatternDays(
          pattern,
          pattern.Days.Select(day => new WeeklyRecurringPatternDayInput(
            day.DayOfWeek,
            day.Enabled,
            day.StartTime,
            day.EndTime,
            day.DayOfWeek == DayOfWeek.Wednesday && day.Enabled
              ? AvailabilityStatus.Remote
              : day.Status)).ToList());
        break;

      case 2:
        ReplacePatternDays(
          pattern,
          pattern.Days.Select(day => new WeeklyRecurringPatternDayInput(
            day.DayOfWeek,
            day.Enabled,
            day.StartTime,
            day.DayOfWeek == DayOfWeek.Friday && day.Enabled
              ? settings.HalfDaySplitTime
              : day.EndTime,
            day.Status)).ToList());
        break;

      case 3:
        ReplacePatternDays(
          pattern,
          Enum.GetValues<DayOfWeek>()
            .Select(day =>
            {
              var enabled = day is DayOfWeek.Tuesday or DayOfWeek.Thursday;
              return new WeeklyRecurringPatternDayInput(
                day,
                enabled,
                enabled ? settings.DefaultWorkdayStart : null,
                enabled ? settings.DefaultWorkdayEnd : null,
                enabled ? AvailabilityStatus.OnSite : null);
            })
            .ToList());
        break;
    }
  }

  private static void ReplacePatternDays(
    WeeklyRecurringPattern pattern,
    IReadOnlyList<WeeklyRecurringPatternDayInput> dayInputs)
  {
    var replaceResult = pattern.ReplaceDays(dayInputs);
    if (!replaceResult.IsSuccess)
      throw new InvalidOperationException($"Failed to customize weekly pattern: {string.Join(", ", replaceResult.ValidationErrors.Select(e => e.ErrorMessage))}");
  }

  private async Task<int> RegenerateRecurringEntriesAsync(
    Guid userId,
    WeeklyRecurringPattern pattern,
    DateOnly fromDate,
    CancellationToken cancellationToken)
  {
    var toDate = fromDate.AddDays(RecurringHorizonDays);
    var count = 0;

    foreach (var day in pattern.Days.Where(d =>
               d.Enabled && d.StartTime.HasValue && d.EndTime.HasValue && d.Status.HasValue))
    {
      for (var date = fromDate; date <= toDate; date = date.AddDays(1))
      {
        if (date.DayOfWeek != day.DayOfWeek)
          continue;

        var entryResult = AvailabilityEntry.CreateRecurring(
          userId,
          date,
          day.StartTime!.Value,
          day.EndTime!.Value,
          day.Status!.Value);
        if (!entryResult.IsSuccess)
          throw new InvalidOperationException($"Failed to create recurring availability entry: {string.Join(", ", entryResult.ValidationErrors.Select(e => e.ErrorMessage))}");

        await dbContext.AvailabilityEntries.AddAsync(entryResult.Value, cancellationToken);
        count++;
      }
    }

    return count;
  }

  private async Task<int> GenerateManualEntriesAsync(
    Guid userId,
    Guid actorId,
    DateOnly today,
    Faker faker,
    CancellationToken cancellationToken)
  {
    var count = 0;
    var remoteDay = today.AddDays(faker.Random.Int(3, 10));
    while (remoteDay.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
      remoteDay = remoteDay.AddDays(1);

    var remoteResult = AvailabilityEntry.CreateManual(
      userId,
      remoteDay,
      remoteDay,
      allDay: true,
      startTime: null,
      endTime: null,
      AvailabilityStatus.Remote,
      notes: "Demo remote day");
    if (!remoteResult.IsSuccess)
      throw new InvalidOperationException($"Failed to create remote availability entry: {string.Join(", ", remoteResult.ValidationErrors.Select(e => e.ErrorMessage))}");

    var remoteEntry = remoteResult.Value;
    EntityAudit.Apply(remoteEntry, actorId);
    await dbContext.AvailabilityEntries.AddAsync(remoteEntry, cancellationToken);
    count++;

    var onsiteDay = today.AddDays(faker.Random.Int(11, 20));
    while (onsiteDay.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday || onsiteDay == remoteDay)
      onsiteDay = onsiteDay.AddDays(1);

    var onsiteResult = AvailabilityEntry.CreateManual(
      userId,
      onsiteDay,
      onsiteDay,
      allDay: false,
      startTime: new TimeOnly(10, 0),
      endTime: new TimeOnly(12, 0),
      AvailabilityStatus.OnSite,
      notes: "Demo on-site meeting block");
    if (!onsiteResult.IsSuccess)
      throw new InvalidOperationException($"Failed to create on-site availability entry: {string.Join(", ", onsiteResult.ValidationErrors.Select(e => e.ErrorMessage))}");

    var onsiteEntry = onsiteResult.Value;
    EntityAudit.Apply(onsiteEntry, actorId);
    await dbContext.AvailabilityEntries.AddAsync(onsiteEntry, cancellationToken);
    count++;

    return count;
  }

  private async Task<int> GenerateLeaveRequestsAsync(
    IReadOnlyList<DomainUser> demoUsers,
    Guid approverId,
    Guid actorId,
    LeaveType vacationType,
    LeaveType sickType,
    BillingSettings settings,
    DateOnly today,
    Faker faker,
    CancellationToken cancellationToken)
  {
    if (demoUsers.Count == 0)
      return 0;

    var count = 0;
    var scenarios = new List<(DomainUser User, LeaveType Type, DateOnly Start, DateOnly End, LeaveDayPortion? Portion, string Reason)>
    {
      (
        demoUsers[0],
        vacationType,
        NextWeekday(today.AddDays(21)),
        NextWeekday(today.AddDays(21)).AddDays(4),
        null,
        "Demo vacation"),
      (
        demoUsers[Math.Min(1, demoUsers.Count - 1)],
        sickType,
        NextWeekday(today.AddDays(7)),
        NextWeekday(today.AddDays(7)),
        LeaveDayPortion.FullDay,
        "Demo sick leave"),
    };

    if (demoUsers.Count > 2)
    {
      var halfDay = NextWeekday(today.AddDays(10));
      scenarios.Add((
        demoUsers[2],
        vacationType,
        halfDay,
        halfDay,
        LeaveDayPortion.FirstHalf,
        "Demo half-day leave"));
    }

    foreach (var scenario in scenarios)
    {
      var requestResult = LeaveRequest.CreateDraft(
        scenario.User.Id,
        scenario.Type.Id,
        scenario.Start,
        scenario.End,
        scenario.Portion,
        scenario.Reason);
      if (!requestResult.IsSuccess)
        throw new InvalidOperationException($"Failed to create leave request: {string.Join(", ", requestResult.ValidationErrors.Select(e => e.ErrorMessage))}");

      var request = requestResult.Value;
      var submitResult = request.Submit(DateTime.UtcNow, scenario.Type.RequiresApproval);
      if (!submitResult.IsSuccess)
        throw new InvalidOperationException($"Failed to submit leave request: {string.Join(", ", submitResult.ValidationErrors.Select(e => e.ErrorMessage))}");

      if (scenario.Type.RequiresApproval)
      {
        var approveResult = request.Approve(approverId, DateTime.UtcNow);
        if (!approveResult.IsSuccess)
          throw new InvalidOperationException($"Failed to approve leave request: {string.Join(", ", approveResult.ValidationErrors.Select(e => e.ErrorMessage))}");
      }

      EntityAudit.Apply(request, scenario.User.Id);
      await dbContext.LeaveRequests.AddAsync(request, cancellationToken);
      await SyncLeaveEntriesAsync(request, scenario.Type.Name, settings, cancellationToken);
      count++;
    }

    return count;
  }

  private async Task SyncLeaveEntriesAsync(
    LeaveRequest request,
    string leaveTypeName,
    BillingSettings settings,
    CancellationToken cancellationToken)
  {
    var workdayStart = settings.DefaultWorkdayStart;
    var workdayEnd = settings.DefaultWorkdayEnd;
    var halfDaySplit = settings.HalfDaySplitTime;

    for (var date = request.StartDate; date <= request.EndDate; date = date.AddDays(1))
    {
      var allDay = true;
      TimeOnly? startTime = null;
      TimeOnly? endTime = null;

      if (request.StartDate == request.EndDate && request.DayPortion is not null and not LeaveDayPortion.FullDay)
      {
        allDay = false;
        if (request.DayPortion == LeaveDayPortion.FirstHalf)
        {
          startTime = workdayStart;
          endTime = halfDaySplit;
        }
        else
        {
          startTime = halfDaySplit;
          endTime = workdayEnd;
        }
      }

      var entryResult = AvailabilityEntry.CreateLeave(
        request.UserId,
        date,
        allDay,
        startTime,
        endTime,
        leaveTypeName,
        request.Id);
      if (!entryResult.IsSuccess)
        throw new InvalidOperationException($"Failed to create leave availability entry: {string.Join(", ", entryResult.ValidationErrors.Select(e => e.ErrorMessage))}");

      await dbContext.AvailabilityEntries.AddAsync(entryResult.Value, cancellationToken);
    }
  }

  private async Task<Guid> ResolveApproverUserIdAsync(
    string emailDomain,
    CancellationToken cancellationToken)
  {
    var emailSuffix = $"@{emailDomain.Trim().ToUpperInvariant()}";

    var approverId = await dbContext.Users
      .AsNoTracking()
      .Where(u => !u.NormalizedEmail.EndsWith(emailSuffix))
      .OrderBy(u => u.CreatedAt)
      .Select(u => u.Id)
      .FirstOrDefaultAsync(cancellationToken);

    if (approverId == Guid.Empty)
      throw new InvalidOperationException("No non-demo user found to approve demo leave requests.");

    return approverId;
  }

  private static DateOnly NextWeekday(DateOnly date)
  {
    while (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
      date = date.AddDays(1);

    return date;
  }
}

internal sealed record BillingGenerationResult(
  int ProfileCount,
  int ContractCount,
  int PatternCount,
  int RecurringEntryCount,
  int ManualEntryCount,
  int LeaveRequestCount)
{
  public static BillingGenerationResult Empty { get; } = new(0, 0, 0, 0, 0, 0);
}
