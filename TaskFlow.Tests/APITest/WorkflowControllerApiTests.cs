using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskFlow.Application.DTOs;
using Xunit;

namespace TaskFlow.Tests.APITest;

public class WorkflowControllerEdgeCaseApiTests : BaseApiTests
{
    public WorkflowControllerEdgeCaseApiTests(ApiDatabaseFixture fixture)
        : base(fixture)
    {
    }

    // ---------- HELPERS ----------

    private async Task<(string token, int projectId)> RegisterLoginCreateOrgAndProjectAsync(string username)
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

        var projectResp = await Client.PostAsJsonAsync("/api/project", new { Name = $"{username}-project" });
        var projectBody = await projectResp.Content.ReadAsStringAsync();

        // Basitlik için 1 varsayıyoruz; projende response DTO varsa burayı parse et
        return (auth.AccessToken, 1);
    }

    // ---------- CREATE WORKFLOW ----------

    [Fact]
    public async Task CreateWorkflow_ShouldReturn401_WhenTokenMissing()
    {
        var response = await Client.PostAsync("/api/projects/1/workflow", null);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateWorkflow_ShouldCreate_WhenProjectExists()
    {
        var (token, projectId) = await RegisterLoginCreateOrgAndProjectAsync("wfcreator");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.PostAsync($"/api/projects/{projectId}/workflow", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateWorkflow_ShouldFail_WhenProjectDoesNotExist()
    {
        var (token, _) = await RegisterLoginCreateOrgAndProjectAsync("wfmissing");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.PostAsync("/api/projects/999/workflow", null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- GET WORKFLOW ----------

    [Fact]
    public async Task GetWorkflow_ShouldReturn401_WhenTokenMissing()
    {
        var response = await Client.GetAsync("/api/projects/1/workflow");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetWorkflow_ShouldReturnWorkflow_WhenExists()
    {
        var (token, projectId) = await RegisterLoginCreateOrgAndProjectAsync("wfget");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        await Client.PostAsync($"/api/projects/{projectId}/workflow", null);

        var response = await Client.GetAsync($"/api/projects/{projectId}/workflow");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ---------- ADD STATE ----------

    [Fact]
    public async Task AddState_ShouldFail_WhenNameIsEmpty()
    {
        var (token, projectId) = await RegisterLoginCreateOrgAndProjectAsync("wfemptystate");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        await Client.PostAsync($"/api/projects/{projectId}/workflow", null);

        var response = await Client.PostAsJsonAsync(
            $"/api/projects/{projectId}/workflow/states",
            new WorkflowStateDto { Name = "" }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddState_ShouldFail_WhenDuplicateStateName()
    {
        var (token, projectId) = await RegisterLoginCreateOrgAndProjectAsync("wfdupstate");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        await Client.PostAsync($"/api/projects/{projectId}/workflow", null);

        await Client.PostAsJsonAsync(
            $"/api/projects/{projectId}/workflow/states",
            new WorkflowStateDto { Name = "Todo" }
        );

        var response = await Client.PostAsJsonAsync(
            $"/api/projects/{projectId}/workflow/states",
            new WorkflowStateDto { Name = "Todo" }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---------- ADD TRANSITION ----------

    [Fact]
    public async Task AddTransition_ShouldFail_WhenStatesDoNotExist()
    {
        var (token, projectId) = await RegisterLoginCreateOrgAndProjectAsync("wfbadtransition");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        await Client.PostAsync($"/api/projects/{projectId}/workflow", null);

        var response = await Client.PostAsJsonAsync(
            $"/api/projects/{projectId}/workflow/transitions",
            new WorkflowTransitionDto
            {
                FromStateId = 999,
                ToStateId = 1000
            }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddTransition_ShouldFail_WhenSelfTransition()
    {
        var (token, projectId) = await RegisterLoginCreateOrgAndProjectAsync("wfselftransition");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        await Client.PostAsync($"/api/projects/{projectId}/workflow", null);

        var stateResp = await Client.PostAsJsonAsync(
            $"/api/projects/{projectId}/workflow/states",
            new WorkflowStateDto { Name = "InProgress" }
        );

        // StateId'yi response’tan parse et (örnek için 1 varsayıyoruz)
        var response = await Client.PostAsJsonAsync(
            $"/api/projects/{projectId}/workflow/transitions",
            new WorkflowTransitionDto
            {
                FromStateId = 1,
                ToStateId = 1
            }
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---------- REMOVE STATE ----------

    [Fact]
    public async Task RemoveState_ShouldFail_WhenStateIsUsedInTransition()
    {
        var (token, projectId) = await RegisterLoginCreateOrgAndProjectAsync("wfremovestate");
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        await Client.PostAsync($"/api/projects/{projectId}/workflow", null);

        await Client.PostAsJsonAsync(
            $"/api/projects/{projectId}/workflow/states",
            new WorkflowStateDto { Name = "Todo" }
        );
        await Client.PostAsJsonAsync(
            $"/api/projects/{projectId}/workflow/states",
            new WorkflowStateDto { Name = "Done" }
        );

        await Client.PostAsJsonAsync(
            $"/api/projects/{projectId}/workflow/transitions",
            new WorkflowTransitionDto
            {
                FromStateId = 1,
                ToStateId = 2
            }
        );

        var response = await Client.DeleteAsync(
            $"/api/projects/{projectId}/workflow/states/1"
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---------- CONTRACT ----------

    [Fact]
    public async Task WorkflowEndpoints_ShouldNotAllow_WrongHttpMethod()
    {
        var response = await Client.PutAsync("/api/projects/1/workflow", null);
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }
}
