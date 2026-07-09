#nullable enable
using Application.PlannerTasks.Commands.UpdatePlannerTask;
using Application.PlannerTasks.Dtos;
using Domain.Common;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers;
using WebAPI.UnitTests.TestInfrastructure;

namespace WebAPI.UnitTests.Controllers;

/// <summary>
/// Unit tests for <see cref="PlannerTasksController"/>.
/// </summary>
public sealed class PlannerTasksControllerTests
{
    /// <summary>
    /// Verifies that route id values are copied into update planner task commands.
    /// </summary>
    [Fact]
    public async Task UpdatePlannerTask_WithRouteId_SendsCommandWithId()
    {
        var response = BaseResponse<PlannerTaskDto>.Ok(new PlannerTaskDto(new PlannerTask
        {
            Id = 100001,
            Title = "Go to JMB for water bill",
            CalendarId = 1
        }));
        var mediator = new TestMediator(_ => Task.FromResult<object>(response));
        var controller = new PlannerTasksController(mediator);
        var command = new UpdatePlannerTaskCommand { Title = "Updated task" };

        var result = await controller.UpdatePlannerTask(100001, command);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, okResult.Value);
        var sentCommand = Assert.IsType<UpdatePlannerTaskCommand>(mediator.LastRequest);
        Assert.Equal(100001, sentCommand.Id);
        Assert.Equal("Updated task", sentCommand.Title);
    }
}
