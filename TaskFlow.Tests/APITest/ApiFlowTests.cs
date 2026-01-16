using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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
        // ---------------- REGISTER ----------------
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

        // ---------------- ORGANIZATION ----------------
        var orgResponse = await Client.PostAsJsonAsync("/api/organization", new
        {
            Name = "Full Flow Org"
        });

        Assert.Equal(HttpStatusCode.OK, orgResponse.StatusCode);

        // Token yenileniyor (orgId + role içeriyor)
        var orgBody = await orgResponse.Content.ReadAsStringAsync();
        Assert.Contains("AccessToken", orgBody);

        // ---------------- PROJECT ----------------
        var projectResponse = await Client.PostAsJsonAsync("/api/project", new
        {
            Name = "Full Flow Project"
        });

        Assert.Equal(HttpStatusCode.OK, projectResponse.StatusCode);

        // ---------------- WORKFLOW ----------------
        var workflowResponse = await Client.PostAsync("/api/projects/1/workflow", null);
        Assert.Equal(HttpStatusCode.OK, workflowResponse.StatusCode);

        // Workflow states
        await Client.PostAsJsonAsync("/api/projects/1/workflow/states",
            new WorkflowStateDto { Name = "Todo" });

        await Client.PostAsJsonAsync("/api/projects/1/workflow/states",
            new WorkflowStateDto { Name = "In Progress" });

        await Client.PostAsJsonAsync("/api/projects/1/workflow/states",
            new WorkflowStateDto { Name = "Done" });

        // ---------------- TASK ----------------
        var taskResponse = await Client.PostAsJsonAsync("/api/task", new
        {
            Title = "My First Task",
            ProjectId = 1
        });

        Assert.Equal(HttpStatusCode.OK, taskResponse.StatusCode);

        // ---------------- CHANGE STATUS ----------------
        var statusResponse = await Client.PostAsJsonAsync("/api/task/status", new
        {
            TaskId = 1,
            NewStateId = 2 // Todo -> In Progress
        });

        Assert.Equal(HttpStatusCode.OK, statusResponse.StatusCode);
    }
}
