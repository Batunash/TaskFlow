using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskFlow.Application.DTOs;
using Xunit;

namespace TaskFlow.Tests.APITest;

public class OrganizationControllerApiTests : BaseApiTests
{
    public OrganizationControllerApiTests(ApiDatabaseFixture fixture)
        : base(fixture)
    {
    }

    // ---------- HELPERS ----------

    private async Task<string> RegisterAndLoginAsync(string username)
    {
        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            UserName = username,
            Password = "Password123!"
        });

        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        return auth!.AccessToken;
    }

    // ---------- CREATE ORGANIZATION ----------

    [Fact]
    public async Task CreateOrganization_ShouldReturn401_WhenTokenIsMissing()
    {
        var response = await Client.PostAsJsonAsync("/api/organization", new
        {
            Name = "Test Org"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrganization_ShouldCreateOrg_AndReturnNewToken()
    {
        var token = await RegisterAndLoginAsync("orgowner");

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.PostAsJsonAsync("/api/organization", new
        {
            Name = "My Organization"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();

        Assert.Contains("Organization", body);
        Assert.Contains("AccessToken", body);
    }

    [Fact]
    public async Task CreateOrganization_ShouldFail_WhenNameIsEmpty()
    {
        var token = await RegisterAndLoginAsync("emptyorg");

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.PostAsJsonAsync("/api/organization", new
        {
            Name = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrganization_ShouldFail_WhenBodyIsMissing()
    {
        var token = await RegisterAndLoginAsync("nobodyorg");

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.PostAsync("/api/organization", null);

        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.UnsupportedMediaType
        );
    }

    // ---------- CURRENT ORGANIZATION ----------

    [Fact]
    public async Task GetCurrentOrganization_ShouldReturn401_WhenTokenMissing()
    {
        var response = await Client.GetAsync("/api/organization/current");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentOrganization_ShouldReturnOrg_WhenUserHasOrganization()
    {
        var token = await RegisterAndLoginAsync("currentorguser");

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        await Client.PostAsJsonAsync("/api/organization", new
        {
            Name = "Current Org"
        });

        var response = await Client.GetAsync("/api/organization/current");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Current Org", body);
    }

    [Fact]
    public async Task GetCurrentOrganization_ShouldFail_WhenUserHasNoOrganization()
    {
        var token = await RegisterAndLoginAsync("noorguser");

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("/api/organization/current");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---------- INVITE USER ----------

    [Fact]
    public async Task InviteUser_ShouldReturn401_WhenTokenMissing()
    {
        var response = await Client.PostAsJsonAsync("/api/organization/invite", new
        {
            Email = "invite@test.com"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task InviteUser_ShouldSucceed_WhenOwnerInvitesUser()
    {
        var ownerToken = await RegisterAndLoginAsync("orgownerinvite");

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", ownerToken);

        await Client.PostAsJsonAsync("/api/organization", new
        {
            Name = "Invite Org"
        });

        var response = await Client.PostAsJsonAsync("/api/organization/invite", new
        {
            Email = "user@test.com"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task InviteUser_ShouldFail_WhenUserIsNotOwner()
    {
        var ownerToken = await RegisterAndLoginAsync("realowner");

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", ownerToken);

        await Client.PostAsJsonAsync("/api/organization", new
        {
            Name = "Role Org"
        });

        // ikinci kullanıcı
        var memberToken = await RegisterAndLoginAsync("memberuser");

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", memberToken);

        var response = await Client.PostAsJsonAsync("/api/organization/invite", new
        {
            Email = "evil@test.com"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ---------- SECURITY / CONTRACT ----------

    [Fact]
    public async Task OrganizationEndpoints_ShouldNotAllow_WrongHttpMethods()
    {
        var response = await Client.PutAsync("/api/organization", null);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrganization_ShouldIgnore_ExtraFields()
    {
        var token = await RegisterAndLoginAsync("extrafieldorg");

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.PostAsJsonAsync("/api/organization", new
        {
            Name = "Safe Org",
            IsAdmin = true,
            Hack = "nope"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
