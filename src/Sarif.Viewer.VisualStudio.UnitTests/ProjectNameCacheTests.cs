// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using EnvDTE;
using FluentAssertions;
using Moq;
using Xunit;

namespace Microsoft.Sarif.Viewer.VisualStudio.UnitTests
{
    public class ProjectNameCacheTests
    {
        private const string FileName = "SomeFile.cs";
        private const string ProjectName = "SomeProject";

        [Fact]
        public void ProjectNameCache_GetName_WhereSolutionDoesNotContainFile_ReturnsEmptyString()
        {
            // Arrange.

            // Create a solution which doesn't contain project items matching any file name.
            var mockSolution = new Mock<Solution>();
            mockSolution
                .Setup(s => s.FindProjectItem(It.IsAny<string>()))
                .Returns(default(ProjectItem));

            var target = new ProjectNameCache(mockSolution.Object);

            // Act.
            string result = target.GetName(FileName);

            // Assert.
            result.Should().BeEmpty();
        }

        [Fact]
        public void ProjectNameCache_GetName_WhereSolutionContainsFile_ReturnsNameOfProjectContainingFile()
        {
            // Arrange.

            // Create a project with the specified name.
            var mockProject = new Mock<Project>();
            mockProject
                .SetupGet(p => p.Name)
                .Returns(ProjectName);

            // Create an item within that project. The item's Name property doesn't matter;
            // all that matters is its ContainingProject.
            var mockProjectItem = new Mock<ProjectItem>();
            mockProjectItem
                .SetupGet(pi => pi.ContainingProject)
                .Returns(mockProject.Object);

            // Create a solution which returns that project item for the specified file name.
            // Again, the ProjectItem's Name property doesn't have to be set to match the name
            // by which we look it up.
            var mockSolution = new Mock<Solution>(MockBehavior.Loose);
            mockSolution
                .Setup(s => s.FindProjectItem(FileName))
                .Returns(mockProjectItem.Object);

            var target = new ProjectNameCache(mockSolution.Object);

            // Act.
            string result = target.GetName(FileName);

            // Assert.
            result.Should().Be(ProjectName);
        }

        [Fact]
        public void ProjectNameCache_GetName_LooksUpAGivenFileNameInTheSolutionOnlyOnce()
        {
            // Arrange.

            // Create a project with the specified name.
            var mockProject = new Mock<Project>();
            mockProject
                .SetupGet(p => p.Name)
                .Returns(ProjectName);

            // Create an item within that project. The item's Name property doesn't matter;
            // all that matters is its ContainingProject.
            var mockProjectItem = new Mock<ProjectItem>();
            mockProjectItem
                .SetupGet(pi => pi.ContainingProject)
                .Returns(mockProject.Object);

            // Create a solution which returns that project item for the specified file name.
            // Again, the ProjectItem's Name property doesn't have to be set to match the name
            // by which we look it up.
            var mockSolution = new Mock<Solution>(MockBehavior.Loose);
            mockSolution
                .Setup(s => s.FindProjectItem(FileName))
                .Returns(mockProjectItem.Object);

            var target = new ProjectNameCache(mockSolution.Object);

            // Act.
            string result1 = target.GetName(FileName);
            string result2 = target.GetName(FileName);

            // Assert.
            result1.Should().Be(ProjectName);
            result2.Should().Be(ProjectName);

            // We should have looked up the file name in the solution only once.
            mockSolution.Verify(s => s.FindProjectItem(FileName), Times.Once());
        }
    }
}
