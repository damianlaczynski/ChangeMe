namespace ChangeMe.Backend.UnitTests;

public sealed class PaginationResultTests
{
  [Fact]
  public void Create_WhenCalledWithExplicitArguments_ShouldCalculateMetadata()
  {
    var items = new List<string> { "one", "two" };

    var result = PaginationResult<string>.Create(
      items,
      totalCount: 25,
      currentPage: 2,
      pageSize: 10,
      sortField: "CreatedAt",
      ascending: false);

    Assert.Equal(items, result.Items);
    Assert.Equal(25, result.TotalCount);
    Assert.Equal(3, result.TotalPages);
    Assert.Equal(2, result.CurrentPage);
    Assert.Equal(10, result.PageSize);
    Assert.Equal("CreatedAt", result.SortField);
    Assert.False(result.Ascending);
    Assert.True(result.HasPrevious);
    Assert.True(result.HasNext);
  }

  [Fact]
  public void Create_WhenCalledWithPaginationParameters_ShouldUseParameterValues()
  {
    var parameters = PaginationParameters<string>.Create(pageNumber: 3, pageSize: 5, sortField: "Title", ascending: true);
    var items = new List<string> { "one", "two", "three" };

    var result = PaginationResult<string>.Create(items, totalCount: 11, parameters);

    Assert.Equal(3, result.CurrentPage);
    Assert.Equal(5, result.PageSize);
    Assert.Equal("Title", result.SortField);
    Assert.True(result.Ascending);
    Assert.Equal(3, result.TotalPages);
    Assert.True(result.HasPrevious);
    Assert.False(result.HasNext);
  }

  [Fact]
  public void Empty_WhenCalled_ShouldReturnDefaultEmptyResult()
  {
    var result = PaginationResult<string>.Empty();

    Assert.Empty(result.Items);
    Assert.Equal(0, result.TotalCount);
    Assert.Equal(0, result.TotalPages);
    Assert.Equal(PaginationParameters<string>.DefaultPageNumber, result.CurrentPage);
    Assert.Equal(PaginationParameters<string>.DefaultPageSize, result.PageSize);
    Assert.Equal(PaginationParameters<string>.DefaultSortField, result.SortField);
    Assert.Equal(PaginationParameters<string>.DefaultAscending, result.Ascending);
    Assert.False(result.HasPrevious);
    Assert.False(result.HasNext);
  }
}
