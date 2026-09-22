using EnananV2.Definitions.Data;
using EnananV2.Definitions.Models;
using NetCord;
using NetCord.Rest;

namespace EnananV2.Tools.Factories;

public static class RoleMenuFactory
{
    public static async Task<RestMessage> SendCharacterRolePanelAsync(
        TextGuildChannel channel, UnitDefinition unit, IReadOnlyDictionary<string, ulong> characterRoleIds)
    {
        var options = unit.Characters.Select(character =>
            new StringMenuSelectOptionProperties(character.RoleName, characterRoleIds[character.Name].ToString()));
        
        var menu = new StringMenuProperties("characterRoleSelect")
            .WithPlaceholder($"Choose a {unit.Name} character...")
            .WithOptions(options);

        var embed = EmbedFactory.CreateRolePanelEmbed(
            $"{unit.Name} Character",
            $"Choose a **{unit.Name}** character and use their signature color.",
            unit.IconUrl,
            unit.Color);
        
        var message = new MessageProperties().WithComponents([menu]).WithEmbeds([embed]);
        return await channel.SendMessageAsync(message);
    }

    public static async Task<RestMessage> SendUnitRolePanelAsync(
        TextGuildChannel channel, IReadOnlyDictionary<string, ulong> unitRoleIds)
    {
        var options = CharacterCatalog.Units.Values
            .OrderBy(unit => unit.Id)
            .Select(unit => new StringMenuSelectOptionProperties(unit.RoleName, unitRoleIds[unit.Name].ToString()));

        var menu = new StringMenuProperties("unitRoleSelect")
            .WithPlaceholder("Choose a unit...")
            .WithOptions(options);

        var embed = EmbedFactory.CreateRolePanelEmbed(
            "Unit",
            "Choose your favorite Project SEKAI unit and wear its signature color.",
            Cdn.Ena("units"),
            0x00CCBB);
        
        var message = new MessageProperties().WithComponents([menu]).WithEmbeds([embed]);
        return await channel.SendMessageAsync(message);
    }

    public static async Task<RestMessage> SendGenericRolePanelAsync(
        TextGuildChannel channel, IReadOnlyDictionary<string, ulong> roleIds, string componentId,
        string property, string description, string placeholder, string iconUrl, int color = 0xCCAA88)
    {
        var options = roleIds.Select(role =>
            new StringMenuSelectOptionProperties(role.Key, role.Value.ToString()));
        
        var menu = new StringMenuProperties(componentId)
            .WithPlaceholder(placeholder)
            .WithOptions(options);

        var embed = EmbedFactory.CreateRolePanelEmbed(property, description, iconUrl, color);
        var message = new MessageProperties().WithComponents([menu]).WithEmbeds([embed]);
        return await channel.SendMessageAsync(message);
    }
}