using EnananV2.Definitions.Data;

namespace EnananV2.Definitions.Models;

public record UnitDefinition(int Id, string Name, int Color, string Emoji, CharacterDefinition[] Characters)
{
    public string RoleName => $"{Name} Fan {Emoji}";
    public string IconUrl => Cdn.Unit($"unit{Id}");
}