using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace Clms.Ui.Services;

/// <summary>
/// Minimal JWT-backed auth state. The token is kept in localStorage so a refresh
/// doesn't sign the user out, and its role claim drives &lt;AuthorizeView Roles="..."&gt;
/// in the shared components — which is how RBAC (ConOps 6.1.3) surfaces in the UI.
///
/// The server re-checks every role on every request; this only controls what the
/// UI offers, never what it is allowed to do.
/// </summary>
public sealed class ClmsAuthStateProvider(IJSRuntime js, HttpClient http)
    : AuthenticationStateProvider
{
    private const string StorageKey = "clms.token";
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public string? CurrentUsername { get; private set; }
    public string? CurrentRole { get; private set; }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string? token = null;
        try
        {
            token = await js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
        }
        catch (JSException)
        {
            // Storage can be unavailable (private mode, blocked site data).
            // Treat that as "not signed in" rather than failing the render.
        }

        if (string.IsNullOrWhiteSpace(token)) return Anonymous;

        var claims = ParseClaims(token).ToList();
        if (claims.Count == 0) return Anonymous;

        // Expired token? Drop it rather than sending it and getting 401s.
        var exp = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
        if (long.TryParse(exp, out var expSeconds) &&
            DateTimeOffset.FromUnixTimeSeconds(expSeconds) <= DateTimeOffset.UtcNow)
        {
            await SignOutAsync();
            return Anonymous;
        }

        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var identity = new ClaimsIdentity(claims, authenticationType: "jwt");
        CurrentUsername = identity.FindFirst(ClaimTypes.Name)?.Value;
        CurrentRole = identity.FindFirst(ClaimTypes.Role)?.Value;

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task SignInAsync(string token)
    {
        await js.InvokeVoidAsync("localStorage.setItem", StorageKey, token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task SignOutAsync()
    {
        await js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
        http.DefaultRequestHeaders.Authorization = null;
        CurrentUsername = null;
        CurrentRole = null;
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }

    private static IEnumerable<Claim> ParseClaims(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length != 3) yield break;

        Dictionary<string, JsonElement>? payload;
        try
        {
            payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                Base64UrlDecode(parts[1]));
        }
        catch (Exception e) when (e is JsonException or FormatException)
        {
            yield break;
        }

        if (payload is null) yield break;

        foreach (var (key, value) in payload)
        {
            // Normalize the two claim types the UI actually reads.
            var type = key switch
            {
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" => ClaimTypes.Name,
                "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" => ClaimTypes.Role,
                "unique_name" => ClaimTypes.Name,
                "role" => ClaimTypes.Role,
                _ => key
            };

            if (value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in value.EnumerateArray())
                    yield return new Claim(type, item.ToString());
            }
            else
            {
                yield return new Claim(type, value.ToString());
            }
        }
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var s = input.Replace('-', '+').Replace('_', '/');
        return Convert.FromBase64String(s.PadRight(s.Length + (4 - s.Length % 4) % 4, '='));
    }
}
