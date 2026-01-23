using System.Text.Json;
using System.Text.Json.Serialization;

namespace Slack.Models;

/// <summary>
/// Represents a registered project entry
/// </summary>
public class ProjectEntry
{
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUsedAt { get; set; } = DateTime.UtcNow;
}

[JsonSerializable(typeof(ProjectRegistry))]
internal partial class ProjectRegistryContext : JsonSerializerContext { }

/// <summary>
/// Registry for tracking all initialized projects
/// </summary>
public class ProjectRegistry
{
    public List<ProjectEntry> Projects { get; set; } = [];

    private static readonly string RegistryFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".config", "slack", "projects.json");

    /// <summary>
    /// Register a project in the registry
    /// </summary>
    public static void RegisterProject(string projectPath, string projectName)
    {
        var registry = Load();
        var normalizedPath = Path.GetFullPath(projectPath);

        var existing = registry.Projects.Find(p =>
            string.Equals(p.Path, normalizedPath, StringComparison.OrdinalIgnoreCase));

        if (existing != null)
        {
            existing.Name = projectName;
            existing.LastUsedAt = DateTime.UtcNow;
        }
        else
        {
            registry.Projects.Add(new ProjectEntry
            {
                Path = normalizedPath,
                Name = projectName,
                CreatedAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow
            });
        }

        registry.Save();
    }

    /// <summary>
    /// Unregister a project from the registry
    /// </summary>
    public static void UnregisterProject(string projectPath)
    {
        var registry = Load();
        var normalizedPath = Path.GetFullPath(projectPath);

        registry.Projects.RemoveAll(p =>
            string.Equals(p.Path, normalizedPath, StringComparison.OrdinalIgnoreCase));

        registry.Save();
    }

    /// <summary>
    /// Get all registered projects
    /// </summary>
    public static List<ProjectEntry> GetAllProjects()
    {
        return Load().Projects;
    }

    /// <summary>
    /// Load the registry from disk
    /// </summary>
    public static ProjectRegistry Load()
    {
        try
        {
            if (File.Exists(RegistryFilePath))
            {
                var json = File.ReadAllText(RegistryFilePath);
                return JsonSerializer.Deserialize(json, ProjectRegistryContext.Default.ProjectRegistry)
                       ?? new ProjectRegistry();
            }
        }
        catch
        {
            // Registry file corrupted, return empty registry
        }
        return new ProjectRegistry();
    }

    /// <summary>
    /// Save the registry to disk
    /// </summary>
    public void Save()
    {
        var dir = Path.GetDirectoryName(RegistryFilePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var json = JsonSerializer.Serialize(this, ProjectRegistryContext.Default.ProjectRegistry);
        File.WriteAllText(RegistryFilePath, json);
    }

    /// <summary>
    /// Delete the registry file
    /// </summary>
    public static void DeleteRegistryFile()
    {
        if (File.Exists(RegistryFilePath))
        {
            File.Delete(RegistryFilePath);
        }
    }

    /// <summary>
    /// Get the global config directory path
    /// </summary>
    public static string GetGlobalConfigDir()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config", "slack");
    }
}
