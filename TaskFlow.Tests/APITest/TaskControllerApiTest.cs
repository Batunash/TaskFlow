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
    private async Task<(string Token, int ProjectId)> RegisterLoginCreateOrgAndProjectAsync(string username)
    {
        var reg = await Client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            UserName = username,
            Password = "Password123!"
        });
        var auth = await reg.Content.ReadFromJsonAsync<AuthResponseDto>();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        var orgResponse = await Client.PostAsJsonAsync("/api/organization", new { Name = $"{username}-org" });
        var orgBody = await orgResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var newToken = orgBody.GetProperty("accessToken").GetString();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
        var projResponse = await Client.PostAsJsonAsync("/api/project", new { Name = $"{username}-project" });
        var projData = await projResponse.Content.ReadFromJsonAsync<ResponseProjectDto>();
        var projectId = projData!.Id;
        var wfResponse = await Client.PostAsync($"/api/projects/{projectId}/workflow", null);
        Assert.Equal(HttpStatusCode.OK, wfResponse.StatusCode);
        var stateResponse = await Client.PostAsJsonAsync($"/api/projects/{projectId}/workflow/states", new WorkflowStateDto
        {
            Name = "Open",
            IsInitial = true,
            IsFinal = false
        });
        Assert.Equal(HttpStatusCode.OK, stateResponse.StatusCode);

        return (newToken!, projectId);
    }
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
        var (token, projectId) = await RegisterLoginCreateOrgAndProjectAsync("taskcreator");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.PostAsJsonAsync("/api/task", new
        {
            Title = "My Task",
            ProjectId = projectId 
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("My Task", body);
    }
    [Fact]
    public async Task CreateTask_ShouldFail_WhenTitleIsEmpty()
    {
        var (token, projectId) = await RegisterLoginCreateOrgAndProjectAsync("emptytask");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.PostAsJsonAsync("/api/task", new
        {
            Title = "",
            ProjectId = projectId
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateTask_ShouldFail_WhenProjectDoesNotExist()
    {
        var (token, _) = await RegisterLoginCreateOrgAndProjectAsync("wrongproject");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.PostAsJsonAsync("/api/task", new
        {
            Title = "Ghost Task",
            ProjectId = 99999
        });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    [Fact]
    public async Task GetTasksByProject_ShouldReturn401_WhenTokenMissing()
    {
        var response = await Client.GetAsync("/api/project/1/tasks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTasksByProject_ShouldReturnTasks_WhenAuthorized()
    {
        var (token, projectId) = await RegisterLoginCreateOrgAndProjectAsync("listtasks");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await Client.PostAsJsonAsync("/api/task", new { Title = "Task A", ProjectId = projectId });
        await Client.PostAsJsonAsync("/api/task", new { Title = "Task B", ProjectId = projectId });

        var response = await Client.GetAsync($"/api/project/{projectId}/tasks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Task A", body);
        Assert.Contains("Task B", body);
    }
    [Fact]
    public async Task UpdateTask_ShouldReturn404_WhenUserIsNotInSameOrg()
    {
        var (ownerToken, projectId) = await RegisterLoginCreateOrgAndProjectAsync("taskowner");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        var createRes = await Client.PostAsJsonAsync("/api/task", new { Title = "Private Task", ProjectId = projectId });
        var taskData = await createRes.Content.ReadFromJsonAsync<ResponseTaskDto>();
        var taskId = taskData!.Id;
        var (outsiderToken, _) = await RegisterLoginCreateOrgAndProjectAsync("outsider");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", outsiderToken);
        var response = await Client.PutAsJsonAsync("/api/task", new
        {
            Id = taskId,
            Title = "Hack Update"
        });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    [Fact]
    public async Task AssignTask_ShouldReturn404_WhenUserIsNotInSameOrg()
    {
        var (ownerToken, projectId) = await RegisterLoginCreateOrgAndProjectAsync("assignowner");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);

        var createRes = await Client.PostAsJsonAsync("/api/task", new { Title = "Assignable Task", ProjectId = projectId });
        var taskData = await createRes.Content.ReadFromJsonAsync<ResponseTaskDto>();
        var taskId = taskData!.Id;

        var (outsiderToken, _) = await RegisterLoginCreateOrgAndProjectAsync("assignoutsider");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", outsiderToken);

        var response = await Client.PostAsJsonAsync("/api/task/assign", new
        {
            TaskId = taskId,
            UserId = 999
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    [Fact]
    public async Task ChangeTaskStatus_ShouldFail_WhenTransitionIsInvalid()
    {
        var (token, projectId) = await RegisterLoginCreateOrgAndProjectAsync("invalidtransition");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRes = await Client.PostAsJsonAsync("/api/task", new { Title = "Workflow Task", ProjectId = projectId });
        var taskData = await createRes.Content.ReadFromJsonAsync<ResponseTaskDto>();
        var taskId = taskData!.Id;

        var response = await Client.PostAsJsonAsync("/api/task/status", new
        {
            TaskId = taskId,
            TargetStateId = 99999 
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    [Fact]
    public async Task DeleteTask_ShouldReturn404_WhenUserIsNotInSameOrg()
    {
        var (ownerToken, projectId) = await RegisterLoginCreateOrgAndProjectAsync("deletetaskowner");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);

        var createRes = await Client.PostAsJsonAsync("/api/task", new { Title = "Delete Me", ProjectId = projectId });
        var taskData = await createRes.Content.ReadFromJsonAsync<ResponseTaskDto>();
        var taskId = taskData!.Id;

        var (outsiderToken, _) = await RegisterLoginCreateOrgAndProjectAsync("deletetaskoutsider");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", outsiderToken);

        var response = await Client.DeleteAsync($"/api/task/{taskId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    [Fact]
    public async Task TaskEndpoints_ShouldNotAllow_WrongHttpMethod()
    {
        var response = await Client.PatchAsync("/api/task", null);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }
}