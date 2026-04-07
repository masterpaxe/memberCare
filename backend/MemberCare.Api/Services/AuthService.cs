using MemberCare.Api.Contracts;

namespace MemberCare.Api.Services;

public sealed class AuthService
{
    public AuthTokenResponse Login(AuthLoginRequest request)
    {
        _ = request;
        return new AuthTokenResponse(
            AccessToken: "dev-access-token",
            RefreshToken: "dev-refresh-token",
            ExpiresIn: 3600
        );
    }
}
