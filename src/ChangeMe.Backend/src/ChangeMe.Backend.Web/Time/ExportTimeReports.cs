using System.Net.Mime;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Time;
using ChangeMe.Backend.UseCases.Time.Dtos;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ChangeMe.Backend.Web.Time;

public class ExportTimeReports(IMediator mediator) : Endpoint<ExportTimeReportsQuery>
{
  public override void Configure()
  {
    Get("/time/reports/export");
    AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    Policies(PermissionCodes.TimeViewReports);
    Summary(s => s.Summary = "Export time report as CSV");
  }

  public override async Task HandleAsync(ExportTimeReportsQuery req, CancellationToken ct)
  {
    var result = await mediator.Send(req, ct);
    if (!result.IsSuccess)
    {
      await HttpContext.SendResultAsync(result, ct);
      return;
    }

    var export = result.Value;
    var response = HttpContext.Response;
    response.ContentType = "text/csv; charset=utf-8";
    response.Headers.ContentDisposition = new ContentDisposition
    {
      FileName = export.FileName,
      DispositionType = DispositionTypeNames.Attachment
    }.ToString();

    await response.Body.WriteAsync(export.Content, ct);
  }
}
