using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MemberCare.Web.Models;

namespace MemberCare.Web.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly AppState _state;
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public ApiClient(HttpClient http, AppState state)
    {
        _http = http;
        _state = state;
    }

    private void Authorize()
    {
        if (!string.IsNullOrEmpty(_state.Token))
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _state.Token);
    }

    public async Task<AuthTokenResponse?> LoginAsync(string username)
    {
        var response = await _http.PostAsJsonAsync("auth/login", new { username, password = "test" });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthTokenResponse>(_json);
    }

    public async Task<List<MemberDto>> GetMembersAsync()
    {
        Authorize();
        var result = await _http.GetFromJsonAsync<ItemsResponse<MemberDto>>("members?page=1&pageSize=200", _json);
        return result?.Items ?? [];
    }

    public async Task<bool> CreateMemberAsync(object body)
    {
        Authorize();
        return (await _http.PostAsJsonAsync("members", body)).IsSuccessStatusCode;
    }

    public async Task<List<VisitorDto>> GetVisitorsAsync()
    {
        Authorize();
        var result = await _http.GetFromJsonAsync<ItemsResponse<VisitorDto>>("visitors", _json);
        return result?.Items ?? [];
    }

    public async Task<bool> CreateVisitorAsync(object body)
    {
        Authorize();
        return (await _http.PostAsJsonAsync("visitors", body)).IsSuccessStatusCode;
    }

    public async Task<bool> ConvertVisitorAsync(Guid visitorId)
    {
        Authorize();
        return (await _http.PostAsync($"visitors/{visitorId}/convert", null)).IsSuccessStatusCode;
    }

    public async Task<List<NewConvertDto>> GetConvertsAsync()
    {
        Authorize();
        var result = await _http.GetFromJsonAsync<ItemsResponse<NewConvertDto>>("new-converts", _json);
        return result?.Items ?? [];
    }

    public async Task<bool> CreateConvertAsync(object body)
    {
        Authorize();
        return (await _http.PostAsJsonAsync("new-converts", body)).IsSuccessStatusCode;
    }

    public async Task<List<AttendanceSessionDto>> GetSessionsAsync()
    {
        Authorize();
        var result = await _http.GetFromJsonAsync<ItemsResponse<AttendanceSessionDto>>("attendance/sessions", _json);
        return result?.Items ?? [];
    }

    public async Task<bool> CreateSessionAsync(object body)
    {
        Authorize();
        return (await _http.PostAsJsonAsync("attendance/sessions", body)).IsSuccessStatusCode;
    }

    public async Task<bool> CreateAttendanceRecordAsync(object body)
    {
        Authorize();
        return (await _http.PostAsJsonAsync("attendance/records", body)).IsSuccessStatusCode;
    }

    public async Task<List<FollowUpDto>> GetFollowUpsAsync()
    {
        Authorize();
        var result = await _http.GetFromJsonAsync<ItemsResponse<FollowUpDto>>("follow-up/records", _json);
        return result?.Items ?? [];
    }

    public async Task<bool> CreateFollowUpAsync(object body)
    {
        Authorize();
        return (await _http.PostAsJsonAsync("follow-up/records", body)).IsSuccessStatusCode;
    }

    public async Task<DashboardSummaryDto?> GetDashboardAsync()
    {
        Authorize();
        return await _http.GetFromJsonAsync<DashboardSummaryDto>("dashboard/summary", _json);
    }
}
