using EnananV2.Definitions.Data;
using EnananV2.Definitions.Models;
using NetCord;
using NetCord.Rest;

namespace EnananV2.Tools.Factories;

public static class RoleMenuFactory
{
    public static async Task<RestMessage> SendCharacterRolePanelAsync(
        TextGuildChannel channel,
        UnitDefinition unit,
        IReadOnlyDictionary<string, ulong> characterRoleIds)
    {
        var options = unit.Characters.Select(character =>
            new StringMenuSelectOptionProperties(character.RoleName, characterRoleIds[character.Name].ToString()));
        
        var menu = new StringMenuProperties($"characterRoleSelect:{StringTools.StringToIdentifier(unit.Name)}")
            .WithPlaceholder($"Select a {unit.Name} character...")
            .WithOptions(options);

        var embed = EmbedFactory.CreateSimpleColorEmbed(
            $"Select a {unit.Name} role below. Selecting it again removes it.", unit.Color);
        
        var message = new MessageProperties().WithComponents([menu]).WithEmbeds([embed]);
        return await channel.SendMessageAsync(message);
    }

    public static async Task<RestMessage> SendUnitRolePanelAsync(
        TextGuildChannel channel,
        IReadOnlyDictionary<string, ulong> unitRoleIds)
    {
        var options = CharacterCatalog.Units.Values.Select(unitOption =>
            new StringMenuSelectOptionProperties(unitOption.RoleName, unitRoleIds[unitOption.Name].ToString()));

        var menu = new StringMenuProperties("unitRoleSelect")
            .WithPlaceholder("Select a unit...")
            .WithOptions(options);

        var embed = EmbedFactory.CreateSimpleColorEmbed(
            "Select a unit role below. Selecting it again removes it.");
        
        var message = new MessageProperties().WithComponents([menu]).WithEmbeds([embed]);
        return await channel.SendMessageAsync(message);
    }

    public static async Task<RestMessage> SendGenericRolePanelAsync(
        TextGuildChannel channel, IReadOnlyDictionary<string, ulong> roleIds, string componentId)
    {
        var options = roleIds.Keys.Select(roleName =>
            new StringMenuSelectOptionProperties(roleName, roleIds[roleName].ToString()));
        
        var menu = new StringMenuProperties(componentId)
            .WithPlaceholder("Select a role...")
            .WithOptions(options);

        var embed = EmbedFactory.CreateSimpleColorEmbed(
            "Select a role below to add it, select it again to remove it. " +
            "Use the menu again to add or remove another.");
        
        var message = new MessageProperties().WithComponents([menu]).WithEmbeds([embed]);
        return await channel.SendMessageAsync(message);
    }
}