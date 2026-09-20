using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using EnananV2.Definitions.Data;
using EnananV2.Definitions.Exceptions;
using EnananV2.Definitions.Models;
using EnananV2.Tools;
using NetCord;
using NetCord.Gateway;

namespace EnananV2.Services;

[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global", Justification = "Dependency Injection")]
public sealed class ValidationService(
    DatabaseService database,
    SekaiCardService sekaiCardService,
    SekaiEventService sekaiEventService)
{
    private const int MaxGuildRoles = 250;
    
    // ============================================================
    //                    DATABASE VALIDATION
    // ============================================================
    public async Task ValidateCustomRoleAccess(ulong guildId)
    {
        await ValidateGuildIsAvailable(guildId);
        await ValidateCustomRolesEnabled(guildId);
    }
    
    public async Task ValidateGuildAccess(ulong guildId)
    {
        await ValidateGuildIsAvailable(guildId);
    }

    private async Task ValidateGuildIsAvailable(ulong guildId)
    {
        if (!await database.Guild.Exists(guildId))
            throw new InvalidRequestException("This server has not been registered yet. Contact an administrator.");

        if (!await database.Guild.IsSetupComplete(guildId))
            throw new InvalidRequestException("This server's setup is incomplete. Contact an administrator.");

        if (!await database.Guild.IsEnabled(guildId))
            throw new InvalidRequestException("Enanan is currently disabled in this server. Contact an administrator.");
    }
    
    public async Task ValidateGuildIsRegistered(ulong guildId)
    {
        if (!await database.Guild.Exists(guildId)) 
            throw new InvalidRequestException("This server has not been registered yet. Contact an administrator.");
    }
    
    public async Task ValidateGuildIsNotRegistered(ulong guildId)
    {
        if (!await database.Guild.Exists(guildId)) return;

        if (await database.Guild.IsSetupComplete(guildId)) 
            throw new InvalidRequestException("This server is already registered.");

        throw new InvalidRequestException("This server has an incomplete registration.");
    }

    private async Task ValidateCustomRolesEnabled(ulong guildId)
    {
        var roleMode = await database.Guild.GetRoleMode(guildId);

        if (roleMode is not (RoleMode.Custom or RoleMode.Both))
            throw new InvalidRequestException("This server does not support custom roles.");
    }

    public async Task ValidateUserDoesNotOwnCustomRole(Guild guild, ulong userId)
    {
        var roleId = await database.GuildMember.GetCustomRoleId(guild.Id, userId);
        if (roleId is null) return;

        if (guild.Roles.ContainsKey(roleId.Value))
            throw new InvalidRequestException("This user already has a custom role.");

        await database.GuildMember.RemoveUserCustomRole(guild.Id, userId);
    }
    
    public async Task<ulong> ValidateUserOwnsCustomRole(Guild guild, ulong userId)
    {
        var roleId = await database.GuildMember.GetCustomRoleId(guild.Id, userId);
        if (roleId is null) throw new InvalidRequestException("This user does not have a custom role.");
        
        if (guild.Roles.ContainsKey(roleId.Value)) return roleId.Value;
        
        await database.GuildMember.RemoveUserCustomRole(guild.Id, userId);
        
        throw new InvalidRequestException("Your custom role no longer exists. Its stored reference has been cleared.");
    }

    public async Task UserAlreadyHasTierProfile(ulong userId)
    {
        if (await database.TierProfile.Exists(userId))
            throw new InvalidRequestException("This user already has a tier profile.");
    }
    
    public async Task<TierProfile> ValidateTierProfileExists(ulong userId)
    {
        var profile = await database.TierProfile.GetTierProfile(userId);

        return profile ?? throw new InvalidRequestException("This user does not have a tier profile.");
    }
    
    
    // ============================================================
    //                     INPUT VALIDATION
    // ============================================================
    

    public void ValidateGuildCanCreateRole(int roleCount)
    {
        if (roleCount >= MaxGuildRoles) 
            throw new InvalidRequestException("This server has hit the maximum role amount.");
    }
    
    public string ValidateRoleTitle(string title)
    {
        title = StringTools.NormalizeWhitespace(title);

        switch (title.Length)
        {
            case < 2: throw new InvalidInputException(title, "Role name must be at least 2 characters long.");
            case > 64: throw new InvalidInputException(title, "Role name cannot be longer than 64 characters.");
        }
        return title;
    }
    
    public void ValidateColor(string input, out Color color)
    {
        input = StringTools.NormalizeWhitespace(input);

        if (input.Length > 64) 
            throw new InvalidInputException(input, "Color name cannot be longer than 64 characters.");

        if (ColorCatalog.NamedColors.TryGetValue(input, out var rgb))
        {
            color = new Color(rgb);
            return;
        }

        if (TryParseHexColor(input, out color)) return;

        var example = ColorCatalog.GetRandomExample();

        throw new InvalidInputException(input,
            $"Unknown color. Use a standard color name (e.g. \"**{example.Key}**\") " +
            $"or a hex color such as **#{example.Value:X6}**.");
    }
    
    public void ValidateEditRequest(string? roleName, string? color)
    {
        if (string.IsNullOrWhiteSpace(roleName) && string.IsNullOrWhiteSpace(color)) 
            throw new InvalidRequestException("At least a new role name or color must be provided.");
    }
    
    public string? ValidateOptionalRoleTitle(string? title)
    {
        return string.IsNullOrWhiteSpace(title) ? null : ValidateRoleTitle(title);
    }
    
    public void ValidateOptionalColor(string? input, out Color? color)
    {
        color = null;
        if (string.IsNullOrWhiteSpace(input)) return;
        ValidateColor(input, out var parsedColor);
        color = parsedColor;
    }
    
    public string ValidateIsv(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) throw new InvalidInputException(input, "ISV cannot be empty.");
        input = StringTools.NormalizeWhitespace(input);
        var parts = input.Split('/');

        if (parts.Length != 2)
            throw new InvalidInputException(input, "Invalid ISV format. Use the format lead/team (e.g. 150/700).");

        if (!double.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out var lead) ||
            !double.TryParse(parts[1], NumberStyles.Number, CultureInfo.InvariantCulture, out var team))
        {
            throw new InvalidInputException(input, "Invalid ISV format. Both values must be numbers (e.g. 150/700).");
        }

        if (lead <= 0 || team <= 0) throw new InvalidInputException(input, "ISV values must be greater than 0.");

        return input;
    }
    
    public void ValidateCardId(int cardId)
    {
        if (cardId <= 0)
            throw new InvalidInputException(cardId, "Card ID must be greater than 0.");

        if (!sekaiCardService.CardExists(cardId))
            throw new InvalidInputException(cardId, "Card ID does not exist.");
    }

    public void ValidateEventId(int? eventId)
    {
        switch (eventId)
        {
            case null:
                return;
            case <= 0:
                throw new InvalidInputException(eventId, "Event ID must be greater than 0.");
        }
        if (!sekaiEventService.EventExists(eventId.Value))
            throw new InvalidInputException(eventId, "Event ID does not exist.");
    }
    
    
    // ============================================================
    //                      HELPER METHODS
    // ============================================================
    private static bool TryParseHexColor(string input, out Color color)
    {
        color = default;
        if (input.StartsWith('#')) input = input[1..];
        if (input.Length != 6) return false;
        
        if (!int.TryParse(input, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb))
            return false;

        color = new Color(rgb);
        return true;
    }
}