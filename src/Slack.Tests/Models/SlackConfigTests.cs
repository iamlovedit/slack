using Xunit;
using Slack.Models;

namespace Slack.Tests.Models;

public class SlackConfigTests
{
    #region Default Values Tests

    [Fact]
    public void NewInstance_ShouldHaveDefaultValues()
    {
        // Act
        var config = new SlackConfig();

        // Assert
        Assert.Equal("my-awesome-project", config.ProjectName);
        Assert.Equal("C#", config.Language);
        Assert.Equal("ASP.NET Core Web API", config.TechStack);
        Assert.Equal("NuGet", config.PackageManager);
        Assert.Equal(".NET 10.0", config.RuntimeVersion);
        Assert.Equal("Production", config.EnvName);
        Assert.Empty(config.CustomModules);
        Assert.Empty(config.CustomTasks);
        Assert.Equal(0, config.BuildDuration);
        Assert.Equal(0, config.WarningMin);
        Assert.Equal(15, config.WarningMax);
        Assert.True(config.RandomPauses);
    }

    #endregion

    #region GetModules Tests

    [Theory]
    [InlineData("C#")]
    [InlineData("F#")]
    public void GetModules_ForDotNetLanguages_ShouldReturnDotNetModules(string language)
    {
        // Arrange
        var config = new SlackConfig { Language = language };

        // Act
        var modules = config.GetModules();

        // Assert
        Assert.NotEmpty(modules);
        Assert.Contains(modules, m => m.Contains("Controllers"));
        Assert.Contains(modules, m => m.Contains("Services"));
    }

    [Theory]
    [InlineData("TypeScript")]
    [InlineData("JavaScript")]
    public void GetModules_ForJsLanguages_ShouldReturnJsModules(string language)
    {
        // Arrange
        var config = new SlackConfig { Language = language };

        // Act
        var modules = config.GetModules();

        // Assert
        Assert.NotEmpty(modules);
        Assert.Contains(modules, m => m.Contains("express"));
        Assert.Contains(modules, m => m.Contains("prisma"));
    }

    [Fact]
    public void GetModules_ForPython_ShouldReturnPythonModules()
    {
        // Arrange
        var config = new SlackConfig { Language = "Python" };

        // Act
        var modules = config.GetModules();

        // Assert
        Assert.NotEmpty(modules);
        Assert.Contains(modules, m => m.Contains("fastapi"));
        Assert.Contains(modules, m => m.Contains("sqlalchemy"));
    }

    [Fact]
    public void GetModules_ForGo_ShouldReturnGoModules()
    {
        // Arrange
        var config = new SlackConfig { Language = "Go" };

        // Act
        var modules = config.GetModules();

        // Assert
        Assert.NotEmpty(modules);
        Assert.Contains(modules, m => m.Contains("handlers"));
        Assert.Contains(modules, m => m.Contains("repository"));
    }

    [Theory]
    [InlineData("Rust")]
    [InlineData("Zig")]
    public void GetModules_ForRustZig_ShouldReturnRustStyleModules(string language)
    {
        // Arrange
        var config = new SlackConfig { Language = language };

        // Act
        var modules = config.GetModules();

        // Assert
        Assert.NotEmpty(modules);
        Assert.Contains(modules, m => m.Contains("::"));
    }

    [Theory]
    [InlineData("Java")]
    [InlineData("Kotlin")]
    [InlineData("Scala")]
    public void GetModules_ForJvmLanguages_ShouldReturnJvmModules(string language)
    {
        // Arrange
        var config = new SlackConfig { Language = language };

        // Act
        var modules = config.GetModules();

        // Assert
        Assert.NotEmpty(modules);
        Assert.Contains(modules, m => m.Contains("controller") || m.Contains("Controller"));
        Assert.Contains(modules, m => m.Contains("service") || m.Contains("Service"));
    }

