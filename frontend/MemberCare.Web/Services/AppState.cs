namespace MemberCare.Web.Services;

public class AppState
{
    public string Role { get; set; } = "church_admin";
    public string Branch { get; set; } = "Central";
    public string Token { get; set; } = "";
    public string BranchId { get; set; } = "";
    public bool IsConnected { get; set; } = false;
    public string? StatusMessage { get; set; }

    public event Action? OnChange;
    public void Notify() => OnChange?.Invoke();
}
