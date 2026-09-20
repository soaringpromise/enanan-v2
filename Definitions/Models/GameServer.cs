using NetCord.Services.ApplicationCommands;

namespace EnananV2.Definitions.Models;

public enum GameServer
{
    [SlashCommandChoice(Name = "EN")] En,
    [SlashCommandChoice(Name = "JP")] Jp,
    [SlashCommandChoice(Name = "CN")] Cn,
    [SlashCommandChoice(Name = "KR")] Kr
}