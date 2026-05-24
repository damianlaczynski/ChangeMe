using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.Infrastructure.Auth.Passkey;
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

    var twoFactor = auth.TwoFactor;
    var configuredProviders = auth.ExternalProviders
      .Where(x => x.IsConfigured)
      .Select(x => new ExternalProviderSettingsDto
      {
        ProviderKey = x.ProviderKey.Trim(),
        DisplayName = x.DisplayName.Trim()
      })
      .ToList();

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
      MaximumPasswordAgeDays = auth.MaximumPasswordAgeDays,
      TwoFactorAuthenticationEnabled = auth.TwoFactorAuthenticationEnabled,
      TwoFactorAuthenticationRequired = auth.TwoFactorAuthenticationRequired,
      TrustIdentityProviderMfa = auth.TrustIdentityProviderMfa
        && auth.TwoFactorAuthenticationEnabled
        && auth.ExternalProvidersEnabled,
      ExternalProvidersEnabled = auth.ExternalProvidersEnabled && configuredProviders.Count > 0,
      TwoFactor = new TwoFactorSettingsDto
      {
        VerificationCodeLength = twoFactor.VerificationCodeLength,
        RecoveryCodeCount = twoFactor.RecoveryCodeCount,
        TotpTimeStepSeconds = twoFactor.TotpTimeStepSeconds,
        StepUpExternalSignInValidityMinutes = twoFactor.StepUpExternalSignInValidityMinutes
      },
      ExternalProviders = configuredProviders,
      Passkeys = new PasskeySettingsDto
      {
        PasskeysAuthenticationEnabled = auth.Passkeys.PasskeysAuthenticationEnabled,
        PasskeysAuthenticationRequired = auth.Passkeys.PasskeysAuthenticationRequired,
        PasskeySatisfiesTwoFactor = auth.Passkeys.PasskeySatisfiesTwoFactor
          && auth.TwoFactorAuthenticationEnabled,
        DiscoverablePasskeySignInOnLogin = auth.Passkeys.DiscoverablePasskeySignInOnLogin,
        RelyingPartyId = PasskeyFido2Service.ResolveRpId(auth),
        RelyingPartyDisplayName = auth.Passkeys.RelyingPartyDisplayName,
        MaximumPasskeysPerUser = auth.Passkeys.MaximumPasskeysPerUser
      }
    };

    return Task.FromResult(Result.Success(dto));
  }
}
