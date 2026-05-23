using ChangeMe.Backend.Domain.Aggregates.Auth;
using ChangeMe.Backend.Domain.Interfaces;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Auth.Utils;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record GetInvitationPreviewQuery(string Token) : IQuery<InvitationPreviewDto>;

public class GetInvitationPreviewHandler(
  ApplicationDbContext context,
  IUserAuthTokenService tokenService) : IQueryHandler<GetInvitationPreviewQuery, InvitationPreviewDto>
{
  public async Task<Result<InvitationPreviewDto>> Handle(
    GetInvitationPreviewQuery query,
    CancellationToken cancellationToken)
  {
    var validateResult = await tokenService.ValidateTokenAsync(
      query.Token,
      UserAuthTokenType.Invitation,
      cancellationToken);

    if (!validateResult.IsSuccess)
    {
      return Result.Success(new InvitationPreviewDto { IsValid = false });
    }

    var user = await context.Users
      .AsNoTracking()
      .FirstOrDefaultAsync(x => x.Id == validateResult.Value, cancellationToken);

    if (user is null || user.HasPasswordSet)
      return Result.Success(new InvitationPreviewDto { IsValid = false });

    return Result.Success(new InvitationPreviewDto
    {
      IsValid = true,
      Email = user.Email,
      FirstName = user.FirstName,
      LastName = user.LastName
    });
  }
}
