using Xunit;

namespace Slack.Tests.Commands;

/// <summary>
/// GuidCommand 相关功能的测试
/// 由于 GuidCommand 直接依赖 AnsiConsole 和 ClipboardService，
/// 这里测试的是 GUID 格式化逻辑的正确性
/// </summary>
public class GuidFormatterTests
{
    #region GUID Format Tests

    [Fact]
    public void GuidFormat_D_ShouldProduceHyphenatedFormat()
    {
        // Arrange
        var guid = Guid.Parse("12345678-1234-1234-1234-123456789012");

        // Act
        var formatted = guid.ToString("D");

        // Assert
        Assert.Equal("12345678-1234-1234-1234-123456789012", formatted);
        Assert.Equal(36, formatted.Length);
        Assert.Contains("-", formatted);
    }

    [Fact]
    public void GuidFormat_N_ShouldProduceNoHyphensFormat()
    {
        // Arrange
        var guid = Guid.Parse("12345678-1234-1234-1234-123456789012");

        // Act
        var formatted = guid.ToString("N");

        // Assert
        Assert.Equal("12345678123412341234123456789012", formatted);
        Assert.Equal(32, formatted.Length);
        Assert.DoesNotContain("-", formatted);
    }

    [Fact]
    public void GuidFormat_B_ShouldProduceBracesFormat()
    {
        // Arrange
        var guid = Guid.Parse("12345678-1234-1234-1234-123456789012");

        // Act
        var formatted = guid.ToString("B");

        // Assert
        Assert.Equal("{12345678-1234-1234-1234-123456789012}", formatted);
        Assert.StartsWith("{", formatted);
        Assert.EndsWith("}", formatted);
    }

    [Fact]
    public void GuidFormat_P_ShouldProduceParenthesesFormat()
    {
        // Arrange
        var guid = Guid.Parse("12345678-1234-1234-1234-123456789012");

        // Act
        var formatted = guid.ToString("P");

        // Assert
        Assert.Equal("(12345678-1234-1234-1234-123456789012)", formatted);
        Assert.StartsWith("(", formatted);
        Assert.EndsWith(")", formatted);
    }

    [Fact]
    public void GuidFormat_X_ShouldProduceHexFormat()
    {
        // Arrange
        var guid = Guid.Parse("12345678-1234-1234-1234-123456789012");

        // Act
        var formatted = guid.ToString("X");

        // Assert
        Assert.StartsWith("{0x", formatted);
        Assert.Contains("{0x12,0x34,", formatted);
    }

    [Fact]
    public void GuidFormat_Upper_ShouldProduceUppercaseFormat()
    {
        // Arrange
        var guid = Guid.Parse("abcdefab-abcd-abcd-abcd-abcdefabcdef");

        // Act
        var formatted = guid.ToString("D").ToUpperInvariant();

        // Assert
        Assert.Equal("ABCDEFAB-ABCD-ABCD-ABCD-ABCDEFABCDEF", formatted);
        Assert.DoesNotContain("a", formatted);
        Assert.DoesNotContain("b", formatted);
        Assert.DoesNotContain("c", formatted);
    }

    #endregion

    #region GUID Generation Tests

    [Fact]
    public void NewGuid_ShouldGenerateUniqueGuids()
    {
        // Arrange & Act
        var guids = Enumerable.Range(0, 100)
            .Select(_ => Guid.NewGuid())
            .ToList();

        // Assert
        Assert.Equal(100, guids.Distinct().Count());
    }

    [Fact]
    public void NewGuid_ShouldNotGenerateEmptyGuid()
    {
        // Arrange & Act
        var guids = Enumerable.Range(0, 100)
            .Select(_ => Guid.NewGuid())
            .ToList();

        // Assert
        Assert.DoesNotContain(Guid.Empty, guids);
    }

    #endregion

    #region Format Validation Tests

    [Theory]
    [InlineData("D")]
    [InlineData("N")]
    [InlineData("B")]
    [InlineData("P")]
    [InlineData("X")]
    public void ValidFormats_ShouldNotThrow(string format)
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act & Assert
        var exception = Record.Exception(() => guid.ToString(format));
        Assert.Null(exception);
    }

    [Fact]
    public void AllFormats_ShouldProduceDifferentResults()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var formatD = guid.ToString("D");
        var formatN = guid.ToString("N");
        var formatB = guid.ToString("B");
        var formatP = guid.ToString("P");
        var formatX = guid.ToString("X");

        // Assert - 各格式应该不同
        var allFormats = new[] { formatD, formatN, formatB, formatP, formatX };
        Assert.Equal(5, allFormats.Distinct().Count());
    }

    #endregion

    #region Batch Generation Tests

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(100)]
    public void BatchGeneration_ShouldGenerateCorrectCount(int count)
    {
        // Arrange & Act
        var guids = new List<string>();
        for (int i = 0; i < count; i++)
        {
            guids.Add(Guid.NewGuid().ToString("D"));
        }

        // Assert
        Assert.Equal(count, guids.Count);
        Assert.Equal(count, guids.Distinct().Count()); // 应该都是唯一的
    }

    [Fact]
    public void BatchGeneration_WithDifferentFormats_ShouldWork()
    {
        // Arrange
        var formats = new[] { "D", "N", "B", "P", "X" };
        var results = new Dictionary<string, List<string>>();

        // Act
        foreach (var format in formats)
        {
            results[format] = new List<string>();
            for (int i = 0; i < 5; i++)
            {
                var guid = Guid.NewGuid();
                results[format].Add(guid.ToString(format));
            }
        }

        // Assert
        foreach (var format in formats)
        {
            Assert.Equal(5, results[format].Count);
            Assert.Equal(5, results[format].Distinct().Count());
        }
    }

    #endregion

    #region Clipboard Content Format Tests

    [Fact]
    public void MultipleGuids_JoinedByNewline_ShouldFormatCorrectly()
    {
        // Arrange
        var guids = new List<string>
        {
            "12345678-1234-1234-1234-123456789012",
            "abcdefab-abcd-abcd-abcd-abcdefabcdef"
        };

        // Act
        var clipboardText = string.Join(Environment.NewLine, guids);

        // Assert
        Assert.Contains(Environment.NewLine, clipboardText);
        Assert.Equal(2, clipboardText.Split(Environment.NewLine).Length);
    }

    [Fact]
    public void SingleGuid_ShouldNotContainNewline()
    {
        // Arrange
        var guids = new List<string> { Guid.NewGuid().ToString("D") };

        // Act
        var clipboardText = string.Join(Environment.NewLine, guids);

        // Assert
        Assert.DoesNotContain(Environment.NewLine, clipboardText);
    }

    #endregion
}
