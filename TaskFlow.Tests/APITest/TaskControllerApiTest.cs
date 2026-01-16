using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskFlow.Application.DTOs;
using Xunit;

namespace TaskFlow.Tests.APITest;

public class TaskControllerApiTests : BaseApiTests
{
    public TaskControllerApiTests(ApiDatabaseFixture fixture)
        : base(fixture)
    {
    }

    // ---------- HELPERS ----------

    private async Task<string> RegisterLoginCreateOrgAndProjectAsync(string username)
    {
        var reg = await Client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            UserName = username,
            Password = "Password123!"
        });
        var auth = await reg.Content.ReadFromJsonAsync<AuthResponseDto>();

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        await Client.PostAsJsonAsync("/api/organization", new { Name = $"{username}-org" });
        await Client.PostAsJsonAsync("/api/project", new { Name = $"{username}-project" });

        return auth.AccessToken;
    }

    // ---------- CREATE TASK ----------

    [Fact]
    public async Task CreateTask_ShouldReturn401_WhenTokenMissing()
    {
        var response = await Client.PostAsJsonAsync("/api/task", new
        {
            Title = "Task 1",
            ProjectId = 1
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateTask_ShouldCreate_WhenUserIsProjectMember()
    {
        var token = await RegisterLoginCreateOrgAndProjectAsync("taskcreator");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.PostAsJsonAsync("/api/task", new
        {
            Title = "My Task",
            ProjectId = 1
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("My Task", body);
    }

    [Fact]
    public async Task CreateTask_ShouldFail_WhenTitleIsEmpty()
    {
        var token = await RegisterLoginCreateOrgAndProjectAsync("emptytask");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.PostAsJsonAsync("/api/task", new
        {
            Title = "",
            ProjectId = 1
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateTask_ShouldFail_WhenProjectDoesNotExist()
    {
        var token = await RegisterLoginCreateOrgAndProjectAsync("wrongproject");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.PostAsJsonAsync("/api/task", new
        {
            Title = "Ghost Task",
            ProjectId = 999
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- GET TASKS ----------

    [Fact]
    public async Task GetTasksByProject_ShouldReturn401_WhenTokenMissing()
    {
        var response = await Client.GetAsync("/api/project/1/tasks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTasksByProject_ShouldReturnTasks_WhenAuthorized()
    {
        var token = await RegisterLoginCreateOrgAndProjectAsync("listtasks");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        await Client.PostAsJsonAsync("/api/task", new { Title = "Task A", ProjectId = 1 });
        await Client.PostAsJsonAsync("/api/task", new { Title = "Task B", ProjectId = 1 });

        var response = await Client.GetAsync("/api/project/1/tasks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Task A", body);
        Assert.Contains("Task B", body);
    }

    // ---------- UPDATE TASK ----------

    [Fact]
    public async Task UpdateTask_ShouldFail_WhenUserIsNotProjectMember()
    {
        var ownerToken = await RegisterLoginCreateOrgAndProjectAsync("taskowner");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", ownerToken);

        await Client.PostAsJsonAsync("/api/task", new { Title = "Private Task", ProjectId = 1 });

        // başka kullanıcı
        var outsiderToken = await RegisterLoginCreateOrgAndProjectAsync("outsider");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", outsiderToken);

        var response = await Client.PutAsJsonAsync("/api/task", new
        {
            TaskId = 1,
            Title = "Hack Update"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ---------- ASSIGN TASK ----------

    [Fact]
    public async Task AssignTask_ShouldFail_WhenUserIsNotProjectMember()
    {
        var ownerToken = await RegisterLoginCreateOrgAndProjectAsync("assignowner");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", ownerToken);

        await Client.PostAsJsonAsync("/api/task", new { Title = "Assignable Task", ProjectId = 1 });

        var outsiderToken = await RegisterLoginCreateOrgAndProjectAsync("assignoutsider");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", outsiderToken);

        var response = await Client.PostAsJsonAsync("/api/task/assign", new
        {
            TaskId = 1,
            UserId = 999
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ---------- CHANGE STATUS ----------

    [Fact]
    public async Task ChangeTaskStatus_ShouldFail_WhenTransitionIsInvalid()
    {
        var token = await RegisterLoginCreateOrgAndProjectAsync("invalidtransition");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        await Client.PostAsJsonAsync("/api/task", new { Title = "Workflow Task", ProjectId = 1 });

        var response = await Client.PostAsJsonAsync("/api/task/status", new
        {
            TaskId = 1,
            NewStateId = 999 // geçersiz state
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---------- DELETE TASK ----------

    [Fact]
    public async Task DeleteTask_ShouldFail_WhenUserIsNotAuthorized()
    {
        var ownerToken = await RegisterLoginCreateOrgAndProjectAsync("deletetaskowner");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", ownerToken);

        await Client.PostAsJsonAsync("/api/task", new { Title = "Delete Me", ProjectId = 1 });

        var outsiderToken = await RegisterLoginCreateOrgAndProjectAsync("deletetaskoutsider");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", outsiderToken);

        var response = await Client.DeleteAsync("/api/task/1");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ---------- CONTRACT ----------

    [Fact]
    public async Task TaskEndpoints_ShouldNotAllow_WrongHttpMethod()
    {
        var response = await Client.PatchAsync("/api/task", null);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }
}
