namespace Clms.Shared;

/// <summary>
/// Role names used for RBAC. These mirror the user types in CLMS ConOps 6.1.3.
/// </summary>
public static class ClmsRoles
{
    public const string LabManager = "LabManager";
    public const string SpecimenCollector = "SpecimenCollector";
    public const string LabTechnician = "LabTechnician";
    public const string Cashier = "Cashier";
    public const string HealthcareProvider = "HealthcareProvider";
    public const string Patient = "Patient";

    public static readonly string[] All =
    [
        LabManager, SpecimenCollector, LabTechnician, Cashier, HealthcareProvider, Patient
    ];
}

public sealed record LoginRequest(string Username, string Password);

public sealed record LoginResponse(string Token, string Username, string Role, DateTime ExpiresAtUtc);

public sealed record ReagentDto(
    Guid Id,
    string Name,
    string LotNumber,
    int QuantityOnHand,
    int ReorderThreshold,
    DateOnly ExpirationDate)
{
    /// <summary>ConOps 6.4.2: flag stock below the reorder threshold.</summary>
    public bool IsLowStock => QuantityOnHand < ReorderThreshold;

    /// <summary>ConOps 6.4.6: warn on reagents expiring within 30 days.</summary>
    public bool IsExpiringSoon =>
        ExpirationDate <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));

    public bool IsExpired => ExpirationDate < DateOnly.FromDateTime(DateTime.UtcNow);
}

public sealed record AdjustStockRequest(int Delta, string Reason);

public sealed record TestOrderDto(
    Guid Id,
    string Barcode,
    string PatientName,
    string OrderingPhysician,
    string TestName,
    string Status,
    DateTime OrderedAtUtc,
    DateTime? CollectedAtUtc,
    IReadOnlyList<TestResultDto> Results);

public sealed record TestResultDto(
    Guid Id,
    string TestCode,
    string TestDescription,
    string Value,
    DateTime ResultedAtUtc,
    string? InstrumentId);

public sealed record CreateTestOrderRequest(
    Guid PatientId,
    string OrderingPhysician,
    string TestName);

public sealed record PatientDto(
    Guid Id,
    string FullName,
    DateOnly DateOfBirth,
    string? InsuranceProvider,
    string? InsurancePolicyNumber);
