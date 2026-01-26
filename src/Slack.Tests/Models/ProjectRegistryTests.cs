using Xunit;
using Slack.Models;

namespace Slack.Tests.Models;

public class ProjectRegistryTests
{
    #region ProjectEntry Tests

    [Fact]
    public void ProjectEntry_NewInstance_ShouldHaveDefaultValues()
    {
        // Act
        var entry = new ProjectEntry();

        // Assert
        Assert.Equal(string.Empty, entry.Path);
        Assert.Equal(string.Empty, entry.Name);
        Assert.True(entry.CreatedAt <= DateTime.UtcNow);
        Assert.True(entry.LastUsedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void ProjectEntry_ShouldBeSettable()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var entry = new ProjectEntry
        {
            Path = "/test/path",
            Name = "TestProject",
            CreatedAt = now,
            LastUsedAt = now
        };

        // Assert
        Assert.Equal("/test/path", entry.Path);
        Assert.Equal("TestProject", entry.Name);
        Assert.Equal(now, entry.CreatedAt);
        Assert.Equal(now, entry.LastUsedAt);
    }

    #endregion

    #region ProjectRegistry Instance Tests

    [Fact]
    public void ProjectRegistry_NewInstance_ShouldHaveEmptyProjects()
    {
        // Act
        var registry = new ProjectRegistry();

        // Assert
        Assert.NotNull(registry.Projects);
        Assert.Empty(registry.Projects);
    }

    [Fact]
    public void ProjectRegistry_AddProject_ShouldAddToList()
    {
        // Arrange
        var registry = new ProjectRegistry();
        var entry = new ProjectEntry
        {
            Path = "/test/project",
            Name = "Test"
        };

        // Act
        registry.Projects.Add(entry);

        // Assert
        Assert.Single(registry.Projects);
        Assert.Contains(entry, registry.Projects);
    }

    [Fact]
    public void ProjectRegistry_RemoveProject_ShouldRemoveFromList()
    {
        // Arrange
        var registry = new ProjectRegistry();
        var entry = new ProjectEntry { Path = "/test/project", Name = "Test" };
        registry.Projects.Add(entry);

        // Act
        registry.Projects.Remove(entry);

        // Assert
        Assert.Empty(registry.Projects);
    }

    #endregion

    #region Serialization Tests

    [Fact]
    public void ProjectRegistry_SerializationRoundTrip_ShouldPreserveData()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var registry = new ProjectRegistry
        {
            Projects =
            [
                new ProjectEntry
                {
                    Path = "/project1",
                    Name = "Project 1",
                    CreatedAt = now,
                    LastUsedAt = now
                },
                new ProjectEntry
                {
                    Path = "/project2",
                    Name = "Project 2",
                    CreatedAt = now.AddDays(-1),
                    LastUsedAt = now
                }
            ]
        };

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(registry);
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<ProjectRegistry>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(2, deserialized.Projects.Count);
        Assert.Equal("/project1", deserialized.Projects[0].Path);
        Assert.Equal("Project 1", deserialized.Projects[0].Name);
        Assert.Equal("/project2", deserialized.Projects[1].Path);
        Assert.Equal("Project 2", deserialized.Projects[1].Name);
    }

    [Fact]
    public void ProjectRegistry_EmptyRegistry_ShouldSerializeCorrectly()
    {
        // Arrange
        var registry = new ProjectRegistry();

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(registry);
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<ProjectRegistry>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Empty(deserialized.Projects);
    }

    #endregion

    #region Project Lookup Tests

    [Fact]
    public void FindProject_ByPath_ShouldWork()
    {
        // Arrange
        var registry = new ProjectRegistry
        {
            Projects =
            [
                new ProjectEntry { Path = "/path/to/project1", Name = "Project 1" },
                new ProjectEntry { Path = "/path/to/project2", Name = "Project 2" },
                new ProjectEntry { Path = "/path/to/project3", Name = "Project 3" }
            ]
        };

        // Act
        var found = registry.Projects.Find(p =>
            string.Equals(p.Path, "/path/to/project2", StringComparison.OrdinalIgnoreCase));

        // Assert
        Assert.NotNull(found);
        Assert.Equal("Project 2", found.Name);
    }

