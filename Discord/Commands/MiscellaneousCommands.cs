using EnananV2.Definitions.Data;
using EnananV2.Definitions.Models;
using EnananV2.Services;
using EnananV2.Tools;
using NetCord.Services.ApplicationCommands;

namespace EnananV2.Discord.Commands;

[SlashCommand("misc", "Miscellaneous commands.")]
public sealed class MiscellaneousCommands(ResponseService responses) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("credits", "Displays general bot information.")]
    public async Task MiscInfoCommand()
    {
        var contributors = new List<(string Title, string Info, bool Inline)>
        {
            (
                "Developer:",
                $"{CustomAssets.Twitter.Display} [mia.dev (@soaringpromise)](<https://x.com/soaringpromise>)\n" +
                $"{CustomAssets.GitHub.Display} [Mia (@soaringpromise)](<https://github.com/soaringpromise>)\n" +
                $"{CustomAssets.Bluesky.Display} [mia.dev (@soaringpromise.moe)](<https://bsky.app/profile/soaringpromise.moe>)",
                false
            ),
            (
                "Icon Art",
                $"{CustomAssets.Twitter.Display} [Xin (@XinChan_)](<https://x.com/XinChan_>)",
                false
            ),
            (
                "Source Code",
                $"{CustomAssets.GitHub.Display} [enanan-v2 on GitHub](<https://github.com/soaringpromise/enanan-v2>)",
                false
            ),
            (
                "Dev Team Server",
                $"{CustomAssets.Discord.Display} [The Cheesecake Factory](<https://discord.gg/486Sh9EfHR>)",
                false
            )
        };
        await responses.SendFieldResponse(Context, "Credits & Contributors", contributors);
    }
    
    [SlashCommand("donate", "Support the bot's development.")]
    public async Task DonateCommand()
    {
        var message = StringTools.SafeFormat(
            CustomMessages.GetRandomMessage(CustomMessageType.Donation), "https://ko-fi.com/soaringpromise");
    
        await responses.SendPlainResponse(Context, message);
    }
    
    [SlashCommand("invite", "Invite me to your server!")]
    public async Task InviteCommand()
    {
        const string inviteLink = "https://discord.com/oauth2/authorize?client_id=1460482352001323185&permissions=4503876921584753&integration_type=0&scope=bot";
        var message = StringTools.SafeFormat(
            CustomMessages.GetRandomMessage(CustomMessageType.Invite), inviteLink);
        
        await responses.SendPlainResponse(Context, message);
    }
}