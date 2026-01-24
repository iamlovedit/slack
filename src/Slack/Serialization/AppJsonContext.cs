using System.Text.Json;
using System.Text.Json.Serialization;
using Slack.Commands;

namespace Slack.Serialization;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(GitHubRelease))]
internal partial class AppJsonContext : JsonSerializerContext
{
}
