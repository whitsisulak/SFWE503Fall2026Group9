namespace Clms.Api.Domain;

public sealed class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public required string Role { get; set; }

    /// <summary>ConOps 6.2.6: lock out after 5 consecutive failed attempts.</summary>
    public int FailedLoginAttempts { get; set; }
    public bool IsLockedOut { get; set; }
}

public sealed class Patient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string FullName { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string? InsuranceProvider { get; set; }
    public string? InsurancePolicyNumber { get; set; }

    public List<TestOrder> TestOrders { get; set; } = [];
}

public sealed class Reagent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string LotNumber { get; set; }
    public int QuantityOnHand { get; set; }
    public int ReorderThreshold { get; set; }
    public DateOnly ExpirationDate { get; set; }
}

public enum TestOrderStatus
{
    Ordered = 0,
    Collected = 1,
    InProgress = 2,
    Resulted = 3,
    Validated = 4
}

public sealed class TestOrder
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The barcode printed at collection (ConOps 6.3.4) and echoed back by the
    /// instrument in its result file (ConOps 6.3.5). This is the join key between
    /// a physical sample and its machine results.
    /// </summary>
    public required string Barcode { get; set; }

    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }

    public required string OrderingPhysician { get; set; }
    public required string TestName { get; set; }

    public TestOrderStatus Status { get; set; } = TestOrderStatus.Ordered;
    public DateTime OrderedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CollectedAtUtc { get; set; }

    public List<TestResult> Results { get; set; } = [];
}

public sealed class TestResult
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TestOrderId { get; set; }
    public TestOrder? TestOrder { get; set; }

    public required string TestCode { get; set; }
    public required string TestDescription { get; set; }

    /// <summary>
    /// Kept as text deliberately: instruments emit both quantitative ("28")
    /// and qualitative ("Absent") values in the same field.
    /// </summary>
    public required string Value { get; set; }

    public DateTime ResultedAtUtc { get; set; }
    public string? InstrumentId { get; set; }
}

/// <summary>ConOps 6.3.6 / 6.5.1: activity and transaction logging.</summary>
public sealed class AuditEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public required string Actor { get; set; }
    public required string Action { get; set; }
    public string? EntityName { get; set; }
    public string? EntityId { get; set; }
    public string? Details { get; set; }
}
