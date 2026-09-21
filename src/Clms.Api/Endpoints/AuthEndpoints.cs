using Clms.Api.Auth;
using Clms.Api.Data;
using Clms.Api.Domain;
using Clms.Shared;
using Microsoft.EntityFrameworkCore;

namespace Clms.Api.Endpoints;

public static class AuthEndpoints
{
    /// <summary>ConOps 6.2.6: lock the account after this many failed attempts.</summary>
    private const int MaxFailedAttempts = 5;

    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", async (
            LoginRequest request,
            ClmsDbContext db,
            JwtTokenService tokens,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("Auth");

            var user = await db.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username, ct);

            if (user is null)
                return Results.Unauthorized();

            if (user.IsLockedOut)
            {
                // ConOps 6.2.7: only a lab manager can clear a lockout.
                return Results.Problem(
                    title: "Account locked",
                    detail: "This account is locked. A laboratory manager must unlock it.",
                    statusCode: StatusCodes.Status423Locked);
            }

            if (!PasswordHasher.Verify(request.Password, user.PasswordHash))
            {
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= MaxFailedAttempts)
                {
                    user.IsLockedOut = true;

                    db.AuditEntries.Add(new AuditEntry
                    {
                        Actor = user.Username,
                        Action = "AccountLockedOut",
                        EntityName = nameof(AppUser),
                        EntityId = user.Id.ToString(),
                        Details = $"Locked after {MaxFailedAttempts} failed attempts."
                    });

                    // ConOps 6.2.6 also requires emailing the laboratory manager here.
                    // Wire in a transactional email provider (Brevo, SendGrid, SMTP)
                    // behind an INotificationService when you implement that.
                    logger.LogWarning("Account {Username} locked out", user.Username);
                }

                await db.SaveChangesAsync(ct);
                return Results.Unauthorized();
            }

            user.FailedLoginAttempts = 0;
            await db.SaveChangesAsync(ct);

            var (token, expires) = tokens.Create(user.Username, user.Role);
            return Results.Ok(new LoginResponse(token, user.Username, user.Role, expires));
        })
        .WithName("Login")
        .AllowAnonymous();

        // ConOps 6.2.7: lockout recovery is a laboratory-manager-only action.
        group.MapPost("/unlock/{username}", async (
            string username,
            ClmsDbContext db,
            CancellationToken ct) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username, ct);
            if (user is null) return Results.NotFound();

            user.IsLockedOut = false;
            user.FailedLoginAttempts = 0;
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("UnlockUser")
        .RequireAuthorization(ClmsRoles.LabManager);
    }
}