    [Fact]
    public void GetModules_WithCustomModules_ShouldReturnCustomModules()
    {
        // Arrange
        var customModules = new List<string> { "Custom.Module1", "Custom.Module2" };
        var config = new SlackConfig
        {
            Language = "C#",
            CustomModules = customModules
        };

        // Act
        var modules = config.GetModules();

        // Assert
        Assert.Equal(2, modules.Length);
        Assert.Equal("Custom.Module1", modules[0]);
        Assert.Equal("Custom.Module2", modules[1]);
    }

    [Fact]
    public void GetModules_ForUnknownLanguage_ShouldReturnDefaultModules()
    {
        // Arrange
        var config = new SlackConfig { Language = "UnknownLang" };

        // Act
        var modules = config.GetModules();

        // Assert
        Assert.NotEmpty(modules);
        Assert.Contains(modules, m => m.Contains("core"));
    }

    #endregion

    #region GetTaskPrefixes Tests

    [Theory]
    [InlineData("C#", "Compiling")]
    [InlineData("F#", "Building")]
    [InlineData("TypeScript", "Transpiling")]
    [InlineData("JavaScript", "Bundling")]
    [InlineData("Python", "Installing")]
    [InlineData("Go", "Building")]
    [InlineData("Rust", "Compiling")]
    [InlineData("Java", "Compiling")]
    public void GetTaskPrefixes_ForLanguage_ShouldContainExpectedPrefix(string language, string expectedPrefix)
    {
        // Arrange
        var config = new SlackConfig { Language = language };

        // Act
        var prefixes = config.GetTaskPrefixes();

        // Assert
        Assert.Contains(prefixes, p => p == expectedPrefix);
    }

    [Fact]
    public void GetTaskPrefixes_ShouldReturnNonEmptyArray()
    {
        // Arrange
        var languages = new[] { "C#", "TypeScript", "Python", "Go", "Rust", "Java", "Swift", "PHP", "Ruby" };

        foreach (var language in languages)
        {
            var config = new SlackConfig { Language = language };

            // Act
            var prefixes = config.GetTaskPrefixes();

            // Assert
            Assert.NotEmpty(prefixes);
            Assert.True(prefixes.Length >= 5, $"Language {language} should have at least 5 task prefixes");
        }
    }

    #endregion

    #region GetActions Tests

