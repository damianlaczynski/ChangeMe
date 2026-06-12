using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.Infrastructure.Auth.Passkey;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record GetAuthSettingsQuery() : IQuery<AuthSettingsDto>;

public class GetAuthSettingsHandler(IOptions<AuthOptions> options)
  : IQueryHandler<GetAuthSettingsQuery, AuthSettingsDto>
{
  public ValueTask<Result<AuthSettingsDto>> Handle(GetAuthSettingsQuery query, CancellationToken cancellationToken)
  {
    var auth = options.Value;
    var policy = auth.PasswordPolicy;

    var twoFactor = auth.TwoFactor;
    var configuredProviders = auth.External.Providers
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
      PublicRegistrationEnabled = auth.Registration.PublicEnabled,
      EmailVerificationEnabled = auth.EmailVerification.Enabled,
      PasswordExpirationEnabled = auth.PasswordExpiration.Enabled,
      MaximumPasswordAgeDays = auth.PasswordExpiration.MaximumPasswordAgeDays,
      TwoFactorAuthenticationEnabled = auth.TwoFactor.Enabled,
      TwoFactorAuthenticationRequired = auth.TwoFactor.Required,
      TrustIdentityProviderMfa = auth.TwoFactor.TrustIdentityProviderMfa
        && auth.TwoFactor.Enabled
        && auth.External.Enabled,
      ExternalProvidersEnabled = auth.External.Enabled && configuredProviders.Count > 0,
      ExternalProviderLinkingEnabled = auth.External.Enabled
        && auth.External.LinkingEnabled
        && configuredProviders.Count > 0,
      SelfServiceEmailChangeEnabled = auth.EmailChange.Enabled,
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
          && auth.TwoFactor.Enabled,
        DiscoverablePasskeySignInOnLogin = auth.Passkeys.DiscoverablePasskeySignInOnLogin,
        OfferPasskeyEnrollmentPrompt = auth.Passkeys.PasskeysAuthenticationEnabled
          && auth.Passkeys.OfferPasskeyEnrollmentPrompt,
        RelyingPartyId = PasskeyFido2Service.ResolveRpId(auth),
        RelyingPartyDisplayName = auth.Passkeys.RelyingPartyDisplayName,
        MaximumPasskeysPerUser = auth.Passkeys.MaximumPasskeysPerUser
      }
    };

    return ValueTask.FromResult(Result.Success(dto));
  }
}
