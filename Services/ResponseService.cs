using EnananV2.Definitions.Data;
using EnananV2.Definitions.Models;
using EnananV2.Tools;
using EnananV2.Tools.Factories;
using NetCord;
using NetCord.Rest;
using NetCord.Services;

namespace EnananV2.Services;

public sealed class ResponseService(RestClient client)
{
    public async Task DeferAsync(IInteractionContext ctx, bool ephemeral = false)
    {
        await ctx.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(ephemeral ? MessageFlags.Ephemeral : null));
    }
    
    public async Task ModifyStatusResponse(IInteractionContext ctx, string message, ResponseType type)
    {
        var embed = EmbedFactory.CreateResponseEmbed(message, type);

        await ctx.Interaction.ModifyResponseAsync(options =>
        {
            options.Embeds = [embed];
        });
    }
    
    public async Task ModifyImageResponse(IInteractionContext ctx, string message, string filename, Stream image)
    {
        if (image.CanSeek && image.Position != 0) image.Position = 0;

        var fullFilename = $"{filename}.png";
        var attachment = new AttachmentProperties(fullFilename, image);
        var embed = EmbedFactory.CreateImageEmbed($"attachment://{fullFilename}", message);

        await ctx.Interaction.ModifyResponseAsync(options =>
        {
            options.Embeds = [embed];
            options.Attachments = [attachment];
        });
    }
    
    public Task ModifyEmbedResponse(IInteractionContext ctx, EmbedProperties embed)
    {
        return ctx.Interaction.ModifyResponseAsync(options =>
        {
            options.Embeds = [embed];
        });
    }
    
    public async Task SendPlainResponse(IInteractionContext ctx, string message)
    {
        var embed = EmbedFactory.CreatePlainEmbed(message);
        var properties = new InteractionMessageProperties().AddEmbeds(embed);

        await ctx.Interaction.SendResponseAsync(InteractionCallback.Message(properties));
    }

    public async Task SendStatusResponse(IInteractionContext ctx, string message, ResponseType type)
    {
        var embed = EmbedFactory.CreateResponseEmbed(message, type);

        var properties = new InteractionMessageProperties()
            .AddEmbeds(embed)
            .WithFlags(type is ResponseType.Success ? 0 : MessageFlags.Ephemeral);

        await ctx.Interaction.SendResponseAsync(InteractionCallback.Message(properties));
    }

    public async Task SendImageResponse(IInteractionContext ctx, string message, string filename, Stream image)
    {
        if (image.CanSeek && image.Position != 0) image.Position = 0;

        var fullFilename = $"{filename}.png";

        var attachment = new AttachmentProperties(fullFilename, image);
        var embed = EmbedFactory.CreateImageEmbed($"attachment://{fullFilename}", message);

        var properties = new InteractionMessageProperties()
            .AddEmbeds(embed)
            .AddAttachments(attachment);

        await ctx.Interaction.SendResponseAsync(InteractionCallback.Message(properties));
    }

    public async Task SendWelcomeMessage(ulong channelId, ulong userId)
    {
        var message = CustomMessages.GetRandomMessage(CustomMessageType.Welcome);
        message = StringTools.SafeFormat(message, userId);

        var bannerUrl = CustomAssets.GetRandomWelcomeBanner();
        var embed = EmbedFactory.CreateImageEmbed(bannerUrl, message);

        await client.SendMessageAsync(channelId, new MessageProperties().AddEmbeds(embed));
    }

    public async Task SendSystemMessage(ulong channelId, string message)
    {
        var embed = EmbedFactory.CreatePlainEmbed(message);
        await client.SendMessageAsync(channelId, new MessageProperties().AddEmbeds(embed));
    }
    
    public async Task SendEmbedResponse(IInteractionContext ctx, EmbedProperties embed)
    {
        var properties = new InteractionMessageProperties().AddEmbeds(embed);
        await ctx.Interaction.SendResponseAsync(InteractionCallback.Message(properties));
    }
    
    public async Task SendFieldResponse(
        IInteractionContext ctx, string title, IEnumerable<(string Name, string Value, bool Inline)> fields)
    {
        var embed = EmbedFactory.FieldEmbed(title, fields);
        var properties = new InteractionMessageProperties().AddEmbeds(embed);
        await ctx.Interaction.SendResponseAsync(InteractionCallback.Message(properties));
    }
}