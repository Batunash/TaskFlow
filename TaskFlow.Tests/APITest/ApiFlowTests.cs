using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using TaskFlow.Application.DTOs;
using Xunit;

namespace TaskFlow.Tests.APITest;

public class FullFlow_EndToEnd_ApiTests : BaseApiTests
{
    public FullFlow_EndToEnd_ApiTests(ApiDatabaseFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task FullFlow_Register_To_TaskStatusChange_ShouldWork()
    {
        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            UserName = "fullflowuser",
            Password = "Password123!"
        });

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(auth);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
        var orgResponse = await Client.PostAsJsonAsync("/api/organization", new
        {
            Name = "Full Flow Org"
        });

        Assert.Equal(HttpStatusCode.OK, orgResponse.StatusCode);

        var orgNode = await orgResponse.Content.ReadFromJsonAsync<JsonNode>();
        var newAccessToken = orgNode?["accessToken"]?.ToString();
        Assert.False(string.IsNullOrEmpty(newAccessToken), "Yeni token dönmedi!");
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newAccessToken);
        var projectResponse = await Client.PostAsJsonAsync("/api/project", new
        {
            Name = "Full Flow Project"
        });

        Assert.Equal(HttpStatusCode.OK, projectResponse.StatusCode);

        var projectDto = await projectResponse.Content.ReadFromJsonAsync<ResponseProjectDto>();
        var projectId = projectDto!.Id;
        var workflowResponse = await Client.PostAsync($"/api/projects/{projectId}/workflow", null);
        Assert.Equal(HttpStatusCode.OK, workflowResponse.StatusCode);
        var todoStateResponse = await Client.PostAsJsonAsync($"/api/projects/{projectId}/workflow/states",new WorkflowStateDto { Name = "Todo", IsInitial = true });
        Assert.Equal(HttpStatusCode.OK, todoStateResponse.StatusCode);
        var todoState = await todoStateResponse.Content.ReadFromJsonAsync<WorkflowStateDto>();
        var inProgressStateResponse = await Client.PostAsJsonAsync($"/api/projects/{projectId}/workflow/states",new WorkflowStateDto { Name = "In Progress" });
        Assert.Equal(HttpStatusCode.OK, inProgressStateResponse.StatusCode);
        var inProgressState = await inProgressStateResponse.Content.ReadFromJsonAsync<WorkflowStateDto>();
        await Client.PostAsJsonAsync($"/api/projects/{projectId}/workflow/states",new WorkflowStateDto { Name = "Done", IsFinal = true });
        var transitionResponse = await Client.PostAsJsonAsync($"/api/projects/{projectId}/workflow/transitions",new WorkflowTransitionDto{FromStateId = todoState!.Id!.Value,ToStateId = inProgressState!.Id!.Value,AllowedRoles = new List<string>()});
        Assert.Equal(HttpStatusCode.OK, transitionResponse.StatusCode);
        var taskResponse = await Client.PostAsJsonAsync("/api/task", new
        {
            Title = "My First Task",
            ProjectId = projectId
        });

        Assert.Equal(HttpStatusCode.OK, taskResponse.StatusCode);

        var taskNode = await taskResponse.Content.ReadFromJsonAsync<JsonNode>();
        var taskId = taskNode?["id"]?.GetValue<int>();
        var statusResponse = await Client.PostAsJsonAsync("/api/task/status", new{TaskId = taskId,TargetStateId = inProgressState.Id!.Value});

        Assert.Equal(HttpStatusCode.OK, statusResponse.StatusCode);
    }
}