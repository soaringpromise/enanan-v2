using NetCord.Services.ApplicationCommands;

namespace EnananV2.Definitions.Models;

public enum Playstyle
{
    Casual,
    Dedicated,
    Tierer,
    Collector,
    [SlashCommandChoice(Name = "Rhythm Game Only")]
    Rhythm,
    [SlashCommandChoice(Name = "Story Only")]
    Story,
    [SlashCommandChoice(Name = "MySEKAI Only")]
    MySekai,
    Other
}