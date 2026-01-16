using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskFlow.Application.DTOs;
using Xunit;

namespace TaskFlow.Tests.APITest;

public class ProjectControllerApiTests : BaseApiTests
{
    public ProjectControllerApiTests(ApiDatabaseFixture fixture)
        : base(fixture)
    {
    }

    // ---------- HELPERS ----------

    private async Task<string> RegisterLoginAndCreateOrgAsync(string username)
    {
        var register = await Client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            UserName = username,
            Password = "Password123!"
        });

        var auth = await register.Content.ReadFromJsonAsync<AuthResponseDto>();

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        await Client.PostAsJsonAsync("/api/organization", new
        {
            Name = $"{username}-org"
        });

        return auth.AccessToken;
    }

    // ---------- CREATE PROJECT ----------

    [Fact]
    public async Task CreateProject_ShouldReturn401_WhenTokenMissing()
    {
        var response = await Client.PostAsJsonAsync("/api/project", new
        {
            Name = "Test Project"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateProject_ShouldCreate_WhenUserIsOrgMember()
    {
        var token = await RegisterLoginAndCreateOrgAsync("projectowner");

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.PostAsJsonAsync("/api/project", new
        {
            Name = "My Project"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("My Project", body);
    }

    [Fact]
    public async Task CreateProject_ShouldFail_WhenNameIsEmpty()
    {
        var token = await RegisterLoginAndCreateOrgAsync("emptyproject");

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.PostAsJsonAsync("/api/project", new
        {
            Name = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---------- GET PROJECTS ----------

    [Fact]
    public async Task GetAllProjects_ShouldReturn401_WhenTokenMissing()
    {
        var response = await Client.GetAsync("/api/project");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAllProjects_ShouldReturnOnlyUserProjects()
    {
        var token = await RegisterLoginAndCreateOrgAsync("listprojects");

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        await Client.PostAsJsonAsync("/api/project", new { Name = "Project A" });
        await Client.PostAsJsonAsync("/api/project", new { Name = "Project B" });

        var response = await Client.GetAsync("/api/project");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Project A", body);
        Assert.Contains("Project B", body);
    }

    // ---------- GET BY ID ----------

    [Fact]
    public async Task GetProjectById_ShouldReturn404_WhenProjectDoesNotExist()
    {
        var token = await RegisterLoginAndCreateOrgAsync("missingproject");

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.GetAsync("/api/project/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetProjectById_ShouldReturn403_WhenUserIsNotMember()
    {
        var ownerToken = await RegisterLoginAndCreateOrgAsync("realowner");

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", ownerToken);

        var create = await Client.PostAsJsonAsync("/api/project", new
        {
            Name = "Private Project"
        });

        var projectBody = await create.Content.ReadAsStringAsync();

        // başka kullanıcı
        var otherToken = await RegisterLoginAndCreateOrgAsync("outsider");

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", otherToken);

        var response = await Client.GetAsync("/api/project/1");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ---------- UPDATE PROJECT ----------

    [Fact]
    public async Task UpdateProject_ShouldFail_WhenUserIsNotOwner()
    {
        var ownerToken = await RegisterLoginAndCreateOrgAsync("updateowner");

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", ownerToken);

        await Client.PostAsJsonAsync("/api/project", new
        {
            Name = "Owned Project"
        });

        var memberToken = await RegisterLoginAndCreateOrgAsync("member");

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", memberToken);

        var response = await Client.PutAsJsonAsync("/api/project/1", new
        {
            Id = 1,
            Name = "Hack Update"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ---------- DELETE PROJECT ----------

    [Fact]
    public async Task DeleteProject_ShouldReturn403_WhenUserIsNotOwner()
    {
        var ownerToken = await RegisterLoginAndCreateOrgAsync("deleteowner");

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", ownerToken);

        await Client.PostAsJsonAsync("/api/project", new
        {
            Name = "Delete Project"
        });

        var memberToken = await RegisterLoginAndCreateOrgAsync("deletemember");

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", memberToken);

        var response = await Client.DeleteAsync("/api/project/1");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ---------- CONTRACT ----------

    [Fact]
    public async Task ProjectEndpoints_ShouldNotAllow_WrongHttpMethod()
    {
        var response = await Client.PatchAsync("/api/project", null);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }
}
