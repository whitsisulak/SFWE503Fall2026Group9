using Clms.Shared;

namespace Clms.Ui.Services;

/// <summary>
/// The seam that makes these components host-agnostic.
///
/// Clms.Web (Blazor WebAssembly) and a future MAUI Blazor Hybrid shell both
/// register <see cref="HttpClmsApi"/>. A Blazor Server host could instead register
/// an implementation that talks to EF Core directly, with no HTTP hop — the
/// components would not change.
/// </summary>
public interface IClmsApi
{
    Task<LoginResponse?> LoginAsync(string username, string password, CancellationToken ct = default);

    Task<IReadOnlyList<ReagentDto>> GetReagentsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ReagentDto>> GetInventoryAlertsAsync(CancellationToken ct = default);
    Task<ReagentDto?> AdjustStockAsync(Guid reagentId, int delta, string reason, CancellationToken ct = default);

    Task<IReadOnlyList<TestOrderDto>> GetTestOrdersAsync(CancellationToken ct = default);
    Task<TestOrderDto?> GetTestOrderAsync(string barcode, CancellationToken ct = default);
    Task<TestOrderDto?> CollectSampleAsync(string barcode, CancellationToken ct = default);
}
