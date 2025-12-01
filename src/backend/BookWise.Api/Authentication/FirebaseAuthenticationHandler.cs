using System.Security.Claims;
using System.Text.Encodings.Web;
using BookWise.Infrastructure.Persistence;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookWise.Api.Authentication;

public class FirebaseAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly FirebaseAuth _firebaseAuth;
    private readonly BookWiseDbContext _dbContext;

    #pragma warning disable CS0618
    public FirebaseAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        FirebaseAuth firebaseAuth,
        BookWiseDbContext dbContext) : base(options, logger, encoder, clock)
    {
        _firebaseAuth = firebaseAuth;
        _dbContext = dbContext;
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

            if (!decoded.Claims.TryGetValue("email", out var emailObj) || emailObj is not string email)
            {
                Logger.LogWarning("Firebase token for UID {Uid} did not include an email claim.", decoded.Uid);
                return AuthenticateResult.Fail("Firebase token is missing an email claim.");
            }

            var normalizedEmail = NormalizeEmail(email);
            var userEmail = await _dbContext.UserEmails
                .Include(ue => ue.User)
                .SingleOrDefaultAsync(
                    ue => ue.Email == normalizedEmail,
                    Context.RequestAborted);

            if (userEmail is null)
            {
                Logger.LogWarning("Firebase user {Email} is not allowlisted in BookWise.", normalizedEmail);
                return AuthenticateResult.Fail("User is not authorized to access BookWise.");
            }

            var appUser = userEmail.User;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, appUser.UserId.ToString()),
                new Claim(ClaimTypes.Role, appUser.Role),
                new Claim("bookwise:userId", appUser.UserId.ToString()),
                new Claim("firebase:uid", decoded.Uid)
            };

            claims.Add(new Claim(ClaimTypes.Email, normalizedEmail));
            claims.Add(new Claim(ClaimTypes.Name, normalizedEmail));

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

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