    [Fact]
    public void FindProject_CaseInsensitive_ShouldWork()
    {
        // Arrange
        var registry = new ProjectRegistry
        {
            Projects = [new ProjectEntry { Path = "/Path/To/Project", Name = "Test" }]
        };

        // Act
        var found = registry.Projects.Find(p =>
            string.Equals(p.Path, "/path/to/project", StringComparison.OrdinalIgnoreCase));

        // Assert
        Assert.NotNull(found);
    }

    [Fact]
    public void FindProject_NotFound_ShouldReturnNull()
    {
        // Arrange
        var registry = new ProjectRegistry
        {
            Projects = [new ProjectEntry { Path = "/existing/path", Name = "Existing" }]
        };

        // Act
        var found = registry.Projects.Find(p =>
            string.Equals(p.Path, "/nonexistent/path", StringComparison.OrdinalIgnoreCase));

        // Assert
        Assert.Null(found);
    }

    #endregion

    #region Project Operations Tests

    [Fact]
    public void UpdateProject_ShouldUpdateExistingEntry()
    {
        // Arrange
        var registry = new ProjectRegistry
        {
            Projects = [new ProjectEntry { Path = "/test/project", Name = "OldName" }]
        };

        // Act
        var existing = registry.Projects.Find(p => p.Path == "/test/project");
        if (existing != null)
        {
            existing.Name = "NewName";
            existing.LastUsedAt = DateTime.UtcNow;
        }

        // Assert
        Assert.Single(registry.Projects);
        Assert.Equal("NewName", registry.Projects[0].Name);
    }

    [Fact]
    public void RemoveAllByPath_ShouldRemoveMatchingEntries()
    {
        // Arrange
        var registry = new ProjectRegistry
        {
            Projects =
            [
                new ProjectEntry { Path = "/path/to/remove", Name = "Remove" },
                new ProjectEntry { Path = "/path/to/keep", Name = "Keep" }
            ]
        };

        // Act
        registry.Projects.RemoveAll(p =>
            string.Equals(p.Path, "/path/to/remove", StringComparison.OrdinalIgnoreCase));

        // Assert
        Assert.Single(registry.Projects);
        Assert.Equal("Keep", registry.Projects[0].Name);
    }

    #endregion

    #region GetGlobalConfigDir Tests

    [Fact]
    public void GetGlobalConfigDir_ShouldReturnValidPath()
    {
        // Act
        var configDir = ProjectRegistry.GetGlobalConfigDir();

        // Assert
        Assert.NotEmpty(configDir);
        Assert.EndsWith(Path.Combine(".config", "slack"), configDir);
    }

    [Fact]
    public void GetGlobalConfigDir_ShouldContainUserProfilePath()
    {
        // Arrange
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        // Act
        var configDir = ProjectRegistry.GetGlobalConfigDir();

        // Assert
        Assert.StartsWith(userProfile, configDir);
    }

    #endregion

    #region Multiple Projects Tests

    [Fact]
    public void MultipleProjects_ShouldMaintainOrder()
    {
        // Arrange
        var registry = new ProjectRegistry();
        for (int i = 1; i <= 5; i++)
        {
            registry.Projects.Add(new ProjectEntry
            {
                Path = $"/project{i}",
                Name = $"Project {i}"
            });
        }

        // Assert
        Assert.Equal(5, registry.Projects.Count);
        for (int i = 0; i < 5; i++)
        {
            Assert.Equal($"/project{i + 1}", registry.Projects[i].Path);
        }
    }

    [Fact]
    public void Projects_ShouldSupportLinqOperations()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var registry = new ProjectRegistry
        {
            Projects =
            [
                new ProjectEntry { Path = "/a", Name = "A", LastUsedAt = now.AddDays(-3) },
                new ProjectEntry { Path = "/b", Name = "B", LastUsedAt = now.AddDays(-1) },
                new ProjectEntry { Path = "/c", Name = "C", LastUsedAt = now }
            ]
        };

        // Act
        var recentlyUsed = registry.Projects
            .OrderByDescending(p => p.LastUsedAt)
            .Take(2)
            .ToList();

        // Assert
        Assert.Equal(2, recentlyUsed.Count);
        Assert.Equal("/c", recentlyUsed[0].Path);
        Assert.Equal("/b", recentlyUsed[1].Path);
    }

    #endregion
}
