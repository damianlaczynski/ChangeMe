using ChangeMe.Backend.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace ChangeMe.Backend.UnitTests;

public sealed class PaginationQueryableExtensionsTests
{
  [Fact]
  public async Task ToPaginationResultAsync_WhenCalledForEnumerable_ShouldReturnPagedItems()
  {
    var source = new[]
    {
      new PaginationItem { Id = 3, Title = "Gamma" },
      new PaginationItem { Id = 1, Title = "Alpha" },
      new PaginationItem { Id = 2, Title = "Beta" }
    };

    var parameters = PaginationParameters<PaginationItemDto>.Create(pageNumber: 1, pageSize: 2, sortField: "Title", ascending: true);

    var result = await source.ToPaginationResultAsync(
      item => new PaginationItemDto { Id = item.Id, Title = item.Title },
      parameters,
      TestContext.Current.CancellationToken);

    Assert.Equal(3, result.TotalCount);
    Assert.Equal(2, result.Items.Count);
    Assert.Equal(2, result.TotalPages);
    Assert.Equal("Alpha", result.Items[0].Title);
    Assert.Equal("Beta", result.Items[1].Title);
  }

  [Fact]
  public async Task ToPaginationResultAsync_WhenSortFieldDoesNotExist_ShouldFallbackToDefaultSortField()
  {
    var source = new[]
    {
      new PaginationItem { Id = 3, Title = "Gamma" },
      new PaginationItem { Id = 1, Title = "Alpha" },
      new PaginationItem { Id = 2, Title = "Beta" }
    };

    var parameters = PaginationParameters<PaginationItemDto>.Create(pageNumber: 1, pageSize: 10, sortField: "MissingField", ascending: true);

    var result = await source.ToPaginationResultAsync(
      item => new PaginationItemDto { Id = item.Id, Title = item.Title },
      parameters,
      TestContext.Current.CancellationToken);

    Assert.Equal(PaginationParameters<PaginationItemDto>.DefaultSortField, result.SortField);
    Assert.Equal(1, result.Items[0].Id);
    Assert.Equal(2, result.Items[1].Id);
    Assert.Equal(3, result.Items[2].Id);
  }

  [Fact]
  public async Task ToPaginationResultAsync_WhenCalledForQueryableWithAsyncProvider_ShouldReturnPagedItems()
  {
    var options = new DbContextOptionsBuilder<PaginationTestDbContext>()
      .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
      .Options;

    await using var dbContext = new PaginationTestDbContext(options);
    dbContext.Items.AddRange(
      new PaginationItem { Id = 1, Title = "Alpha" },
      new PaginationItem { Id = 2, Title = "Beta" },
      new PaginationItem { Id = 3, Title = "Gamma" });
    await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var parameters = PaginationParameters<PaginationItemDto>.Create(pageNumber: 2, pageSize: 1, sortField: "Title", ascending: false);

    var result = await dbContext.Items.ToPaginationResultAsync(
      item => new PaginationItemDto { Id = item.Id, Title = item.Title },
      parameters,
      TestContext.Current.CancellationToken);

    Assert.Equal(3, result.TotalCount);
    Assert.Equal(3, result.TotalPages);
    Assert.Single(result.Items);
    Assert.Equal("Beta", result.Items[0].Title);
    Assert.True(result.HasPrevious);
    Assert.True(result.HasNext);
  }

  private sealed class PaginationTestDbContext(DbContextOptions<PaginationTestDbContext> options) : DbContext(options)
  {
    public DbSet<PaginationItem> Items => Set<PaginationItem>();
  }

  private sealed class PaginationItem
  {
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
  }

  private sealed class PaginationItemDto
  {
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
  }
}
