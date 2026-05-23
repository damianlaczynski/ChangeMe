using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record GetAuthSettingsQuery(bool DoNothing = false) : IQuery<AuthSettingsDto>;

public class GetAuthSettingsHandler(IOptions<AuthOptions> options)
  : IQueryHandler<GetAuthSettingsQuery, AuthSettingsDto>
{
  public Task<Result<AuthSettingsDto>> Handle(GetAuthSettingsQuery query, CancellationToken cancellationToken)
  {
    var auth = options.Value;
    var policy = auth.PasswordPolicy;

    var dto = new AuthSettingsDto
    {
      PasswordPolicy = new PasswordPolicySettingsDto
      {
        MinimumLength = policy.MinimumLength,
        MaximumLength = policy.MaximumLength,
        RequireUppercase = policy.RequireUppercase,
        RequireLowercase = policy.RequireLowercase,
        RequireDigit = policy.RequireDigit,
        RequireSpecialCharacter = policy.RequireSpecialCharacter
      },
      PublicRegistrationEnabled = auth.PublicRegistrationEnabled,
      EmailVerificationEnabled = auth.EmailVerificationEnabled,
      PasswordExpirationEnabled = auth.PasswordExpirationEnabled,
      MaximumPasswordAgeDays = auth.MaximumPasswordAgeDays
    };

    return Task.FromResult(Result.Success(dto));
  }
}
