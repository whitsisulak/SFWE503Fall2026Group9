using System.Security.Claims;
using Clms.Api.Data;
using Clms.Api.Domain;
using Clms.Shared;
using Microsoft.EntityFrameworkCore;

namespace Clms.Api.Endpoints;

public static class InventoryEndpoints
{
    public static void MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reagents")
            .WithTags("Inventory")
            .RequireAuthorization();

        group.MapGet("/", async (ClmsDbContext db, CancellationToken ct) =>
        {
            var reagents = await db.Reagents
                .OrderBy(r => r.Name)
                .Select(r => new ReagentDto(
                    r.Id, r.Name, r.LotNumber, r.QuantityOnHand, r.ReorderThreshold, r.ExpirationDate))
                .ToListAsync(ct);

            return Results.Ok(reagents);
        })
        .WithName("GetReagents");

        // ConOps 6.4.2 — low stock, and 6.4.6 — expiring within 30 days.
        group.MapGet("/alerts", async (ClmsDbContext db, CancellationToken ct) =>
        {
            var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));

            var alerts = await db.Reagents
                .Where(r => r.QuantityOnHand < r.ReorderThreshold || r.ExpirationDate <= cutoff)
                .OrderBy(r => r.ExpirationDate)
                .Select(r => new ReagentDto(
                    r.Id, r.Name, r.LotNumber, r.QuantityOnHand, r.ReorderThreshold, r.ExpirationDate))
                .ToListAsync(ct);

            return Results.Ok(alerts);
        })
        .WithName("GetInventoryAlerts");

        group.MapPost("/{id:guid}/adjust", async (
            Guid id,
            AdjustStockRequest request,
            ClmsDbContext db,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var reagent = await db.Reagents.FindAsync([id], ct);
            if (reagent is null) return Results.NotFound();

            var newQuantity = reagent.QuantityOnHand + request.Delta;
            if (newQuantity < 0)
                return Results.BadRequest("Adjustment would drive quantity below zero.");

            reagent.QuantityOnHand = newQuantity;

            db.AuditEntries.Add(new AuditEntry
            {
                Actor = user.Identity?.Name ?? "unknown",
                Action = "AdjustStock",
                EntityName = nameof(Reagent),
                EntityId = reagent.Id.ToString(),
                Details = $"Delta {request.Delta:+#;-#;0} -> {newQuantity}. {request.Reason}"
            });

            await db.SaveChangesAsync(ct);

            return Results.Ok(new ReagentDto(
                reagent.Id, reagent.Name, reagent.LotNumber,
                reagent.QuantityOnHand, reagent.ReorderThreshold, reagent.ExpirationDate));
        })
        .WithName("AdjustReagentStock")
        // ConOps 6.4.3 / 6.4.5: inventory changes are a laboratory-manager action.
        .RequireAuthorization(ClmsRoles.LabManager);
    }
}
