using EnananV2.Definitions.Data;
using EnananV2.Definitions.Exceptions;
using EnananV2.Definitions.Models;
using EnananV2.Services;
using EnananV2.Tools;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Services.ApplicationCommands;

namespace EnananV2.Discord.Commands;

[SlashCommand(name: "color", description: "Preview or select many colors!")]
public sealed class ColorCommands(
    ImageFactoryService imageFactory,
    ValidationService validator,
    ResponseService responses,
    HttpClient httpClient,
    ILogger<ColorCommands> logger)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("preview", "Preview how a color looks on a Discord username.")]
    public async Task PreviewColorAsChat(
        [SlashCommandParameter(Name = "color", MinLength = 3, Description = "The color as a hex code or an existing color name.")]
        string colorString,
        [SlashCommandParameter(Name = "user", Description = "(Optional) Another user you want to preview as.")]
        GuildUser? queriedUser = null)
    {
        await responses.DeferAsync(Context);
        try
        {
            var user = queriedUser ?? (GuildUser)Context.Interaction.User;

            validator.ValidateColor(colorString, out var color);

            var username = user.Nickname ?? user.GlobalName ?? user.Username;

            var avatarUrl =
                user.GetGuildAvatarUrl(ImageFormat.Png)
                ?? user.GetAvatarUrl(ImageFormat.Png)
                ?? user.DefaultAvatarUrl;

            var decorationUrl =
                user.GetGuildAvatarDecorationUrl()
                ?? user.GetAvatarDecorationUrl();

            var primaryGuild = user.PrimaryGuild;

            var guildTag = primaryGuild?.Tag;
            var guildBadgeUrl = primaryGuild?.GetBadgeUrl(ImageFormat.Png);
            
            var avatarBytes = await DownloadAvatarAsync(avatarUrl);
            var decorationBytes = await DownloadNullableAssetAsync(decorationUrl);
            var guildBadgeBytes = await DownloadNullableAssetAsync(guildBadgeUrl);

            var renderBytes = await imageFactory.GenerateNamePreviewAsync(
                username,
                $"#{color.RawValue:X6}",
                avatarBytes,
                decorationBytes,
                guildTag,
                guildBadgeBytes);

            using var stream = new MemoryStream(renderBytes);

            await responses.ModifyImageResponse(
                Context, $"Here's a chat preview for that color, **{username}**.", "preview", stream);
        }
        catch (InvalidRequestException e)
        {
            await responses.ModifyStatusResponse(Context, e.Message, ResponseType.Warning);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to generate color preview.");

            await responses.ModifyStatusResponse(
                Context,
                "There was an error generating the preview.",
                ResponseType.Error);
        }
    }

    [SubSlashCommand("random", "Display a random named color!")]
    public async Task Random()
    {
        await responses.DeferAsync(Context);
        try
        {
            var user = (GuildUser)Context.Interaction.User;
            var username = user.Nickname ?? user.GlobalName ?? user.Username;
            
            var color = ColorCatalog.GetRandomExample();
            var imageBytes = await imageFactory.GenerateColorPreviewAsync(color.Key, color.Value);
            await using var stream = new MemoryStream(imageBytes);

            await responses.ModifyImageResponse(
                Context,
                $"Here's your color, **{username}**:\n{color.Key} — `#{color.Value:X6}`",
                "random-color",
                stream);
        }
        catch (InvalidRequestException e)
        {
            await responses.ModifyStatusResponse(Context, e.Message, ResponseType.Warning);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to generate color preview.");

            await responses.ModifyStatusResponse(
                Context,
                "There was an error generating the preview.",
                ResponseType.Error);
        }
    }
    
    [SubSlashCommand("list", "I'm not listing every color by hand, but I can show you where they all are.")]
    public async Task ListAllColors()
    {
        try
        {
            var user = (GuildUser)Context.Interaction.User;
            var username = user.Nickname ?? user.GlobalName ?? user.Username;

            var message = StringTools.SafeFormat(
                CustomMessages.GetRandomMessage(CustomMessageType.ColorList), 
                username, "https://enanan.soaringpromise.moe/palette");

            await responses.SendPlainResponse(Context, message);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Failed to display color list for user {UserId}.",
                Context.User.Id);

            await responses.SendStatusResponse(
                Context,
                "There was an error displaying the color list. Try again later or contact the developer.",
                ResponseType.Error);
        }
    }
    
    private async Task<byte[]> DownloadAvatarAsync(ImageUrl url)
    {
        try
        {
            return await httpClient.GetByteArrayAsync(url.ToString());
        }
        catch (HttpRequestException)
        {
            return await File.ReadAllBytesAsync(Path.Combine(AppContext.BaseDirectory, "Resources", "Images", "0.png"));
        }
    }
    
    private async Task<byte[]?> DownloadNullableAssetAsync(ImageUrl? url)
    {
        if (url is null) return null;
        try
        {
            return await httpClient.GetByteArrayAsync(url.ToString());
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}