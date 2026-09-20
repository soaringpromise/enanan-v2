# Enanan Bot

**Enanan** is a Discord bot mainly designed for Project SEKAI tiering communities, with a focus on custom role
management, member customization, global information profiles, and miscellaneous server utilities.

It provides server administrators with configurable systems for managing roles and onboarding, while giving members 
convenient ways to personalize their presence in the server.

## Features

<details>
<summary><strong>Project SEKAI Roles</strong></summary>

Enanan can provide ready-to-use role menus based on Project SEKAI characters and units.

Depending on the server's configuration, members can choose from:

* Roles based on all 26 characters' colors;
* Roles based on all 6 subunits' colors.

Server administrators can decide if this feature is enabled during setup.

</details>

<details>
<summary><strong>Custom Roles</strong></summary>

Servers can allow members to have their own personalized role.

Members can:

* Create their custom role;
* Change its name;
* Change its color;
* Delete it when they no longer want it.

Enanan keeps track of each member's role so that users only manage the role assigned to them.

</details>

<details>
<summary><strong>Color Previews</strong></summary>

Before changing a role color, members can preview how a Discord message would look with that color applied to their name.

The preview uses the selected member's:

* Display name;
* Avatar;
* Avatar decoration, when available;
* Primary server tag, when available.

This makes it easier to test colors without repeatedly editing a role.

<img width=50% src="https://i.ibb.co/Lh8VRzXH/preview.png" alt="Chat preview">

</details>

<details>
<summary><strong>Tier Profiles</strong></summary>

Members can create a profile containing information related to Project SEKAI tiering.

Profiles can include information such as:

* Usual event tier;
* Main game server;
* Preferred playstyle;
* Fill Teams Talent & ISV/Boost display;
* Other tiering information.

This gives other members quick access to useful information when organizing teams or looking for players.

</details>

<details>
<summary><strong>Welcome System</strong></summary>

Servers can configure a welcome channel for new members.

Enanan can use this channel as part of the server's onboarding setup, keeping welcome-related configuration integrated with the rest of the bot.

</details>

<details>
<summary><strong>Server Setup</strong></summary>

Administrators can configure Enanan directly through Discord.

During setup, a server can choose:

* Whether Project SEKAI roles are enabled;
* Whether custom roles are enabled;
* Whether both systems should be available;
* Which utility role selection panels should be posted;
* Which channel should be used for welcome functionality.

Enanan creates and configures the selected systems automatically.

</details>

## Role Modes

Administrators can choose how role customization works in their server.

| Mode          | Description                                      |
| ------------- | ------------------------------------------------ |
| Project SEKAI | Uses the built-in Project SEKAI role system.     |
| Custom        | Allows members to manage their own custom roles. |
| Both          | Enables both Project SEKAI and custom roles.     |
| None          | Disables both role systems.                      |

Additional utility role categories can be enabled or disabled separately during server setup.

## For Server Administrators

Enanan is intended to keep initial configuration relatively simple.

Once the bot is added to a server, administrators can register and configure the server using the provided 
administration commands. Enanan then handles the creation of the configured role systems and member-facing panels.

Administrative actions are restricted to users with the appropriate Discord permissions.

<details>
<summary><strong>What Enanan manages</strong></summary>

Depending on the server configuration, Enanan may manage:

* Project SEKAI-themed roles;
* Member custom roles;
* Role selection panels;
* Welcome Channel configuration;
* Member tier profiles;
* Server-specific bot settings.

Each Discord server has its own independent configuration.

</details>

## For Members

Once Enanan has been configured by the server administrators, members can interact with the features that have been 
enabled for that server.

Depending on the server, this may include:

* Selecting Project SEKAI-themed roles;
* Creating and editing a personal role;
* Previewing role colors;
* Creating and updating a tier profile;
* Viewing information used for tiering and team organization.

Not every feature is necessarily enabled in every server.

## Permissions

Enanan requires the Discord permissions necessary to perform the features enabled by the server.

For role-related functionality, its bot role must be placed above the roles that Enanan is expected to create or manage.

Enanan does not give members unrestricted access to server roles. Member-facing commands only operate on roles and information that the bot is designed to manage.

## Scope

Enanan is primarily built around Project SEKAI tiering communities, particularly servers that want more structured role 
customization and tiering utilities. This doesn't mean Enanan cannot be used in other servers just for its utility
features and custom role support.

The bot is not intended to replace general-purpose moderation or administration bots. Its focus is instead on 
community-specific features such as role personalization, Project SEKAI-themed role selection, member profiles, and related 
server utilities.

## Status

Enanan is actively developed and its available features may change as the project evolves.

Bug reports, suggestions, and feature requests can be submitted through the repository's issue tracker.
