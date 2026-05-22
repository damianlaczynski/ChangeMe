using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ChangeMe.Backend.Web.Common;

public abstract class BaseEndpoint<TRequest, TResponse>(IMediator mediator) : Endpoint<TRequest, Result<TResponse>>
    where TRequest : notnull
{
  public override void Configure()
  {
    DontThrowIfValidationFails();
    AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    ConfigureEndpoint();
  }

  protected virtual void ConfigureEndpoint()
  {
  }

  protected void RequirePermission(string permissionCode) => Policies(permissionCode);

  protected void RequirePermissions(params string[] permissionCodes) => Policies(permissionCodes);

  public override async Task HandleAsync(TRequest req, CancellationToken ct)
  {
    Result<TResponse> response = ValidationFailed switch
    {
      true => Result<TResponse>.Invalid(ValidationFailures.Select(f => new ValidationError
      {
        Identifier = f.PropertyName,
        ErrorMessage = f.ErrorMessage,
        Severity = ValidationSeverity.Error
      }).ToArray()),
      false => await mediator.Send(req, ct) as Result<TResponse> ?? Result<TResponse>.Error("Unknown error")
    };

    var statusCode = response.Status switch
    {
      ResultStatus.Ok => StatusCodes.Status200OK,
      ResultStatus.Created => StatusCodes.Status201Created,
      ResultStatus.Error => StatusCodes.Status400BadRequest,
      ResultStatus.Forbidden => StatusCodes.Status403Forbidden,
      ResultStatus.Unauthorized => StatusCodes.Status401Unauthorized,
      ResultStatus.Invalid => StatusCodes.Status400BadRequest,
      ResultStatus.NotFound => StatusCodes.Status404NotFound,
      ResultStatus.NoContent => StatusCodes.Status204NoContent,
      ResultStatus.Conflict => StatusCodes.Status409Conflict,
      ResultStatus.CriticalError => StatusCodes.Status500InternalServerError,
      ResultStatus.Unavailable => StatusCodes.Status503ServiceUnavailable,
      _ => StatusCodes.Status500InternalServerError
    };
    try
    {
      if (statusCode == StatusCodes.Status204NoContent)
      {
        await HttpContext.Response.SendNoContentAsync(ct);
        return;
      }

      await HttpContext.Response.SendAsync(response, statusCode, cancellation: ct);
    }
    catch (OperationCanceledException)
    {
      // Client disconnected or request was cancelled - this is expected behavior
      // No need to log or handle further as the client is no longer listening
    }
    return;
  }
}
