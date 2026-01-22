using System.Text.Json;
using System.Text.Json.Serialization;

namespace Slack.Models;

[JsonSerializable(typeof(SlackConfig))]
internal partial class SlackConfigContext : JsonSerializerContext { }

public class SlackConfig
{
    public string ProjectName { get; set; } = "my-awesome-project";
    public string Language { get; set; } = "C#";
    public string TechStack { get; set; } = "ASP.NET Core Web API";
    public string PackageManager { get; set; } = "NuGet";
    public string RuntimeVersion { get; set; } = ".NET 10.0";
    public string EnvName { get; set; } = "Production";
    public List<string> CustomModules { get; set; } = [];
    public List<string> CustomTasks { get; set; } = [];

    private static readonly string ConfigFilePath = Path.Combine(
        System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
        ".config", "slack", "config.json");

    private static readonly string LocalConfigPath = Path.Combine(
        Directory.GetCurrentDirectory(), ".slack", "config.json");

    public static SlackConfig LoadLocal()
    {
        try
        {
            if (File.Exists(LocalConfigPath))
            {
                var json = File.ReadAllText(LocalConfigPath);
                return JsonSerializer.Deserialize(json, SlackConfigContext.Default.SlackConfig) ?? new SlackConfig();
            }
        }
        catch
        {
            // ignore
        }
        return new SlackConfig();
    }

