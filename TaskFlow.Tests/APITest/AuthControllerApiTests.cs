using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskFlow.Application.DTOs;
using Xunit;

namespace TaskFlow.Tests.APITest;

public class AuthControllerEdgeCaseApiTests : BaseApiTests
{
    public AuthControllerEdgeCaseApiTests(ApiDatabaseFixture fixture)
        : base(fixture)
    {
    }
    [Fact]
    public async Task Register_ShouldFail_WhenUsernameIsEmpty()
    {
        var dto = new RegisterDto
        {
            UserName = "",
            Password = "Password123!"
        };

        var response = await Client.PostAsJsonAsync("/api/auth/register", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_ShouldFail_WhenPasswordIsWeak()
    {
        var dto = new RegisterDto
        {
            UserName = "weakpassuser",
            Password = "123"
        };

        var response = await Client.PostAsJsonAsync("/api/auth/register", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_ShouldFail_WhenUsernameAlreadyExists()
    {
        var dto = new RegisterDto
        {
            UserName = "duplicateuser",
            Password = "Password123!"
        };

        await Client.PostAsJsonAsync("/api/auth/register", dto);
        var response = await Client.PostAsJsonAsync("/api/auth/register", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_ShouldFail_WhenBodyIsMissing()
    {
        var response = await Client.PostAsync("/api/auth/register", null);

        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.UnsupportedMediaType
        );
    }
    [Fact]
    public async Task Login_ShouldFail_WhenUserDoesNotExist()
    {
        var dto = new LoginDto
        {
            UserName = "ghostuser",
            Password = "Password123!"
        };

        var response = await Client.PostAsJsonAsync("/api/auth/login", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldFail_WhenPasswordIsWrong()
    {
        await Client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            UserName = "wrongpassuser",
            Password = "Password123!"
        });

        var response = await Client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            UserName = "wrongpassuser",
            Password = "WrongPassword!"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldFail_WhenUsernameIsMissing()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Password = "Password123!"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldFail_WhenPasswordIsMissing()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            UserName = "someuser"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    [Fact]
    public async Task Me_ShouldReturn401_WhenTokenIsMissing()
    {
        var response = await Client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_ShouldReturn401_WhenTokenIsInvalid()
    {
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "this.is.not.a.jwt");

        var response = await Client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_ShouldReturn401_WhenTokenIsExpired()
    {
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "expired.token.example");

        var response = await Client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_ShouldReturnUserInfo_WhenTokenIsValid()
    {
        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            UserName = "edgeuser",
            Password = "Password123!"
        });

        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        var response = await Client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("userId", content);
    }

    [Fact]
    public async Task Register_ShouldIgnore_ExtraFields_InRequestBody()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            UserName = "extrafielduser",
            Password = "Password123!",
            IsAdmin = true,
            Hacked = "yes"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Auth_Endpoints_ShouldNotAllow_GetRequests()
    {
        var response = await Client.GetAsync("/api/auth/login");

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }
}
