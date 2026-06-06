using Ardalis.Result;
using ChangeMe.Backend.Domain.Aggregates.Time;

namespace ChangeMe.Backend.UnitTests.Domain.Aggregates.Time;

public sealed class TimeEntryTests
{
  private static readonly Guid TestProjectId = Guid.CreateVersion7();
  private static readonly Guid TestAuthorId = Guid.CreateVersion7();

  [Fact]
  public void Create_WhenInputIsValid_ShouldCreateEntry()
  {
    var workDate = DateOnly.FromDateTime(DateTime.UtcNow);

    var result = TimeEntry.Create(
      TestAuthorId,
      TestProjectId,
      issueId: null,
      workDate,
      durationMinutes: 60,
      description: "  Work done  ");

    Assert.True(result.IsSuccess);
    Assert.Equal(TestAuthorId, result.Value.AuthorUserId);
    Assert.Equal(TestProjectId, result.Value.ProjectId);
    Assert.Equal(workDate, result.Value.WorkDate);
    Assert.Equal(60, result.Value.DurationMinutes);
    Assert.Equal("Work done", result.Value.Description);
  }

  [Fact]
  public void Create_WhenProjectIsEmpty_ShouldReturnInvalidResult()
  {
    var result = TimeEntry.Create(
      TestAuthorId,
      Guid.Empty,
      null,
      DateOnly.FromDateTime(DateTime.UtcNow),
      30,
      null);

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(1441)]
  public void Create_WhenDurationIsOutOfRange_ShouldReturnInvalidResult(int durationMinutes)
  {
    var result = TimeEntry.Create(
      TestAuthorId,
      TestProjectId,
      null,
      DateOnly.FromDateTime(DateTime.UtcNow),
      durationMinutes,
      null);

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
  }

  [Fact]
  public void Create_WhenDescriptionExceedsMaxLength_ShouldReturnInvalidResult()
  {
    var result = TimeEntry.Create(
      TestAuthorId,
      TestProjectId,
      null,
      DateOnly.FromDateTime(DateTime.UtcNow),
      30,
      new string('x', TimeConstraints.DescriptionMaxLength + 1));

    Assert.False(result.IsSuccess);
    Assert.Equal(ResultStatus.Invalid, result.Status);
  }

  [Fact]
  public void Update_WhenInputIsValid_ShouldUpdateMutableFields()
  {
    var createResult = TimeEntry.Create(
      TestAuthorId,
      TestProjectId,
      null,
      DateOnly.FromDateTime(DateTime.UtcNow),
      30,
      "Initial");

    var newProjectId = Guid.CreateVersion7();
    var newWorkDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

    var updateResult = createResult.Value.Update(
      newProjectId,
      null,
      newWorkDate,
      90,
      "Updated");

    Assert.True(updateResult.IsSuccess);
    Assert.Equal(newProjectId, updateResult.Value.ProjectId);
    Assert.Equal(newWorkDate, updateResult.Value.WorkDate);
    Assert.Equal(90, updateResult.Value.DurationMinutes);
    Assert.Equal("Updated", updateResult.Value.Description);
    Assert.Equal(TestAuthorId, updateResult.Value.AuthorUserId);
  }
}
