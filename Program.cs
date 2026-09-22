using EnananV2.Database;
using EnananV2.Database.Configuration;
using EnananV2.Database.Repositories;
using EnananV2.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");

var token = builder.Configuration["Discord:Token"]
            ?? throw new InvalidOperationException("Discord token not configured.");

builder.Services
    .AddSerilog((_, configuration) =>
    {
        configuration
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                "logs/enanan-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 10,
                rollOnFileSizeLimit: true,
                fileSizeLimitBytes: 10 * 1024 * 1024,
                outputTemplate:
                "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] " +
                "{SourceContext}: {Message:lj}{NewLine}{Exception}");
    })
    .AddDiscordGateway(options =>
    {
        options.Token = token;
        options.Intents = GatewayIntents.All;
        options.Presence = new PresenceProperties(UserStatusType.Online)
        {
            Activities =
            [
                new UserActivityProperties("Custom Status", UserActivityType.Custom)
                    { State = "🎨 Here to paint your world~!" }
            ]
        };
    })
    .AddApplicationCommands()
    .AddComponentInteractions<ButtonInteraction, ButtonInteractionContext>()
    .AddComponentInteractions<StringMenuInteraction, StringMenuInteractionContext>()
    .AddComponentInteractions<RoleMenuInteraction, RoleMenuInteractionContext>()
    .AddComponentInteractions<ModalInteraction, ModalInteractionContext>()
    .AddSingleton<DatabaseMigrator>()
    .AddSingleton<SqliteConnector>()
    .AddSingleton<DatabaseInitializer>()
    .AddHttpClient()
    .AddSingleton<GuildRepository>()
    .AddSingleton<GuildMemberRepository>()
    .AddSingleton<TierProfileRepository>()
    .AddSingleton<GuildSetupRepository>()
    .AddSingleton<DatabaseService>()
    .AddSingleton<SekaiCardService>()
    .AddSingleton<SekaiEventService>()
    .AddSingleton<TierProfileDraftService>()
    .AddSingleton<ValidationService>()
    .AddSingleton<GuildNotificationService>()
    .AddSingleton<ResponseService>()
    .AddSingleton<RoleSetupService>()
    .AddSingleton<CustomRoleService>()
    .AddSingleton<ImageFactoryService>()
    .AddGatewayHandlers(typeof(Program).Assembly);

var host = builder.Build();

host.AddModules(typeof(Program).Assembly);

var initializer = host.Services.GetRequiredService<DatabaseInitializer>();
var migrator = host.Services.GetRequiredService<DatabaseMigrator>();
var sekaiCardService = host.Services.GetRequiredService<SekaiCardService>();
var sekaiEventService = host.Services.GetRequiredService<SekaiEventService>();

try
{
    PrepareQuestPdf();

    initializer.Initialize();
    migrator.Migrate();

    await sekaiCardService.InitializeAsync();
    await sekaiEventService.InitializeAsync();

    Log.Information("Starting Enanan.");
    await host.RunAsync();
}
catch (Exception e)
{
    Log.Fatal(e, "Enanan terminated unexpectedly.");
    Environment.ExitCode = 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

return;

void PrepareQuestPdf()
{
    QuestPDF.Settings.License = LicenseType.Community;
    QuestPDF.Settings.EnableDebugging = false;
    QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;
    QuestPDF.Settings.UseEnvironmentFonts = false;
    
    var fontDirectory = Path.Combine(AppContext.BaseDirectory, "Resources", "Fonts");
    
    RegisterFont("gg sans", "gg sans Regular.ttf");
    RegisterFont("gg sans", "gg sans Medium.ttf");
    RegisterFont("gg sans", "gg sans Semibold.ttf");
    RegisterFont("gg sans", "gg sans Bold.ttf");
    
    RegisterFont("Segoe UI", "segoeui.ttf");
    
    RegisterFont("Noto Sans", "NotoSans-SemiBold.ttf");
    RegisterFont("Noto Sans JP", "NotoSansJP-SemiBold.ttf");
    RegisterFont("Noto Sans Math", "NotoSansMath-Regular.ttf");
    RegisterFont("Noto Sans Symbols", "NotoSansSymbols-SemiBold.ttf");
    RegisterFont("Noto Sans Symbols 2", "NotoSansSymbols2-Regular.ttf");
    RegisterFont("Noto Sans Runic", "NotoSansRunic-Regular.ttf");
    RegisterFont("Noto Sans Egyptian Hieroglyphs", "NotoSansEgyptianHieroglyphs-Regular.ttf");
    
    RegisterFont("Noto Color Emoji", "NotoColorEmoji-Regular.ttf");
    
    var warmup = Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Size(20, 20);
            page.Margin(0);
            page.Content()
                .AlignMiddle()
                .AlignCenter()
                .Text("Ena")
                .FontFamily("gg sans")
                .FontSize(2);
        });
    });
    _ = warmup.GenerateImages();
    return;

    void RegisterFont(string name, string file)
    {
        using var stream = File.OpenRead(Path.Combine(fontDirectory, file));
        FontManager.RegisterFontWithCustomName(name, stream);
    }
}