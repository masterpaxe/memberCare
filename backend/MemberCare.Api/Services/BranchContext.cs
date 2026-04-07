using System.Security.Claims;

namespace MemberCare.Api.Services;

/// <summary>
/// Provides access to the current user's branch context from JWT claims.
/// Used for enforcing multi-tenant row-level security.
/// </summary>
public sealed class BranchContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BranchContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the user's assigned branch_id from JWT claims.
    /// Returns null if user is super_admin (unrestricted access).
    /// </summary>
    public Guid? GetUserBranchId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user is null || !user.Identity?.IsAuthenticated == true)
        {
            return null;
        }

        // Super admins have unrestricted access across all branches
        var role = user.FindFirst("role")?.Value;
        if (role == "super_admin")
        {
            return null; // null signals no branch restriction
        }

        // Extract branch_id from claims
        var branchIdClaim = user.FindFirst("branch_id")?.Value;
        if (string.IsNullOrWhiteSpace(branchIdClaim))
        {
            return null; // No branch assigned (fallback to null)
        }

        return Guid.TryParse(branchIdClaim, out var branchId) ? branchId : null;
    }

    /// <summary>
    /// Gets the user's role from JWT claims.
    /// </summary>
    public string? GetUserRole()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirst("role")?.Value;
    }

    /// <summary>
    /// Determines if user is a super admin (unrestricted branch access).
    /// </summary>
    public bool IsSuperAdmin()
    {
        return GetUserRole() == "super_admin";
    }
}
