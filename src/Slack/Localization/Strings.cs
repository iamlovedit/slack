using System.Globalization;
using Slack.Models;

namespace Slack.Localization;

/// <summary>
/// 本地化字符串类（手动部分）
/// </summary>
public static partial class Strings
{
    private static string? _currentLocale;

    /// <summary>
    /// 当前语言代码
    /// </summary>
    public static string CurrentLocale
    {
        get
        {
            if (_currentLocale != null)
                return _currentLocale;

            // 从全局配置加载
            var config = GlobalConfig.Load();
            if (!string.IsNullOrEmpty(config.Locale))
            {
                _currentLocale = config.Locale;
                return _currentLocale;
            }

            // 跟随系统
            var systemLang = CultureInfo.CurrentUICulture.Name;
            _currentLocale = systemLang.StartsWith("zh", StringComparison.OrdinalIgnoreCase) ? "zh-CN" : "en-US";
            return _currentLocale;
        }
        set => _currentLocale = value;
    }

    /// <summary>
    /// 重置语言缓存，下次获取时重新从配置加载
    /// </summary>
    public static void ResetLocale() => _currentLocale = null;

    /// <summary>
    /// 获取格式化的本地化字符串
    /// </summary>
    public static string Format(string template, params object[] args)
    {
        try
        {
            return string.Format(template, args);
        }
        catch
        {
            return template;
        }
    }
}
