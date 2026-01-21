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

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.PostAsJsonAsync("/api/organization", new
        {
            Name = "My Organization"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();

        Assert.Contains("Organization", body);
        Assert.Contains("accessToken", body);
    }

    [Fact]
    public async Task CreateOrganization_ShouldFail_WhenNameIsEmpty()
    {
        var token = await RegisterAndLoginAsync("emptyorg");

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.PostAsync("/api/organization", null);

        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.UnsupportedMediaType
        );
    }

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

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var createResponse = await Client.PostAsJsonAsync("/api/organization", new
        {
            Name = "Current Org"
        });
        var createContent = await createResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var newToken = createContent.GetProperty("accessToken").GetString(); 
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
        var response = await Client.GetAsync("/api/organization/current");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Current Org", body);
    }

    [Fact]
    public async Task GetCurrentOrganization_ShouldFail_WhenUserHasNoOrganization()
    {

        var token = await RegisterAndLoginAsync("noorguser");

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("/api/organization/current");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    
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

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.PostAsJsonAsync("/api/organization", new
        {
            Name = "Safe Org",
            IsAdmin = true,
            Hack = "nope"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    [Fact]
    public async Task InviteUser_ShouldSucceed_WhenOwnerInvitesUser_ByUsername()
    {
        var ownerToken = await RegisterAndLoginAsync("invite_owner");
        var guestUserRegister = await Client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            UserName = "guest_for_invite",
            Password = "Password123!"
        });
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        var createResponse = await Client.PostAsJsonAsync("/api/organization", new { Name = "Invite Org" });
        var createBody = await createResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var ownerOrgToken = createBody.GetProperty("accessToken").GetString();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerOrgToken);
        var response = await Client.PostAsJsonAsync("/api/organization/invite", new
        {
            UserName = "guest_for_invite" 
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task InviteUser_ShouldReturn401_WhenTokenMissing()
    {
        var response = await Client.PostAsJsonAsync("/api/organization/invite", new
        {
            UserName = "targetuser"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task InviteUser_ShouldSucceed_WhenOwnerInvitesUser()
    {
        var ownerToken = await RegisterAndLoginAsync("orgownerinvite");
        var guestUserRegister = await Client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            UserName = "guestuser",
            Password = "Password123!"
        });
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        var createResponse = await Client.PostAsJsonAsync("/api/organization", new
        {
            Name = "Invite Org"
        });

        var createBody = await createResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var newToken = createBody.GetProperty("accessToken").GetString();

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
        var response = await Client.PostAsJsonAsync("/api/organization/invite", new
        {
            UserName = "guestuser"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task InviteUser_ShouldFail_WhenUserIsNotOwner()
    {
        var ownerToken = await RegisterAndLoginAsync("realowner");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        await Client.PostAsJsonAsync("/api/organization", new
        {
            Name = "Role Org"
        });
        var memberToken = await RegisterAndLoginAsync("memberuser");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberToken);
        var response = await Client.PostAsJsonAsync("/api/organization/invite", new
        {
            UserName = "someuser"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    [Fact]
    public async Task User_Should_Be_Able_To_Accept_Invitation()
    {
        // ARRANGE
        var ownerToken = await RegisterAndLoginAsync("flow_owner");
        await Client.PostAsJsonAsync("/api/auth/register", new RegisterDto { UserName = "flow_guest", Password = "Password123!" });
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        var createRes = await Client.PostAsJsonAsync("/api/organization", new { Name = "Flow Org" });
        var orgData = await createRes.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        int orgId = orgData.GetProperty("organization").GetProperty("id").GetInt32();
        string ownerOrgToken = orgData.GetProperty("accessToken").GetString();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerOrgToken);
        await Client.PostAsJsonAsync("/api/organization/invite", new { UserName = "flow_guest" });
        // ACT
        var guestToken = await RegisterAndLoginAsync("flow_guest");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", guestToken);
        var acceptRes = await Client.PostAsync($"/api/organization/accept/{orgId}", null);

        // ASSERT
        Assert.Equal(HttpStatusCode.OK, acceptRes.StatusCode);
    }
}
