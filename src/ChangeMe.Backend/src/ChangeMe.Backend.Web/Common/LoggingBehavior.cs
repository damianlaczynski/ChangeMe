using System.Diagnostics;
using System.Reflection;

namespace ChangeMe.Backend.Web.Common;

/// <summary>
/// Adds logging for all requests in the Mediator pipeline.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IMessage
{
  private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

  public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
  {
    _logger = logger;
  }

  public async ValueTask<TResponse> Handle(
    TRequest request,
    MessageHandlerDelegate<TRequest, TResponse> next,
    CancellationToken cancellationToken)
  {
    if (request is null)
      throw new ArgumentNullException(nameof(request));

    if (_logger.IsEnabled(LogLevel.Information))
    {
      _logger.LogInformation("Handling {RequestName}", typeof(TRequest).Name);

      Type requestType = request.GetType();
      IList<PropertyInfo> props = new List<PropertyInfo>(requestType.GetProperties());
      foreach (PropertyInfo prop in props)
      {
        object? propValue = prop.GetValue(request, null);
        _logger.LogInformation("Property {Property} : {@Value}", prop.Name, propValue);
      }
    }

    var sw = Stopwatch.StartNew();

    var response = await next(request, cancellationToken);

    _logger.LogInformation(
      "Handled {RequestName} with {Response} in {ElapsedMs} ms",
      typeof(TRequest).Name,
      response,
      sw.ElapsedMilliseconds);
    sw.Stop();
    return response;
  }
}
