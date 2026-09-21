using System.Security.Claims;
using Clms.Api.Data;
using Clms.Api.Domain;
using Clms.Shared;
using Microsoft.EntityFrameworkCore;

namespace Clms.Api.Endpoints;

public static class TestOrderEndpoints
{
    public static void MapTestOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/test-orders")
            .WithTags("Test Orders")
            .RequireAuthorization();

        group.MapGet("/", async (ClmsDbContext db, CancellationToken ct) =>
        {
            var orders = await db.TestOrders
                .Include(o => o.Patient)
                .Include(o => o.Results)
                .OrderByDescending(o => o.OrderedAtUtc)
                .ToListAsync(ct);

            return Results.Ok(orders.Select(ToDto).ToList());
        })
        .WithName("GetTestOrders");

        group.MapGet("/{barcode}", async (
            string barcode, ClmsDbContext db, CancellationToken ct) =>
        {
            var order = await db.TestOrders
                .Include(o => o.Patient)
                .Include(o => o.Results)
                .FirstOrDefaultAsync(o => o.Barcode == barcode, ct);

            return order is null ? Results.NotFound() : Results.Ok(ToDto(order));
        })
        .WithName("GetTestOrderByBarcode");

        group.MapPost("/", async (
            CreateTestOrderRequest request,
            ClmsDbContext db,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var patient = await db.Patients.FindAsync([request.PatientId], ct);
            if (patient is null) return Results.BadRequest("Unknown patient.");

            var order = new TestOrder
            {
                Barcode = await GenerateBarcodeAsync(db, ct),
                PatientId = patient.Id,
                OrderingPhysician = request.OrderingPhysician,
                TestName = request.TestName
            };

            db.TestOrders.Add(order);
            db.AuditEntries.Add(new AuditEntry
            {
                Actor = user.Identity?.Name ?? "unknown",
                Action = "CreateTestOrder",
                EntityName = nameof(TestOrder),
                EntityId = order.Id.ToString(),
                Details = $"{order.TestName} for patient {patient.FullName}, barcode {order.Barcode}"
            });

            await db.SaveChangesAsync(ct);
            await db.Entry(order).Reference(o => o.Patient).LoadAsync(ct);

            return Results.Created($"/api/test-orders/{order.Barcode}", ToDto(order));
        })
        .WithName("CreateTestOrder");

        // ConOps 6.3.4: only specimen collectors mark a sample collected / print its barcode.
        group.MapPost("/{barcode}/collect", async (
            string barcode,
            ClmsDbContext db,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var order = await db.TestOrders
                .Include(o => o.Patient)
                .Include(o => o.Results)
                .FirstOrDefaultAsync(o => o.Barcode == barcode, ct);

            if (order is null) return Results.NotFound();

            if (order.CollectedAtUtc is not null)
                return Results.Conflict("Sample has already been collected.");

            order.CollectedAtUtc = DateTime.UtcNow;
            order.Status = TestOrderStatus.Collected;

            db.AuditEntries.Add(new AuditEntry
            {
                Actor = user.Identity?.Name ?? "unknown",
                Action = "CollectSample",
                EntityName = nameof(TestOrder),
                EntityId = order.Id.ToString(),
                Details = $"Barcode {order.Barcode} collected."
            });

            await db.SaveChangesAsync(ct);
            return Results.Ok(ToDto(order));
        })
        .WithName("CollectSample")
        .RequireAuthorization(ClmsRoles.SpecimenCollector);
    }

    private static async Task<string> GenerateBarcodeAsync(ClmsDbContext db, CancellationToken ct)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var candidate = Random.Shared.Next(100_000, 999_999).ToString();
            if (!await db.TestOrders.AnyAsync(o => o.Barcode == candidate, ct))
                return candidate;
        }

        // Fall back to something guaranteed unique rather than looping forever.
        return Guid.NewGuid().ToString("N")[..10];
    }

    private static TestOrderDto ToDto(TestOrder o) => new(
        o.Id,
        o.Barcode,
        o.Patient?.FullName ?? "(unknown)",
        o.OrderingPhysician,
        o.TestName,
        o.Status.ToString(),
        o.OrderedAtUtc,
        o.CollectedAtUtc,
        o.Results
            .OrderBy(r => r.ResultedAtUtc)
            .Select(r => new TestResultDto(
                r.Id, r.TestCode, r.TestDescription, r.Value, r.ResultedAtUtc, r.InstrumentId))
            .ToList());
}
