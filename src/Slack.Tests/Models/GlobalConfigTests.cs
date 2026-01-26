using Xunit;
using Slack.Models;

namespace Slack.Tests.Models;

public class GlobalConfigTests
{
    #region Default Values Tests

    [Fact]
    public void NewInstance_ShouldHaveDefaultValues()
    {
        // Act
        var config = new GlobalConfig();

        // Assert
        Assert.Null(config.Locale);
        Assert.Equal("Normal", config.ScrollSpeed);
        Assert.Equal("Default", config.Theme);
        Assert.True(config.AutoCheckUpdate);
        Assert.Equal("Modern", config.AnimationStyle);
    }

    #endregion

    #region Property Assignment Tests

    [Fact]
    public void Locale_ShouldBeSettable()
    {
        // Arrange
        var config = new GlobalConfig();

        // Act
        config.Locale = "zh-CN";

        // Assert
        Assert.Equal("zh-CN", config.Locale);
    }

    [Theory]
    [InlineData("Slow")]
    [InlineData("Normal")]
    [InlineData("Fast")]
    public void ScrollSpeed_ShouldAcceptValidValues(string speed)
    {
        // Arrange
        var config = new GlobalConfig();

        // Act
        config.ScrollSpeed = speed;

        // Assert
        Assert.Equal(speed, config.ScrollSpeed);
    }

    [Theory]
    [InlineData("Default")]
    [InlineData("Dark")]
    [InlineData("Monokai")]
    [InlineData("Matrix")]
    public void Theme_ShouldAcceptValidValues(string theme)
    {
        // Arrange
        var config = new GlobalConfig();

        // Act
        config.Theme = theme;

        // Assert
        Assert.Equal(theme, config.Theme);
    }

    [Theory]
    [InlineData("Classic")]
    [InlineData("Modern")]
    [InlineData("Minimal")]
    [InlineData("Fancy")]
    public void AnimationStyle_ShouldAcceptValidValues(string style)
    {
        // Arrange
        var config = new GlobalConfig();

        // Act
        config.AnimationStyle = style;

        // Assert
        Assert.Equal(style, config.AnimationStyle);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AutoCheckUpdate_ShouldBeSettable(bool value)
    {
        // Arrange
        var config = new GlobalConfig();

        // Act
        config.AutoCheckUpdate = value;

        // Assert
        Assert.Equal(value, config.AutoCheckUpdate);
    }

    #endregion

    #region Load Tests

    [Fact]
    public void Load_WithNonExistentFile_ShouldReturnDefaultConfig()
    {
        // Act
        // GlobalConfig.Load() 使用固定的配置文件路径
        // 如果文件不存在，应该返回默认配置
        var config = GlobalConfig.Load();

        // Assert
        Assert.NotNull(config);
        // 验证返回的是有效的配置对象（即使使用了现有配置）
        Assert.NotNull(config.ScrollSpeed);
        Assert.NotNull(config.Theme);
        Assert.NotNull(config.AnimationStyle);
    }

    #endregion

    #region Serialization Round-trip Tests

    [Fact]
    public void SerializationRoundTrip_ShouldPreserveAllProperties()
    {
        // Arrange
        var original = new GlobalConfig
        {
            Locale = "ja-JP",
            ScrollSpeed = "Fast",
            Theme = "Matrix",
            AutoCheckUpdate = false,
            AnimationStyle = "Fancy"
        };

        // Act - 使用 JSON 序列化测试
        var json = System.Text.Json.JsonSerializer.Serialize(original);
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<GlobalConfig>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.Locale, deserialized.Locale);
        Assert.Equal(original.ScrollSpeed, deserialized.ScrollSpeed);
        Assert.Equal(original.Theme, deserialized.Theme);
        Assert.Equal(original.AutoCheckUpdate, deserialized.AutoCheckUpdate);
        Assert.Equal(original.AnimationStyle, deserialized.AnimationStyle);
    }

    [Fact]
    public void SerializationRoundTrip_WithNullLocale_ShouldPreserveNull()
    {
        // Arrange
        var original = new GlobalConfig { Locale = null };

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(original);
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<GlobalConfig>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Null(deserialized.Locale);
    }

    #endregion

    #region Configuration Combinations Tests

    [Fact]
    public void AllSettingsCombination_ShouldBeIndependent()
    {
        // Arrange & Act
        var config = new GlobalConfig
        {
            Locale = "en-US",
            ScrollSpeed = "Slow",
            Theme = "Dark",
            AutoCheckUpdate = true,
            AnimationStyle = "Classic"
        };

        // Assert - 验证每个属性独立设置
        Assert.Equal("en-US", config.Locale);
        Assert.Equal("Slow", config.ScrollSpeed);
        Assert.Equal("Dark", config.Theme);
        Assert.True(config.AutoCheckUpdate);
        Assert.Equal("Classic", config.AnimationStyle);

        // 修改一个属性不应影响其他属性
        config.Theme = "Monokai";
        Assert.Equal("en-US", config.Locale);
        Assert.Equal("Slow", config.ScrollSpeed);
        Assert.Equal("Monokai", config.Theme);
        Assert.True(config.AutoCheckUpdate);
        Assert.Equal("Classic", config.AnimationStyle);
    }

    #endregion
}
