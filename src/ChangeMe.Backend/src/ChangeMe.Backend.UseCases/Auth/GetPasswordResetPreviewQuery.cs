using ChangeMe.Backend.UseCases.Auth.Dtos;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record GetPasswordResetPreviewQuery(string Token) : IQuery<PasswordResetPreviewDto>;

public class GetPasswordResetPreviewHandler(
  IUserAuthTokenService tokenService) : IQueryHandler<GetPasswordResetPreviewQuery, PasswordResetPreviewDto>
{
  public async Task<Result<PasswordResetPreviewDto>> Handle(
    GetPasswordResetPreviewQuery query,
    CancellationToken cancellationToken)
  {
    var validateResult = await tokenService.ValidateTokenAsync(
      query.Token,
      UserAuthTokenType.PasswordReset,
      cancellationToken);

    return Result.Success(new PasswordResetPreviewDto
    {
      IsValid = validateResult.IsSuccess
    });
  }
}
