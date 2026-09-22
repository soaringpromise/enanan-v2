using NetCord.Services.ApplicationCommands;

namespace EnananV2.Definitions.Models;

public enum RoleMode
{
    None = 0,
    [SlashCommandChoice(Name = "Project SEKAI Characters")]
    ProjectSekai,
    Custom,
    Both
}