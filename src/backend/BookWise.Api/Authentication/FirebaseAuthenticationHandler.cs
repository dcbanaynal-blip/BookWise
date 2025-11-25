using System.Security.Claims;
using System.Text.Encodings.Web;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookWise.Api.Authentication;

public class FirebaseAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly FirebaseAuth _firebaseAuth;

    #pragma warning disable CS0618
    public FirebaseAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        FirebaseAuth firebaseAuth) : base(options, logger, encoder, clock)
    {
        _firebaseAuth = firebaseAuth;
    }
    #pragma warning restore CS0618

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            return AuthenticateResult.NoResult();
        }

        var headerValue = authorizationHeader.ToString();
        if (!headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.Fail("Invalid authorization header.");
        }

        var token = headerValue["Bearer ".Length..].Trim();
        if (string.IsNullOrEmpty(token))
        {
            return AuthenticateResult.Fail("Missing Firebase ID token.");
        }

        try
        {
            var decoded = await _firebaseAuth.VerifyIdTokenAsync(token);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, decoded.Uid),
            };

            if (decoded.Claims.TryGetValue("email", out var emailObj) && emailObj is string email)
            {
                claims.Add(new Claim(ClaimTypes.Email, email));
                claims.Add(new Claim(ClaimTypes.Name, email));
            }

            foreach (var claim in decoded.Claims)
            {
                if (claim.Value is string value)
                {
                    claims.Add(new Claim($"firebase:{claim.Key}", value));
                }
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
        catch (FirebaseAuthException ex)
        {
            Logger.LogWarning(ex, "Failed to verify Firebase token.");
            return AuthenticateResult.Fail("Invalid Firebase token.");
        }
    }
}
