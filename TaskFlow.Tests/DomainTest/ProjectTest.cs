using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Tests.DomainTest
{
    public class ProjectTest
    {
        [Fact]
        public void CreateProject_ShouldThrowException_WhenNameIsEmpty()
        {
            // Arrange
            string emptyName = "";

            // Act
            Action action = () => new Project(emptyName, "Desc", 1);

            // Assert
            action.Should().Throw<ArgumentException>()
                  .WithMessage("*name cannot be empty*");
        }

        [Fact]
        public void CreateProject_ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            var name = "Test Project";
            var description = "This is a test project.";
            var organizationId = 1;

            // Act
            var project = new Project(name, description, organizationId);

            // Assert
            project.Name.Should().Be(name);
            project.Description.Should().Be(description);
            project.OrganizationId.Should().Be(organizationId);
            project.Tasks.Should().BeEmpty();
            project.ProjectMembers.Should().BeEmpty();
        }

        [Fact]
        public void Update_ShouldThrowException_WhenNameIsEmpty()
        {
            // Arrange
            var project = new Project("Old Name", "Old Description", 1);

            // Act
            Action action = () => project.Update("", "New Description");

            // Assert
            action.Should().Throw<ArgumentException>()
                  .WithMessage("*Project name cannot be empty*");
        }
        [Fact]
        public void AddMember_ShouldAddProjectMember()
        {
            // Arrange
            var project = new Project("Test Project", "Description", 1);
            var userId = 42;
            var role = Role.Member;
            // Act
            project.AddMember(userId, role);
            // Assert
            project.ProjectMembers.Should().ContainSingle(m => m.UserId == userId && m.Role == role);
        }
        [Fact]
        public void IsMember_ShouldReturnTrueIfUserIsMember()
        {
            // Arrange
            var project = new Project("Test Project", "Description", 1);
            var userId = 42;
            project.AddMember(userId, Role.Member);
            // Act
            var isMember = project.IsMember(userId);
            // Assert
            isMember.Should().BeTrue();
        }
        [Fact]
        public void IsMember_ShouldReturnFalseIfUserIsNotMember()
        {
            // Arrange
            var project = new Project("Test Project", "Description", 1);
            var userId = 42;
            // Act
            var isMember = project.IsMember(userId);
            // Assert
            isMember.Should().BeFalse();
        }
        [Fact]
        public void CreateTask_ShouldAddTaskToProject()
        {
            // Arrange
            var project = new Project("Test Project", "Description", 1);
            var title = "New Task";
            var initialWorkflowStateId = 1;
            // Act
            var task = project.CreateTask(title, initialWorkflowStateId);
            // Assert
            project.Tasks.Should().ContainSingle(t => t == task);
            task.Title.Should().Be(title);
            task.WorkflowStateId.Should().Be(initialWorkflowStateId);
        }
        [Fact]
        public void Update_ShouldModifyNameAndDescription()
        {
            // Arrange
            var project = new Project("Old Name", "Old Description", 1);
            var newName = "New Name";
            var newDescription = "New Description";
            // Act
            project.Update(newName, newDescription);
            // Assert
            project.Name.Should().Be(newName);
            project.Description.Should().Be(newDescription);
        }
        [Fact]
        public void AddMember_ShouldNotAddDuplicateMembers()
        {
            // Arrange
            var project = new Project("Test Project", "Description", 1);
            var userId = 42;
            var role = Role.Member;
            project.AddMember(userId, role);
            // Act
            project.AddMember(userId, role);
            // Assert
            project.ProjectMembers.Should().HaveCount(1);
        }
        [Fact]
        public void CreateTask_ShouldInitializeTaskPropertiesCorrectly()
        {
            // Arrange
            var project = new Project("Test Project", "Description", 1);
            var title = "New Task";
            var initialWorkflowStateId = 1;
            // Act
            var task = project.CreateTask(title, initialWorkflowStateId);
            // Assert
            task.Title.Should().Be(title);
            task.ProjectId.Should().Be(project.Id);
            task.OrganizationId.Should().Be(project.OrganizationId);
            task.WorkflowStateId.Should().Be(initialWorkflowStateId);
            task.IsDeleted.Should().BeFalse();
            task.AssignedUserId.Should().BeNull();
        }
    }
}