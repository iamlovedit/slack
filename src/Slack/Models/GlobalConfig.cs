using System.Text.Json;
using System.Text.Json.Serialization;

namespace Slack.Models;

[JsonSerializable(typeof(GlobalConfig))]
internal partial class GlobalConfigContext : JsonSerializerContext { }

/// <summary>
/// 全局配置，存储系统级设置
/// </summary>
public class GlobalConfig
{
    /// <summary>
    /// UI 语言设置，null 表示跟随系统
    /// </summary>
    public string? Locale { get; set; } = null;

    /// <summary>
    /// 滚动速度: Slow, Normal, Fast
    /// </summary>
    public string ScrollSpeed { get; set; } = "Normal";

    /// <summary>
    /// 颜色主题: Default, Dark, Monokai, Matrix
    /// </summary>
    public string Theme { get; set; } = "Default";

    /// <summary>
    /// 自动检查更新
    /// </summary>
    public bool AutoCheckUpdate { get; set; } = true;

    /// <summary>
    /// 动画风格: Classic, Modern, Minimal, Fancy
    /// </summary>
    public string AnimationStyle { get; set; } = "Modern";

    private static readonly string ConfigFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".config", "slack", "global.json");

    public static GlobalConfig Load()
    {
        try
        {
            if (File.Exists(ConfigFilePath))
            {
                var json = File.ReadAllText(ConfigFilePath);
                return JsonSerializer.Deserialize(json, GlobalConfigContext.Default.GlobalConfig) ?? new GlobalConfig();
            }
        }
        catch
        {
            // 配置文件损坏，返回默认配置
        }
        return new GlobalConfig();
    }

    public void Save()
    {
        var dir = Path.GetDirectoryName(ConfigFilePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var json = JsonSerializer.Serialize(this, GlobalConfigContext.Default.GlobalConfig);
        File.WriteAllText(ConfigFilePath, json);
    }
}
