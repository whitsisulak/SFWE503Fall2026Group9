using System.Net;
using System.Net.Http.Json;
using Clms.Shared;

namespace Clms.Ui.Services;

public sealed class HttpClmsApi(HttpClient http) : IClmsApi
{
    public async Task<LoginResponse?> LoginAsync(
        string username, string password, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync(
            "api/auth/login", new LoginRequest(username, password), ct);

        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<LoginResponse>(ct);
    }

    public async Task<IReadOnlyList<ReagentDto>> GetReagentsAsync(CancellationToken ct = default)
        => await http.GetFromJsonAsync<List<ReagentDto>>("api/reagents", ct) ?? [];

    public async Task<IReadOnlyList<ReagentDto>> GetInventoryAlertsAsync(CancellationToken ct = default)
        => await http.GetFromJsonAsync<List<ReagentDto>>("api/reagents/alerts", ct) ?? [];

    public async Task<ReagentDto?> AdjustStockAsync(
        Guid reagentId, int delta, string reason, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync(
            $"api/reagents/{reagentId}/adjust", new AdjustStockRequest(delta, reason), ct);

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<ReagentDto>(ct)
            : null;
    }

    public async Task<IReadOnlyList<TestOrderDto>> GetTestOrdersAsync(CancellationToken ct = default)
        => await http.GetFromJsonAsync<List<TestOrderDto>>("api/test-orders", ct) ?? [];

    public async Task<TestOrderDto?> GetTestOrderAsync(string barcode, CancellationToken ct = default)
    {
        var response = await http.GetAsync($"api/test-orders/{barcode}", ct);
        return response.StatusCode == HttpStatusCode.NotFound
            ? null
            : await response.Content.ReadFromJsonAsync<TestOrderDto>(ct);
    }

    public async Task<TestOrderDto?> CollectSampleAsync(string barcode, CancellationToken ct = default)
    {
        var response = await http.PostAsync($"api/test-orders/{barcode}/collect", null, ct);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<TestOrderDto>(ct)
            : null;
    }
}
