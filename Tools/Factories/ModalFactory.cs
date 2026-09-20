using EnananV2.Definitions.Models;
using NetCord;
using NetCord.Rest;

namespace EnananV2.Tools.Factories;

public static class ModalFactory
{
    public static ModalProperties CreateRegistrationModal()
    {
        return new ModalProperties("admin-register-guild", "Register Guild")
            .AddComponents(
                new LabelProperties("System Message Channel", new ChannelMenuProperties("system-channel-select")
                        .WithChannelTypes([ChannelType.TextGuildChannel])
                        .WithRequired())
                    .WithDescription("Channel where system notifications will be sent if automatic tasks fail."),
                
                new LabelProperties("Welcome Channel", new ChannelMenuProperties("welcome-channel-select")
                        .WithChannelTypes([ChannelType.TextGuildChannel])
                        .WithRequired(false))
                    .WithDescription(
                        "Optional. Select the channel where the bot should send welcome messages for new members."),
                
                new LabelProperties("Role Channel", new ChannelMenuProperties("role-channel-select")
                        .WithChannelTypes([ChannelType.TextGuildChannel])
                        .WithRequired(false))
                    .WithDescription("Optional if no role mode or role types are selected."),
                
                new LabelProperties("Role Mode", 
                        new StringMenuProperties("role-mode-select", EnumTools.CreateSelectOptions<RoleMode>())
                        .WithRequired().WithMaxValues(1))
                    .WithDescription(
                        "Choose which colored role system should be enabled: Project Sekai, custom roles, both, or neither."),
                
                new LabelProperties(
                        "Role Types", new CheckboxGroupProperties("role-type-select", [
                                new CheckboxGroupOptionProperties("Tiering Roles", "tiering"),
                                new CheckboxGroupOptionProperties("Identity Roles", "identity"),
                                new CheckboxGroupOptionProperties("Utility Roles", "utility")
                            ])
                            .WithRequired(false))
                    .WithDescription(
                        "Optional. Select which additional non-colored role categories should be created.")
            );
    }
    
    public static ModalProperties CreateProfileCardStep1Modal()
    {
        return new ModalProperties("profile-card-step-1", "Profile Card (1/4)")
            .AddComponents(
                new LabelProperties("Main Server",
                    new StringMenuProperties("main-server", EnumTools.CreateSelectOptions<GameServer>())
                        .WithRequired().WithMaxValues(1)).WithDescription("Choose your main playing server."),

                new LabelProperties("Timezone", 
                        new TextInputProperties("timezone", TextInputStyle.Short)
                        .WithRequired().WithPlaceholder("e.g. Asia/Tokyo")).WithDescription("Input your timezone."),

                new LabelProperties("Usual Event Tier", 
                        new StringMenuProperties("usual-tier", EnumTools.CreateSelectOptions<TierBracket>())
                        .WithRequired().WithMaxValues(1)).WithDescription("Choose your usual tier target per event."),

                new LabelProperties("Playstyle",
                    new StringMenuProperties("playstyle", EnumTools.CreateSelectOptions<Playstyle>())
                        .WithRequired().WithMaxValues(1)).WithDescription("Choose your usual way of playing the game.")
            );
    }

    public static ModalProperties CreateProfileCardStep2Modal()
    {
        return new ModalProperties("profile-card-step-2", "Profile Card (2/4)")
            .AddComponents(
                new LabelProperties("Game ID",
                    new TextInputProperties("game-id", TextInputStyle.Short)
                        .WithRequired().WithMinLength(18).WithMaxLength(18).WithPlaceholder("Insert your game ID..."))
                    .WithDescription("Input your game ID. You can copy it from your in-game profile."),

                new LabelProperties("Primary Team Talent",
                    new TextInputProperties("primary-team-talent", TextInputStyle.Short)
                        .WithRequired().WithMinLength(1).WithMaxLength(6).WithPlaceholder("e.g. 290000"))
                    .WithDescription("Input your primary fill team's talent."),

                new LabelProperties("Primary Team ISV",
                    new TextInputProperties("primary-team-isv", TextInputStyle.Short)
                        .WithRequired().WithMinLength(6).WithMaxLength(7).WithPlaceholder("e.g. 130/690"))
                    .WithDescription("Input your primary fill team's ISV.")
            );
    }

    public static ModalProperties CreateProfileCardStep3Modal()
    {
        return new ModalProperties("profile-card-step-3", "Profile Card (3/4)")
            .AddComponents(
                new LabelProperties("Heal Team Talent",
                    new TextInputProperties("heal-team-talent", TextInputStyle.Short)
                        .WithRequired(false).WithMaxLength(6).WithPlaceholder("e.g. 276000"))
                    .WithDescription("(Optional) Input your heal fill team's talent."),

                new LabelProperties("Heal Team ISV",
                    new TextInputProperties("heal-team-isv", TextInputStyle.Short)
                        .WithRequired(false).WithMaxLength(7).WithPlaceholder("e.g. 100/600"))
                    .WithDescription("(Optional) Input your heal fill team's ISV."),

                new LabelProperties("Encore Team Talent",
                    new TextInputProperties("encore-team-talent", TextInputStyle.Short)
                        .WithRequired(false).WithMaxLength(6).WithPlaceholder("e.g. 320000"))
                    .WithDescription("(Optional) Input your encore fill team's talent."),

                new LabelProperties("Encore Team ISV",
                    new TextInputProperties("encore-team-isv", TextInputStyle.Short)
                        .WithRequired(false).WithMaxLength(7).WithPlaceholder("e.g. 140/710"))
                    .WithDescription("(Optional) Input your encore fill team's ISV.")
            );
    }

    public static ModalProperties CreateProfileCardStep4Modal()
    {
        return new ModalProperties("profile-card-step-4", "Profile Card (4/4)")
            .AddComponents(
                new LabelProperties("Display Card ID",
                    new TextInputProperties("display-card-id", TextInputStyle.Short)
                        .WithRequired().WithMinLength(1).WithMaxLength(4).WithPlaceholder("e.g. 969"))
                    .WithDescription("The card ID you want displayed in your profile."),

                new LabelProperties("Highest Placement Tier",
                    new StringMenuProperties("highest-tier", EnumTools.CreateSelectOptions<TierBracket>())
                        .WithRequired(false).WithMinValues(0).WithMaxValues(1))
                    .WithDescription("(Optional) Choose the highest tier you've ever placed in."),

                new LabelProperties("Highest Placement Event ID",
                    new TextInputProperties("highest-event", TextInputStyle.Short)
                        .WithRequired(false).WithMaxLength(4).WithPlaceholder("e.g. 157"))
                    .WithDescription("(Optional) Input the event ID of your highest placement.")
            );
    }
}