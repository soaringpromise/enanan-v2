using EnananV2.Services;
using EnananV2.Tools.Factories;
using NetCord.Services.ApplicationCommands;

namespace EnananV2.Discord.Commands;

[SlashCommand("help", "Displays help information.")]
public sealed class HelpCommands(ResponseService responses) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("general", "Displays general bot information.")]
    public async Task General() 
        => await responses.SendEmbedResponse(Context, EmbedFactory.CreateGeneralHelpEmbed());

    [SubSlashCommand("tier-profile", "Displays tier profile command information.")]
    public async Task TierProfile() 
        => await responses.SendEmbedResponse(Context, EmbedFactory.CreateTierProfileHelpEmbed());

    [SubSlashCommand("roles", "Displays custom role command information.")]
    public async Task Roles() =>
        await responses.SendEmbedResponse(Context, EmbedFactory.CreateRoleHelpEmbed());

    [SubSlashCommand("colors", "Displays color command information.")]
    public async Task Colors() =>
        await responses.SendEmbedResponse(Context, EmbedFactory.CreateColorHelpEmbed());

    [SubSlashCommand("admin", "Displays administrator command information.")]
    public async Task Admin() 
        => await responses.SendEmbedResponse(Context, EmbedFactory.CreateAdminHelpEmbed());
}