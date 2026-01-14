using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Tests.DomainTest
{
    public class Workflowtest
    {
        [Fact]
        public void RemoveState_ShouldThrowException_WhenStateIsInitial()
        {
            // Arrange
            var workflow = new Workflow(projectId: 1);
            var initialState = new WorkflowState(workflow.Id, "Start", isInitial: true, isFinal: false);
            workflow.AddState(initialState);

            // Act
            var action = () => workflow.RemoveState(initialState);

            // Assert
            action.Should().Throw<InvalidOperationException>()
                  .WithMessage("Initial state cannot be removed");
        }
        [Fact]
        public void Create_Workflow_ShouldInitializeCorrectly()
        {
            // Arrange
            int projectId = 1;
            // Act
            var workflow = new Workflow(projectId);
            // Assert
            workflow.ProjectId.Should().Be(projectId);
            workflow.States.Should().BeEmpty();
            workflow.Transitions.Should().BeEmpty();
        }
        [Fact]
        public void AddState_ShouldAddStateSuccessfully()
        {
            // Arrange
            var workflow = new Workflow(1);
            var state = new WorkflowState(workflowId:1,"To Do", isInitial: true, isFinal:false);
            // Act
            workflow.AddState(state);
            // Assert
            workflow.States.Should().Contain(state);
        }
        [Fact]
        public void RemoveState_ShouldThrowException_WhenStateHasActiveTransitions()
        {
            // Arrange
            var workflow = new Workflow(projectId: 1);
            var stateA = new WorkflowState(workflow.Id, "A", false, false);
            var stateB = new WorkflowState(workflow.Id, "B", false, false);
            workflow.AddState(stateA);
            workflow.AddState(stateB);

            var transition = new WorkflowTransition(workflow.Id, stateA.Id, stateB.Id, new List<string> { "Admin" });
            workflow.AddTransition(transition);

            // Act 
            var action = () => workflow.RemoveState(stateA);

            // Assert
            action.Should().Throw<InvalidOperationException>()
                  .WithMessage("State has transitions and cannot be removed");
        }
        [Fact]
        public void CanTransition_ShouldReturnTrue_WhenRoleIsAllowed()
        {
            // Arrange
            var workflow = new Workflow(projectId: 1);
            var stateA = new WorkflowState(workflow.Id, "A", true, false); 
            var stateFrom = new WorkflowState(workflow.Id, "Open", true, false);
            var stateTo = new WorkflowState(workflow.Id, "Closed", false, true);
            SetPrivateId(stateFrom, 1);
            SetPrivateId(stateTo, 2);
            workflow.AddState(stateFrom);
            workflow.AddState(stateTo);
            var transition = new WorkflowTransition(workflow.Id, stateFrom.Id, stateTo.Id, new List<string> { "Admin", "Member" });
            workflow.AddTransition(transition);
            // Act
            var canAdmin = workflow.CanTransition(stateFrom, stateTo, "Admin");
            var canGuest = workflow.CanTransition(stateFrom, stateTo, "Guest");

            // Assert
            canAdmin.Should().BeTrue();
            canGuest.Should().BeFalse();
        }
        [Fact] 
        public void AddState_ShouldThrowException_WhenStateNameExists()
        {
            // Arrange
            var workflow = new Workflow(projectId: 1);
            var state1 = new WorkflowState(workflow.Id, "In Progress", false, false);
            var state2 = new WorkflowState(workflow.Id, "In Progress", false, false);
            workflow.AddState(state1);
            // Act
            var action = () => workflow.AddState(state2);
            // Assert
            action.Should().Throw<InvalidOperationException>()
                  .WithMessage("Workflow already contains a state named 'In Progress'.");
        }
        [Fact]
        public void AddState_ShouldThrowException_WhenMultipleInitialStatesAdded()
        {
            // Arrange
            var workflow = new Workflow(projectId: 1);
            var initialState1 = new WorkflowState(workflow.Id, "Start", true, false);
            var initialState2 = new WorkflowState(workflow.Id, "Begin", true, false);
            workflow.AddState(initialState1);
            // Act
            var action = () => workflow.AddState(initialState2);
            // Assert
            action.Should().Throw<InvalidOperationException>()
                  .WithMessage("Workflow can only have one Initial state.");
        }
        [Fact]
        public void RemoveTransition_ShouldThrowException_WhenInitialStateLosesAllOutgoingTransitions()
        {
            // Arrange
            var workflow = new Workflow(projectId: 1);
            var initialState = new WorkflowState(workflow.Id, "Start", true, false);
            var nextState = new WorkflowState(workflow.Id, "Next", false, false);
            workflow.AddState(initialState);
            workflow.AddState(nextState);
            var transition = new WorkflowTransition(workflow.Id, initialState.Id, nextState.Id, new List<string> { "Admin" });
            workflow.AddTransition(transition);
            // Act
            var action = () => workflow.RemoveTransition(transition);
            // Assert
            action.Should().Throw<InvalidOperationException>()
                  .WithMessage("Initial state must have at least one outgoing transition");
        }
        [Fact]
        public void RemoveTransition_ShouldRemoveTransitionSuccessfully()
        {
            // Arrange
            var workflow = new Workflow(projectId: 1);
            var stateA = new WorkflowState(workflow.Id, "A", false, false);
            var stateB = new WorkflowState(workflow.Id, "B", false, false);
            workflow.AddState(stateA);
            workflow.AddState(stateB);
            var transition = new WorkflowTransition(workflow.Id, stateA.Id, stateB.Id, new List<string> { "Admin" });
            workflow.AddTransition(transition);
            // Act
            workflow.RemoveTransition(transition);
            // Assert
            workflow.Transitions.Should().NotContain(transition);
        }
        [Fact]
        public void CanTransition_ShouldReturnFalse_WhenAllowedRolesDoNotMatch()
        {
            // Arrange
            var workflow = new Workflow(projectId: 1);
            var stateFrom = new WorkflowState(workflow.Id, "Open", true, false);
            var stateTo = new WorkflowState(workflow.Id, "Closed", false, true);
            SetPrivateId(stateFrom, 1);
            SetPrivateId(stateTo, 2);
            workflow.AddState(stateFrom);
            workflow.AddState(stateTo);
            var transition = new WorkflowTransition(workflow.Id, stateFrom.Id, stateTo.Id, new List<string> { "Admin", "Member" });
            workflow.AddTransition(transition);
            // Act
            var canGuest = workflow.CanTransition(stateFrom, stateTo, "Guest");
            // Assert
            canGuest.Should().BeFalse();
        }
        [Fact]
        public void AddTransition_ShouldThrowException_WhenStatesDoNotExistInWorkflow()
        {
            // Arrange
            var workflow = new Workflow(projectId: 1);
            var stateA = new WorkflowState(workflow.Id, "A", true, false);
            var stateB = new WorkflowState(workflow.Id, "B", false, false);

            SetPrivateId(stateA, 1);
            SetPrivateId(stateB, 99); 

            workflow.AddState(stateA);

            var transition = new WorkflowTransition(workflow.Id, stateA.Id, stateB.Id, new List<string> { "Admin" });

            // Act
            var action = () => workflow.AddTransition(transition);

            // Assert
            action.Should().Throw<InvalidOperationException>()
                  .WithMessage("Transition states must exist in the workflow.");
        }

        [Fact]
        public void AddTransition_ShouldThrowException_WhenSourceStateIsFinal()
        {
            // Arrange
            var workflow = new Workflow(1);
            var start = new WorkflowState(1, "Start", true, false);
            var end = new WorkflowState(1, "End", false, true); 

            SetPrivateId(start, 1);
            SetPrivateId(end, 2);

            workflow.AddState(start);
            workflow.AddState(end);

            // Act
            var invalidTransition = new WorkflowTransition(1, end.Id, start.Id, new List<string> { "Admin" });
            var action = () => workflow.AddTransition(invalidTransition);

            // Assert
            action.Should().Throw<InvalidOperationException>()
                  .WithMessage("Cannot add outgoing transition from a Final state.");
        }

        [Fact]
        public void AddTransition_ShouldThrowException_WhenTransitionAlreadyExists()
        {
            // Arrange
            var workflow = new Workflow(1);
            var stateA = new WorkflowState(1, "A", true, false);
            var stateB = new WorkflowState(1, "B", false, false);

            SetPrivateId(stateA, 1);
            SetPrivateId(stateB, 2);

            workflow.AddState(stateA);
            workflow.AddState(stateB);

            var transition1 = new WorkflowTransition(1, stateA.Id, stateB.Id, new List<string> { "Admin" });
            workflow.AddTransition(transition1);

            // Act
            var transition2 = new WorkflowTransition(1, stateA.Id, stateB.Id, new List<string> { "User" });
            var action = () => workflow.AddTransition(transition2);

            // Assert
            action.Should().Throw<InvalidOperationException>()
                  .WithMessage("A transition between these states already exists.");
        }

        [Fact]
        public void AddState_ShouldThrowArgumentNullException_WhenStateIsNull()
        {
            // Arrange
            var workflow = new Workflow(1);

            // Act
            var action = () => workflow.AddState(null);

            // Assert
            action.Should().Throw<ArgumentNullException>();
        }
        private void SetPrivateId<T>(T entity, int id)
        {
            typeof(T).GetProperty("Id")?.SetValue(entity, id);
        }

    }
}