    [Theory]
    [InlineData("C#", "NuGet")]
    [InlineData("TypeScript", "esbuild")]
    [InlineData("Python", "pytest")]
    [InlineData("Go", "go vet")]
    [InlineData("Rust", "cargo")]
    [InlineData("Java", "Maven")]
    public void GetActions_ForLanguage_ShouldContainLanguageSpecificAction(string language, string expectedKeyword)
    {
        // Arrange
        var config = new SlackConfig { Language = language };

        // Act
        var actions = config.GetActions();

        // Assert
        Assert.Contains(actions, a => a.Contains(expectedKeyword, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetActions_WithCustomTasks_ShouldReturnCustomTasks()
    {
        // Arrange
        var customTasks = new List<string>
        {
            "=> Running custom task 1...",
            "=> Executing custom task 2..."
        };
        var config = new SlackConfig
        {
            Language = "C#",
            CustomTasks = customTasks
        };

        // Act
        var actions = config.GetActions();

        // Assert
        Assert.Equal(2, actions.Length);
        Assert.Equal(customTasks[0], actions[0]);
        Assert.Equal(customTasks[1], actions[1]);
    }

    [Fact]
    public void GetActions_AllActionsShouldStartWithArrow()
    {
        // Arrange
        var config = new SlackConfig { Language = "C#" };

        // Act
        var actions = config.GetActions();

        // Assert
        foreach (var action in actions)
        {
            Assert.StartsWith("=>", action);
        }
    }

    #endregion

    #region GetPackageInfo Tests

    [Fact]
    public void GetPackageInfo_ShouldReturnCorrectTuple()
    {
        // Arrange
        var config = new SlackConfig
        {
            PackageManager = "npm",
            RuntimeVersion = "Node 20.0"
        };

        // Act
        var (manager, version) = config.GetPackageInfo();

        // Assert
        Assert.Equal("npm", manager);
        Assert.Equal("Node 20.0", version);
    }

    #endregion

    #region File Path Tests

    [Fact]
    public void GetLocalConfigPath_ShouldReturnCorrectPath()
    {
        // Arrange
        var basePath = "/test/project";

        // Act
        var configPath = SlackConfig.GetLocalConfigPath(basePath);

        // Assert
        Assert.Equal(Path.Combine("/test/project", ".slack", "config.json"), configPath);
    }

    #endregion

    #region Serialization Tests

    [Fact]
    public void SaveAndLoad_ShouldPreserveConfiguration()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"slack_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var config = new SlackConfig
            {
                ProjectName = "test-project",
                Language = "Python",
                TechStack = "FastAPI",
                PackageManager = "pip",
                RuntimeVersion = "Python 3.12",
                EnvName = "Development",
                BuildDuration = 30,
                WarningMin = 5,
                WarningMax = 10,
                RandomPauses = false,
                CustomModules = ["module1", "module2"],
                CustomTasks = ["task1", "task2"],
                CustomMessages = ["msg1"]
            };

            // Act
            config.SaveLocal(tempDir);
            var loaded = SlackConfig.LoadLocal(tempDir);

            // Assert
            Assert.Equal(config.ProjectName, loaded.ProjectName);
            Assert.Equal(config.Language, loaded.Language);
            Assert.Equal(config.TechStack, loaded.TechStack);
            Assert.Equal(config.PackageManager, loaded.PackageManager);
            Assert.Equal(config.RuntimeVersion, loaded.RuntimeVersion);
            Assert.Equal(config.EnvName, loaded.EnvName);
            Assert.Equal(config.BuildDuration, loaded.BuildDuration);
            Assert.Equal(config.WarningMin, loaded.WarningMin);
            Assert.Equal(config.WarningMax, loaded.WarningMax);
            Assert.Equal(config.RandomPauses, loaded.RandomPauses);
            Assert.Equal(config.CustomModules, loaded.CustomModules);
            Assert.Equal(config.CustomTasks, loaded.CustomTasks);
            Assert.Equal(config.CustomMessages, loaded.CustomMessages);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void LoadLocal_WithNonExistentFile_ShouldReturnDefaultConfig()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}");

        // Act
        var config = SlackConfig.LoadLocal(nonExistentPath);

        // Assert
        Assert.NotNull(config);
        Assert.Equal("my-awesome-project", config.ProjectName); // Default value
    }

    #endregion

    #region Language Coverage Tests

    [Theory]
    [InlineData("C#")]
    [InlineData("F#")]
    [InlineData("TypeScript")]
    [InlineData("JavaScript")]
    [InlineData("Python")]
    [InlineData("Go")]
    [InlineData("Rust")]
    [InlineData("Zig")]
    [InlineData("Java")]
    [InlineData("Kotlin")]
    [InlineData("Scala")]
    [InlineData("Swift")]
    [InlineData("PHP")]
    [InlineData("Ruby")]
    [InlineData("C++")]
    [InlineData("C")]
    [InlineData("Elixir")]
    [InlineData("Dart")]
    [InlineData("Haskell")]
    [InlineData("OCaml")]
    [InlineData("Clojure")]
    [InlineData("Lua")]
    [InlineData("Nim")]
    [InlineData("V")]
    [InlineData("Gleam")]
    public void GetModules_ForAllSupportedLanguages_ShouldReturnNonEmptyModules(string language)
    {
        // Arrange
        var config = new SlackConfig { Language = language };

        // Act
        var modules = config.GetModules();

        // Assert
        Assert.NotEmpty(modules);
        Assert.True(modules.Length >= 4, $"Language {language} should have at least 4 modules");
    }

    #endregion
}
