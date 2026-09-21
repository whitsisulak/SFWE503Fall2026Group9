using Clms.Api.Auth;
using Clms.Api.Domain;
using Clms.Shared;
using Microsoft.EntityFrameworkCore;

namespace Clms.Api.Data;

public static class SeedData
{
    /// <summary>
    /// Creates the schema if needed and seeds demo data.
    ///
    /// NOTE: this uses EnsureCreated() rather than migrations so the scaffold runs
    /// with no extra tooling steps. Once your schema starts changing, switch to
    /// migrations: dotnet ef migrations add Initial -p src/Clms.Api
    /// and replace the EnsureCreatedAsync call with MigrateAsync.
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClmsDbContext>();

        await db.Database.EnsureCreatedAsync();

        if (await db.Users.AnyAsync()) return;

        // Demo accounts — one per role. Password for all: Passw0rd!
        const string demoPassword = "Passw0rd!";
        db.Users.AddRange(ClmsRoles.All.Select(role => new AppUser
        {
            Username = char.ToLowerInvariant(role[0]) + role[1..],
            PasswordHash = PasswordHasher.Hash(demoPassword),
            Role = role
        }));

        var reagents = new[]
        {
            new Reagent { Name = "T3 Uptake Reagent", LotNumber = "LOT-A1", QuantityOnHand = 48, ReorderThreshold = 20, ExpirationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(8)) },
            new Reagent { Name = "Urinalysis Strips",  LotNumber = "LOT-U7", QuantityOnHand =  9, ReorderThreshold = 25, ExpirationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(3)) },
            new Reagent { Name = "CBC Diluent",        LotNumber = "LOT-C3", QuantityOnHand = 120, ReorderThreshold = 40, ExpirationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(21)) },
            new Reagent { Name = "Glucose Calibrator", LotNumber = "LOT-G9", QuantityOnHand = 15, ReorderThreshold = 10, ExpirationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-4)) }
        };
        db.Reagents.AddRange(reagents);

        var patients = new[]
        {
            new Patient { FullName = "Alice Nguyen",  DateOfBirth = new DateOnly(1984, 3, 12), InsuranceProvider = "Blue Cross", InsurancePolicyNumber = "BC-99213" },
            new Patient { FullName = "Marcus Webb",   DateOfBirth = new DateOnly(1972, 11, 2), InsuranceProvider = "Aetna",      InsurancePolicyNumber = "AE-44120" }
        };
        db.Patients.AddRange(patients);

        // Barcodes 1234 / 5678 match the sample instrument files in instrument-drop/,
        // so the worker has something to attach results to on first run.
        db.TestOrders.AddRange(
            new TestOrder
            {
                Barcode = "1234",
                Patient = patients[0],
                PatientId = patients[0].Id,
                OrderingPhysician = "Dr. Patel",
                TestName = "Thyroid Panel",
                Status = TestOrderStatus.Collected,
                CollectedAtUtc = DateTime.UtcNow.AddHours(-3)
            },
            new TestOrder
            {
                Barcode = "5678",
                Patient = patients[1],
                PatientId = patients[1].Id,
                OrderingPhysician = "Dr. Okafor",
                TestName = "Urinalysis",
                Status = TestOrderStatus.Collected,
                CollectedAtUtc = DateTime.UtcNow.AddHours(-1)
            });

        await db.SaveChangesAsync();
    }
}
