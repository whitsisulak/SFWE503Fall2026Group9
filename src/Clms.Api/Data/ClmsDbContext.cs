using Clms.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Clms.Api.Data;

public sealed class ClmsDbContext(DbContextOptions<ClmsDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Reagent> Reagents => Set<Reagent>();
    public DbSet<TestOrder> TestOrders => Set<TestOrder>();
    public DbSet<TestResult> TestResults => Set<TestResult>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<AppUser>(e =>
        {
            e.HasIndex(x => x.Username).IsUnique();
            e.Property(x => x.Username).HasMaxLength(100);
            e.Property(x => x.Role).HasMaxLength(50);
        });

        b.Entity<Patient>(e =>
        {
            e.Property(x => x.FullName).HasMaxLength(200);
            e.Property(x => x.InsuranceProvider).HasMaxLength(200);
            e.Property(x => x.InsurancePolicyNumber).HasMaxLength(100);
        });

        b.Entity<Reagent>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.LotNumber).HasMaxLength(100);
            e.HasIndex(x => new { x.Name, x.LotNumber });
        });

        b.Entity<TestOrder>(e =>
        {
            // The instrument worker looks orders up by barcode on every inbound
            // message, so this index matters more than it looks.
            e.HasIndex(x => x.Barcode).IsUnique();
            e.Property(x => x.Barcode).HasMaxLength(64);
            e.Property(x => x.OrderingPhysician).HasMaxLength(200);
            e.Property(x => x.TestName).HasMaxLength(200);

            e.HasOne(x => x.Patient)
                .WithMany(p => p.TestOrders)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<TestResult>(e =>
        {
            e.Property(x => x.TestCode).HasMaxLength(50);
            e.Property(x => x.TestDescription).HasMaxLength(200);
            e.Property(x => x.Value).HasMaxLength(100);
            e.Property(x => x.InstrumentId).HasMaxLength(100);

            e.HasOne(x => x.TestOrder)
                .WithMany(o => o.Results)
                .HasForeignKey(x => x.TestOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<AuditEntry>(e =>
        {
            e.Property(x => x.Actor).HasMaxLength(100);
            e.Property(x => x.Action).HasMaxLength(100);
            e.HasIndex(x => x.TimestampUtc);
        });

        // Every entity assigns its own key in C# (Id = Guid.NewGuid()), so tell EF
        // not to expect the store to generate it. Otherwise a new child added through
        // a tracked navigation (order.Results.Add) arrives with a non-default key,
        // looks like an existing row, and EF issues an UPDATE instead of an INSERT.
        foreach (var entity in b.Model.GetEntityTypes())
            foreach (var key in entity.FindPrimaryKey()!.Properties)
                key.ValueGenerated = ValueGenerated.Never;
    }
}
