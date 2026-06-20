namespace ChangeMe.Backend.UnitTests;

public sealed class PaginationParametersTests
{
  [Theory]
  [InlineData(0, PaginationParameters<string>.DefaultPageSize)]
  [InlineData(-5, PaginationParameters<string>.DefaultPageSize)]
  [InlineData(101, 100)]
  [InlineData(150, 100)]
  public void PageSize_WhenValueIsOutsideAllowedRange_ShouldNormalizeToExpectedValue(int pageSize, int expected)
  {
    var parameters = new PaginationParameters<string>
    {
      PageSize = pageSize
    };

    Assert.Equal(expected, parameters.PageSize);
  }

  [Theory]
  [InlineData(0, PaginationParameters<string>.DefaultPageNumber)]
  [InlineData(-10, PaginationParameters<string>.DefaultPageNumber)]
  [InlineData(3, 3)]
  public void Validate_WhenPageNumberIsInvalid_ShouldNormalizePageNumber(int pageNumber, int expected)
  {
    var parameters = new PaginationParameters<string>
    {
      PageNumber = pageNumber
    };

    parameters.Validate();

    Assert.Equal(expected, parameters.PageNumber);
  }

  [Theory]
  [InlineData(null, PaginationParameters<string>.DefaultSortField)]
  [InlineData("", PaginationParameters<string>.DefaultSortField)]
  [InlineData(" ", PaginationParameters<string>.DefaultSortField)]
  [InlineData("CreatedAt", "CreatedAt")]
  public void Validate_WhenSortFieldIsEmpty_ShouldUseDefaultSortField(string? sortField, string expected)
  {
    var parameters = new PaginationParameters<string>
    {
      SortField = sortField!
    };

    parameters.Validate();

    Assert.Equal(expected, parameters.SortField);
  }

  [Fact]
  public void Create_WhenCalled_ShouldReturnValidatedParameters()
  {
    var result = PaginationParameters<string>.Create(pageNumber: 0, pageSize: 500, sortField: "", ascending: false);

    Assert.Equal(PaginationParameters<string>.DefaultPageNumber, result.PageNumber);
    Assert.Equal(100, result.PageSize);
    Assert.Equal(PaginationParameters<string>.DefaultSortField, result.SortField);
    Assert.False(result.Ascending);
  }
}