    public void SaveLocal()
    {
        var dir = Path.GetDirectoryName(LocalConfigPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var json = JsonSerializer.Serialize(this, SlackConfigContext.Default.SlackConfig);
        File.WriteAllText(LocalConfigPath, json);
    }

    public static SlackConfig Load()
    {
        try
        {
            if (File.Exists(ConfigFilePath))
            {
                var json = File.ReadAllText(ConfigFilePath);
                return JsonSerializer.Deserialize(json, SlackConfigContext.Default.SlackConfig) ?? new SlackConfig();
            }
        }
        catch
        {
            // 配置文件损坏，返回默认配置
        }
        return new SlackConfig();
    }

    public void Save()
    {
        var dir = Path.GetDirectoryName(ConfigFilePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var json = JsonSerializer.Serialize(this, SlackConfigContext.Default.SlackConfig);
        File.WriteAllText(ConfigFilePath, json);
    }

    public string[] GetModules() => Language switch
    {
        "C#" or "F#" => CustomModules.Count > 0 ? [.. CustomModules] :
        [
            "Controllers.Api", "Services.Auth", "Data.Repository", "Middleware.Logging",
            "Models.Entities", "Extensions.DI", "Filters.Exception", "Validators.Request",
            "Handlers.Command", "Providers.Cache", "Workers.Background", "Hubs.SignalR",
            "Mappers.AutoMapper", "Policies.Authorization", "Options.Configuration"
        ],
        "TypeScript" or "JavaScript" => CustomModules.Count > 0 ? [.. CustomModules] :
        [
            "express.router", "prisma.client", "redis.cache", "jwt.auth",
            "socket.handler", "queue.worker", "logger.service", "config.loader",
            "middleware.cors", "validator.schema", "mailer.transport", "storage.s3",
            "graphql.resolver", "webhook.handler", "rate.limiter", "session.store"
        ],
        "Python" => CustomModules.Count > 0 ? [.. CustomModules] :
        [
            "fastapi.routes", "sqlalchemy.models", "celery.tasks", "redis.cache",
            "pydantic.schemas", "alembic.migrations", "pytest.fixtures", "jwt.auth",
            "boto3.storage", "httpx.client", "tenacity.retry", "structlog.logger"
        ],
        "Go" => CustomModules.Count > 0 ? [.. CustomModules] :
        [
            "handlers/api", "services/auth", "repository/postgres", "middleware/logging",
            "models/entities", "config/loader", "pkg/validator", "pkg/cache",
            "workers/consumer", "internal/metrics", "pkg/errors", "pkg/jwt"
        ],
        "Rust" or "Zig" => CustomModules.Count > 0 ? [.. CustomModules] :
        [
            "handlers::api", "services::auth", "db::postgres", "middleware::tracing",
            "models::entities", "config::loader", "utils::validator", "cache::redis",
            "workers::tokio", "metrics::prometheus", "errors::handler", "jwt::claims"
        ],
        "Java" or "Kotlin" or "Scala" => CustomModules.Count > 0 ? [.. CustomModules] :
        [
            "controller.ApiController", "service.AuthService", "repository.UserRepository",
            "config.SecurityConfig", "model.Entity", "dto.Request", "mapper.EntityMapper",
            "filter.JwtFilter", "handler.ExceptionHandler", "util.ValidatorUtil",
            "scheduler.TaskScheduler", "listener.EventListener"
        ],
        "Swift" => CustomModules.Count > 0 ? [.. CustomModules] :
        [
            "ViewControllers.Main", "Models.User", "Services.API", "Utils.Network",
            "Extensions.UIKit", "Managers.Auth", "Views.Custom", "Coordinators.App",
            "Repositories.Data", "Handlers.Error", "Protocols.Service", "Helpers.Date"
        ],
        "PHP" => CustomModules.Count > 0 ? [.. CustomModules] :
        [
            "Controllers\\Api", "Models\\User", "Services\\Auth", "Middleware\\Cors",
            "Repositories\\Data", "Helpers\\Utils", "Events\\UserCreated", "Jobs\\SendEmail",
            "Providers\\App", "Requests\\Store", "Resources\\User", "Policies\\Admin"
        ],
        "Ruby" => CustomModules.Count > 0 ? [.. CustomModules] :
        [
            "controllers/api", "models/user", "services/auth", "lib/utils",
            "jobs/mailer", "serializers/user", "policies/admin", "concerns/trackable",
            "validators/request", "decorators/user", "queries/search", "presenters/api"
        ],
        "C++" or "C" => CustomModules.Count > 0 ? [.. CustomModules] :
        [
            "src/main", "src/utils", "src/handlers", "include/types",
            "lib/network", "lib/crypto", "src/parser", "src/serializer",
            "include/config", "src/logger", "lib/memory", "src/thread_pool"
        ],
        "Elixir" => CustomModules.Count > 0 ? [.. CustomModules] :
        [
            "lib/controllers", "lib/schemas", "lib/contexts", "lib/workers",
            "lib/channels", "lib/plugs", "lib/views", "lib/helpers",
            "lib/services", "lib/repo", "lib/auth", "lib/mailer"
        ],
        "Dart" => CustomModules.Count > 0 ? [.. CustomModules] :
        [
            "lib/screens", "lib/widgets", "lib/models", "lib/services",
            "lib/providers", "lib/utils", "lib/bloc", "lib/repositories",
            "lib/routes", "lib/themes", "lib/extensions", "lib/api"
        ],
        "Haskell" or "OCaml" => CustomModules.Count > 0 ? [.. CustomModules] :
        [
            "App.Main", "App.Types", "App.Handlers", "App.Database",
            "Lib.Utils", "Lib.Parser", "Lib.Config", "App.Routes",
            "App.Auth", "Lib.Crypto", "App.Models", "Lib.Logger"
        ],
        "Clojure" => CustomModules.Count > 0 ? [.. CustomModules] :
        [
            "core", "handlers", "middleware", "db",
            "routes", "auth", "utils", "config",
            "models", "services", "jobs", "cache"
        ],
        "Lua" => CustomModules.Count > 0 ? [.. CustomModules] :
        [
            "init", "handlers", "utils", "config",
            "routes", "middleware", "models", "services",
            "cache", "logger", "auth", "helpers"
        ],
        "Nim" or "V" or "Gleam" => CustomModules.Count > 0 ? [.. CustomModules] :
        [
            "src/main", "src/handlers", "src/utils", "src/config",
            "src/routes", "src/models", "src/services", "src/db",
            "src/auth", "src/cache", "src/logger", "src/helpers"
        ],
        _ => CustomModules.Count > 0 ? [.. CustomModules] :
        [
            "core.module", "utils.service", "api.handler", "data.processor"
        ]
    };

    public string[] GetTaskPrefixes() => Language switch
    {
        "C#" or "F#" =>
        [
            "Compiling", "Building", "Restoring", "Publishing", "Analyzing",
            "Generating", "Packaging", "Signing", "Linking", "Optimizing"
        ],
        "TypeScript" or "JavaScript" =>
        [
            "Transpiling", "Bundling", "Minifying", "Tree-shaking", "Linting",
            "Type-checking", "Hot-reloading", "Compiling", "Optimizing", "Resolving"
        ],
        "Python" =>
        [
            "Installing", "Migrating", "Testing", "Linting", "Type-checking",
            "Formatting", "Building", "Packaging", "Validating", "Collecting"
        ],
        "Go" =>
        [
            "Building", "Compiling", "Linking", "Vetting", "Testing",
            "Formatting", "Generating", "Analyzing", "Benchmarking", "Installing"
        ],
        "Rust" or "Zig" =>
        [
            "Compiling", "Building", "Linking", "Checking", "Testing",
            "Documenting", "Optimizing", "Fetching", "Analyzing", "Clippy-ing"
        ],
        "Java" or "Kotlin" or "Scala" =>
        [
            "Compiling", "Building", "Packaging", "Testing", "Analyzing",
            "Resolving", "Downloading", "Processing", "Generating", "Validating"
        ],
        "Swift" =>
        [
            "Compiling", "Building", "Linking", "Signing", "Archiving",
            "Testing", "Analyzing", "Validating", "Packaging", "Uploading"
        ],
        "PHP" =>
        [
            "Installing", "Optimizing", "Caching", "Testing", "Linting",
            "Migrating", "Seeding", "Publishing", "Clearing", "Compiling"
        ],
        "Ruby" =>
        [
            "Installing", "Bundling", "Migrating", "Testing", "Linting",
            "Precompiling", "Caching", "Loading", "Generating", "Seeding"
        ],
        "C++" or "C" =>
        [
            "Compiling", "Linking", "Building", "Preprocessing", "Assembling",
            "Optimizing", "Archiving", "Testing", "Analyzing", "Generating"
        ],
        "Elixir" =>
        [
            "Compiling", "Fetching", "Building", "Testing", "Formatting",
            "Analyzing", "Releasing", "Migrating", "Seeding", "Caching"
        ],
        "Dart" =>
        [
            "Resolving", "Building", "Compiling", "Testing", "Analyzing",
            "Formatting", "Generating", "Packaging", "Running", "Installing"
        ],
        "Haskell" or "OCaml" =>
        [
            "Compiling", "Building", "Linking", "Testing", "Documenting",
            "Resolving", "Fetching", "Analyzing", "Optimizing", "Packaging"
        ],
        "Clojure" =>
        [
            "Compiling", "Building", "Testing", "Linting", "Packaging",
            "Resolving", "Fetching", "Analyzing", "Optimizing", "Deploying"
        ],
        _ =>
        [
            "Compiling", "Building", "Linking", "Optimizing", "Analyzing"
        ]
    };

    public string[] GetActions() => Language switch
    {
        "C#" or "F#" => CustomTasks.Count > 0 ? [.. CustomTasks] :
        [
            "=> Restoring NuGet packages...",
            "=> Building solution...",
            "=> Running analyzers...",
            "=> Executing unit tests...",
            "=> Generating XML docs...",
            "=> Publishing artifacts...",
            "=> Running code coverage...",
            "=> Checking dependencies...",
            "=> AOT compilation...",
            "=> IL trimming..."
        ],
        "TypeScript" or "JavaScript" => CustomTasks.Count > 0 ? [.. CustomTasks] :
        [
            "=> Resolving dependencies...",
            "=> Running type checks...",
            "=> Bundling with esbuild...",
            "=> Minifying JavaScript...",
            "=> Generating source maps...",
            "=> Running ESLint...",
            "=> Building Docker image...",
            "=> Running Jest tests...",
            "=> Checking TypeScript types...",
            "=> Hot module replacement..."
        ],
        "Python" => CustomTasks.Count > 0 ? [.. CustomTasks] :
        [
            "=> Installing dependencies...",
            "=> Running migrations...",
            "=> Executing pytest...",
            "=> Running mypy checks...",
            "=> Formatting with black...",
            "=> Linting with ruff...",
            "=> Building wheel...",
            "=> Collecting static files...",
            "=> Running coverage...",
            "=> Checking imports..."
        ],
        "Go" => CustomTasks.Count > 0 ? [.. CustomTasks] :
        [
            "=> Downloading modules...",
            "=> Running go vet...",
            "=> Executing tests...",
            "=> Building binary...",
            "=> Running golangci-lint...",
            "=> Generating mocks...",
            "=> Formatting code...",
            "=> Checking race conditions...",
            "=> Building Docker image...",
            "=> Running benchmarks..."
        ],
        "Rust" or "Zig" => CustomTasks.Count > 0 ? [.. CustomTasks] :
        [
            "=> Fetching crates...",
            "=> Compiling dependencies...",
            "=> Running cargo check...",
            "=> Executing cargo test...",
            "=> Running clippy...",
            "=> Building release...",
            "=> Generating docs...",
            "=> Optimizing with LTO...",
            "=> Checking unsafe code...",
            "=> Running miri..."
        ],
        "Java" or "Kotlin" or "Scala" => CustomTasks.Count > 0 ? [.. CustomTasks] :
        [
            "=> Resolving Maven dependencies...",
            "=> Compiling sources...",
            "=> Running JUnit tests...",
            "=> Analyzing with SpotBugs...",
            "=> Generating Javadoc...",
            "=> Building JAR...",
            "=> Running Checkstyle...",
            "=> Processing annotations...",
            "=> Creating Docker image...",
            "=> Running integration tests..."
        ],
        "Swift" => CustomTasks.Count > 0 ? [.. CustomTasks] :
        [
            "=> Resolving Swift packages...",
            "=> Compiling Swift modules...",
            "=> Running XCTest...",
            "=> Analyzing with SwiftLint...",
            "=> Building archive...",
            "=> Signing with certificate...",
            "=> Validating app bundle...",
            "=> Uploading to App Store...",
            "=> Generating documentation...",
            "=> Running UI tests..."
        ],
        "PHP" => CustomTasks.Count > 0 ? [.. CustomTasks] :
        [
            "=> Installing Composer packages...",
            "=> Running migrations...",
            "=> Executing PHPUnit tests...",
            "=> Running PHPStan...",
            "=> Clearing cache...",
            "=> Optimizing autoloader...",
            "=> Compiling assets...",
            "=> Seeding database...",
            "=> Publishing vendors...",
            "=> Running Pint formatter..."
        ],
        "Ruby" => CustomTasks.Count > 0 ? [.. CustomTasks] :
        [
            "=> Installing gems...",
            "=> Running migrations...",
            "=> Executing RSpec tests...",
            "=> Running RuboCop...",
            "=> Precompiling assets...",
            "=> Loading Spring...",
            "=> Seeding database...",
            "=> Caching routes...",
            "=> Generating ERD...",
            "=> Running Brakeman..."
        ],
        "C++" or "C" => CustomTasks.Count > 0 ? [.. CustomTasks] :
        [
            "=> Running CMake configure...",
            "=> Compiling source files...",
            "=> Linking objects...",
            "=> Running ctest...",
            "=> Analyzing with clang-tidy...",
            "=> Generating build files...",
            "=> Optimizing binary...",
            "=> Creating static library...",
            "=> Running memory sanitizer...",
            "=> Stripping debug symbols..."
        ],
        "Elixir" => CustomTasks.Count > 0 ? [.. CustomTasks] :
        [
            "=> Fetching hex packages...",
            "=> Compiling dependencies...",
            "=> Running mix test...",
            "=> Checking with Credo...",
            "=> Running Dialyzer...",
            "=> Building release...",
            "=> Running migrations...",
            "=> Seeding database...",
            "=> Generating docs...",
            "=> Starting Phoenix server..."
        ],
        "Dart" => CustomTasks.Count > 0 ? [.. CustomTasks] :
        [
            "=> Resolving pub packages...",
            "=> Running flutter build...",
            "=> Executing flutter test...",
            "=> Analyzing with dart analyze...",
            "=> Generating code...",
            "=> Building APK/IPA...",
            "=> Running integration tests...",
            "=> Formatting code...",
            "=> Building web app...",
            "=> Creating app bundle..."
        ],
        "Haskell" => CustomTasks.Count > 0 ? [.. CustomTasks] :
        [
            "=> Resolving Cabal packages...",
            "=> Compiling modules...",
            "=> Running HSpec tests...",
            "=> Checking with HLint...",
            "=> Building executable...",
            "=> Generating Haddock docs...",
            "=> Running benchmarks...",
            "=> Optimizing with GHC...",
            "=> Type checking...",
            "=> Building Docker image..."
        ],
        _ => CustomTasks.Count > 0 ? [.. CustomTasks] :
        [
            "=> Resolving dependencies...",
            "=> Running tests...",
            "=> Building project...",
            "=> Optimizing output..."
        ]
    };

    public (string manager, string version) GetPackageInfo() => (PackageManager, RuntimeVersion);
}
