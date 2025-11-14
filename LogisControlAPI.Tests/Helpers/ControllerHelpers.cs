using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public static class ControllerHelpers
{
    public static ControllerContext CreateFakeContextWithUser(int userId)
    {
        var user = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim("id", userId.ToString())
            }, "mock"));

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }
}