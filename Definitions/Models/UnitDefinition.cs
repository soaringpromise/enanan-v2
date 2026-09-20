namespace EnananV2.Definitions.Models;

public record UnitDefinition(int Id, string Name, int Color, string Emoji, CharacterDefinition[] Characters)
{
    public string RoleName => $"{Name} Fan {Emoji}";
}