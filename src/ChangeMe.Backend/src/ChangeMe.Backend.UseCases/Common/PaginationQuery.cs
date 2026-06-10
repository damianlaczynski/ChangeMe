using FastEndpoints;

namespace ChangeMe.Backend.UseCases.Common;

public abstract class PaginationQuery<TResult> : IQuery<PaginationResult<TResult>>
{
  [FromQuery]
  public PaginationParameters<TResult> PaginationParameters { get; set; } = new PaginationParameters<TResult>();
}
