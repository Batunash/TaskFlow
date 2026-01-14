using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Tests.DomainTest
{
    public  class TaskItemTest
    {
        [Fact]
        public void CreateTaskItem_ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            var title = "Test Task";
            var projectId = 1;
            var organizationId = 1;
            var initialWorkflowStateId = 1;
            // Act
            var taskItem = new TaskItem(title, projectId, organizationId, initialWorkflowStateId);
            // Assert
            taskItem.Title.Should().Be(title);
            taskItem.ProjectId.Should().Be(projectId);
            taskItem.OrganizationId.Should().Be(organizationId);
            taskItem.WorkflowStateId.Should().Be(initialWorkflowStateId);
            taskItem.IsDeleted.Should().BeFalse();
            taskItem.AssignedUserId.Should().BeNull();
        }
        [Fact]
        public void ChangeState_ShouldUpdateWorkflowStateId()
        {
            // Arrange
            var taskItem = new TaskItem("Test Task", 1, 1, 1);
            var newStateId = 2;
            // Act
            taskItem.ChangeState(newStateId);
            // Assert
            taskItem.WorkflowStateId.Should().Be(newStateId);
        }
        [Fact]
        public void Assign_ShouldSetAssignedUserId()
        {
            // Arrange
            var taskItem = new TaskItem("Test Task", 1, 1, 1);
            var userId = 42;
            // Act
            taskItem.Assign(userId);
            // Assert
            taskItem.AssignedUserId.Should().Be(userId);
        }
        [Fact]
        public void Delete_ShouldMarkTaskAsDeleted()
        {
            // Arrange
            var taskItem = new TaskItem("Test Task", 1, 1, 1);
            // Act
            taskItem.Delete();
            // Assert
            taskItem.IsDeleted.Should().BeTrue();
            taskItem.DeletedAt.Should().NotBeNull();
        }
        [Fact]
        public void Update_ShouldModifyTitleAndDescription()
        {
            // Arrange
            var taskItem = new TaskItem("Old Title", 1, 1, 1);
            var newTitle = "New Title";
            var newDescription = "New Description";
            // Act
            taskItem.Update(newTitle, newDescription);
            // Assert
            taskItem.Title.Should().Be(newTitle);
            taskItem.Description.Should().Be(newDescription);
        }
        [Fact]
        public void CreateTaskItem_ShouldThrowException_WhenTitleIsEmpty()
        {
            // Arrange
            string emptyTitle = "";

            // Act
            Action action = () => new TaskItem(emptyTitle, 1, 1, 1);

            // Assert
            action.Should().Throw<ArgumentException>()
                  .WithMessage("*Title cannot be empty*"); 
        }

        [Fact]
        public void Update_ShouldThrowException_WhenTitleIsEmpty()
        {
            // Arrange
            var taskItem = new TaskItem("Valid Title", 1, 1, 1);

            // Act
            Action action = () => taskItem.Update("", "New Description");

            // Assert
            action.Should().Throw<ArgumentException>()
                  .WithMessage("*Title cannot be empty*");
        }


    }
}
