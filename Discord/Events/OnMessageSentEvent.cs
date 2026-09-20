using EnananV2.Definitions.Data;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace EnananV2.Discord.Events;

public class OnMessageSentEvent : IMessageCreateGatewayHandler
{
    public async ValueTask HandleAsync(Message message)
    {
        if (message.Author.IsBot || string.IsNullOrWhiteSpace(message.Content)) return;
        await TryEasterEggAsync(message);
    }

    private static async Task TryEasterEggAsync(Message message)
    {
        var chosen = CustomAssets.GetRandomEasterEgg();
        if (chosen is null) return;
        try
        {
            await message.AddReactionAsync(new ReactionEmojiProperties(chosen.Name, chosen.Id));
        }
        catch (RestException) { /* do nothing */ }
    }
}