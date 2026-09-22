using System.Text;
using EnananV2.Definitions.Data;
using EnananV2.Definitions.Models;
using NetCord;
using NetCord.Rest;

namespace EnananV2.Tools.Factories;

public static class EmbedFactory
{
    private static readonly string EnaPfpUrl = Cdn.Ena("pfp");
    private static readonly string ArtFooterIconUrl = Cdn.Ena("ena_art");
    private static readonly string HappyFooterIconUrl = Cdn.Ena("ena_happy");
    private static readonly string MessageFooterIconUrl = Cdn.Ena("ena_message");
    private static readonly Color EnaColor = new(0xCCAA88);
    
    private const string AuthorName = "えななん (@enanan_bot)";
    private const string FooterText = "Enanan Bot";
    private const string BotUrl = "https://enanan.soaringpromise.moe";
    
    
    public static EmbedProperties CreatePlainEmbed(string message, bool stats = true)
    {
        return CreateDefaultEmbed().WithDescription(stats ? StringTools.AddFakeStats(message) : message);
    }

    public static EmbedProperties CreateResponseEmbed(string message, ResponseType type)
    {
        var (color, status, icon) = type switch
        {
            ResponseType.Success => (new Color(0x48B02C), "Success", Cdn.Status("success")),
            ResponseType.Warning => (new Color(0xFFBB33), "Warning", Cdn.Status("warning")),
            ResponseType.Error => (new Color(0xFF4141), "Error", Cdn.Status("error")),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        return new EmbedProperties()
            .WithColor(color)
            .WithAuthor(new EmbedAuthorProperties()
                .WithName($"Enanan ・ {status}")
                .WithIconUrl(icon))
            .WithDescription(message)
            .WithFooter(new EmbedFooterProperties()
                .WithText("Enanan Bot ・ Automated response").WithIconUrl(MessageFooterIconUrl));
    }

    public static EmbedProperties CreateImageEmbed(string imageUrl, string? message = null)
    {
        return string.IsNullOrWhiteSpace(message)
            ? CreateDefaultEmbed().WithImage(new EmbedImageProperties(imageUrl))
                .WithDescription(StringTools.AddFakeStats(string.Empty))
            : CreateDefaultEmbed().WithImage(new EmbedImageProperties(imageUrl))
                .WithDescription(StringTools.AddFakeStats(message));
    }

    public static EmbedProperties CreateTierProfileEmbed(
        TierProfile profile, string username, string avatarUrl, string cardUrl, Color characterColor, string? eventName)
    {
        var description = new StringBuilder();

        description.AppendLine($"**Usual Tier Target:** {EnumTools.GetDisplayName(profile.UsualTier)}");
        description.AppendLine($"**Game Server:** {EnumTools.GetDisplayName(profile.Server)}");
        description.AppendLine($"**Playstyle:** {EnumTools.GetDisplayName(profile.Playstyle)}");

        description.AppendLine();

        StringTools.AppendTeam(description, "Primary", profile.Primary);
        StringTools.AppendTeam(description, "Heal", profile.Heal);
        StringTools.AppendTeam(description, "Encore", profile.Encore);
        description.AppendLine();

        description.AppendLine(
            $"**Highest Tier:** {(profile.HighestTier.HasValue ? StringTools.FormatTier(profile.HighestTier.Value) : "—")}");

        description.AppendLine($"**Highest Ranked Event:** {eventName ?? "—"}");
        description.AppendLine();
        description.AppendLine($"**Timezone:** {StringTools.GetUtcOffset(profile.TimeZone)}");
        description.AppendLine($"**Player ID:** {profile.GameId}");

        return new EmbedProperties()
            .WithColor(characterColor)
            .WithAuthor(new EmbedAuthorProperties()
                .WithName($"{username}'s Tier Profile")
                .WithIconUrl(avatarUrl))
            .WithDescription(description.ToString())
            .WithThumbnail(new EmbedThumbnailProperties(cardUrl))
            .WithFooter(new EmbedFooterProperties()
                .WithText("Enanan Bot ・ Forward this profile to share it anywhere on Discord!")
                .WithIconUrl(HappyFooterIconUrl));
    }
    
    public static EmbedProperties CreateRolePanelEmbed(string property, string description, string iconUrl, int color = 0xCCAA88)
    {
        return new EmbedProperties()
            .WithColor(new Color(color))
            .WithAuthor(new EmbedAuthorProperties()
                .WithName($"Enanan ・ {property} Role Selection")
                .WithIconUrl(iconUrl))
            .WithDescription(description)
            .WithFooter(new EmbedFooterProperties()
                .WithIconUrl(ArtFooterIconUrl)
                .WithText("Enanan Bot ・ Roles can be changed at any time!"));
    }
    
    public static EmbedProperties FieldEmbed(
        string title, IEnumerable<(string Name, string Value, bool Inline)> fieldData)
    {
        var fields = fieldData.Select(field =>
            new EmbedFieldProperties()
                .WithName(field.Name)
                .WithValue(field.Value)
                .WithInline(field.Inline));

        return CreateDefaultEmbed().WithTitle(title).WithFields(fields);
    }

    public static EmbedProperties CreateGeneralHelpEmbed()
    {
        return CreateDefaultEmbed()
            .WithTitle("Enanan Help — General")
            .WithDescription(
                """
                Enanan provides tier profiles, custom roles, color utilities, Project SEKAI role management, and server configuration tools.

                Use one of the help categories below for more detailed information.
                """)
            .AddFields(
                new EmbedFieldProperties()
                    .WithName("Help Categories")
                    .WithValue(
                        """
                        `/help tier-profile` — Tier profile creation, viewing, and editing.
                        `/help roles` — Personal custom role management.
                        `/help colors` — Color previews and the supported color palette.
                        `/help admin` — Server setup and administration.
                        """)
                    .WithInline(false));
    }

    public static EmbedProperties CreateTierProfileHelpEmbed()
    {
        return CreateDefaultEmbed()
            .WithTitle("Enanan Help — Tier Profiles")
            .WithDescription(
                "Tier profiles store information about how you play and tier in Project SEKAI.")
            .AddFields(
                new EmbedFieldProperties()
                    .WithName("Profile Management")
                    .WithValue(
                        """
                        `/tier-profile create`
                        Creates a new tier profile using the guided setup.

                        `/tier-profile view [user]`
                        Displays your profile or another user's profile.

                        `/tier-profile delete`
                        Permanently deletes your tier profile.
                        """)
                    .WithInline(false),
                new EmbedFieldProperties()
                    .WithName("Editing")
                    .WithValue(
                        """
                        `/tier-profile edit target` — Change your usual tier target.
                        `/tier-profile edit server` — Change your game server.
                        `/tier-profile edit timezone` — Change your timezone.
                        `/tier-profile edit playstyle` — Change your playstyle.
                        `/tier-profile edit card` — Change your displayed card.
                        """)
                    .WithInline(false),
                new EmbedFieldProperties()
                    .WithName("Teams")
                    .WithValue(
                        "`/tier-profile edit team` — Updates your Primary, Heal, or Encore team. Talent and ISV are " +
                        "always updated together.")
                    .WithInline(false));
    }

    public static EmbedProperties CreateRoleHelpEmbed()
    {
        return CreateDefaultEmbed()
            .WithTitle("Enanan Help — Custom Roles")
            .WithDescription(
                """
                Custom roles let you create a personal Discord role with your own title and color.

                Custom roles must be enabled by the server administrator, and each user can own one custom role at a time.
                """)
            .AddFields(
                new EmbedFieldProperties()
                    .WithName("Custom Role Creation")
                    .WithValue(
                        "`/role create title color` — Creates your custom role. Provide a **title** and either a supported color name or hexadecimal color.")
                    .WithInline(false),
                new EmbedFieldProperties()
                    .WithName("Custom Role Updating")
                    .WithValue(
                        "`/role edit [new-title] [new-color]` — Updates your existing custom role. You can change the **title**, **color**, or both. Leaving one option empty keeps its current value.")
                    .WithInline(false),
                new EmbedFieldProperties()
                    .WithName("Custom Role Deletion")
                    .WithValue(
                        "`/role delete` — Deletes your custom role from the server. You can create another one afterwards if custom roles remain enabled.")
                    .WithInline(false),
                new EmbedFieldProperties()
                    .WithName("Choosing a Color")
                    .WithValue(
                        "Use `/color preview color` to preview a color or `/color list` to browse all supported named colors. Custom roles accept either a supported **color name** or a **hexadecimal color**.")
                    .WithInline(false));
    }

    public static EmbedProperties CreateColorHelpEmbed()
    {
        return CreateDefaultEmbed()
            .WithTitle("Enanan Help — Colors")
            .WithDescription(
                "Color commands help you choose a color before using it for your custom role.")
            .AddFields(
                new EmbedFieldProperties()
                    .WithName("Color Preview")
                    .WithValue(
                        "`/color preview color [user]` — Generates a visual preview of a username using the selected color. You can also preview another server member to see the color with their name and avatar.")
                    .WithInline(false),
                new EmbedFieldProperties()
                    .WithName("Random Color")
                    .WithValue(
                        "`/color random` — Display a random named color with its name and code!")
                    .WithInline(false),
                new EmbedFieldProperties()
                    .WithName("Color List")
                    .WithValue(
                        "`/color list` — Provides a link to Enanan's complete named color palette.")
                    .WithInline(false));
    }

    public static EmbedProperties CreateAdminHelpEmbed()
    {
        return CreateDefaultEmbed()
            .WithTitle("Enanan Help — Administration")
            .WithDescription(
                "Commands for configuring and maintaining Enanan on the server.")
            .AddFields(
                new EmbedFieldProperties()
                    .WithName("Server Registration")
                    .WithValue(
                        "`/admin register` — Opens the server registration menu and configures the initial system channel, welcome channel, role settings, and predefined role groups.")
                    .WithInline(false),
                new EmbedFieldProperties()
                    .WithName("Channels")
                    .WithValue(
                        """
                        `/admin system channel` — Sets or updates the system message channel.
                        `/admin welcome channel` — Sets or updates the welcome channel.
                        `/admin welcome-delete` — Disables welcome messages.
                        """)
                    .WithInline(false),
                new EmbedFieldProperties()
                    .WithName("Role Types")
                    .WithValue(
                        """
                        **Tiering** — Filler, co-op, and natburn roles.
                        **Identity** — Pronoun and identity roles.
                        **Utility** — Opt-in roles for announcements, pull parties, and streams.
                        """)
                    .WithInline(false),
                new EmbedFieldProperties()
                    .WithName("Custom Role Administration")
                    .WithValue(
                        """
                        `/admin role create user title color` — Create a custom role for a user.
                        `/admin role update user [new-title] [new-color]` — Update a user's custom role.
                        `/admin role delete user` — Delete a user's custom role.
                        """)
                    .WithInline(false),
                new EmbedFieldProperties()
                    .WithName("User Registration")
                    .WithValue(
                        """
                        `/admin register-user` — Manually register a server member.
                        `/admin unregister-user` — Manually remove a user's registration using their Discord user ID. Useful if automatic cleanup fails after someone leaves.
                        """)
                    .WithInline(false));
    }
    
    private static EmbedAuthorProperties CreateDefaultAuthor()
    {
        return new EmbedAuthorProperties()
            .WithIconUrl(EnaPfpUrl)
            .WithName(AuthorName)
            .WithUrl(BotUrl);
    }

    private static EmbedFooterProperties CreateDefaultFooter()
    {
        return new EmbedFooterProperties()
            .WithText(FooterText)
            .WithIconUrl(ArtFooterIconUrl);
    }

    private static EmbedProperties CreateDefaultEmbed()
    {
        return new EmbedProperties()
            .WithAuthor(CreateDefaultAuthor())
            .WithFooter(CreateDefaultFooter())
            .WithTimestamp(DateTime.UtcNow)
            .WithColor(EnaColor);
    }
}