namespace EnananV2.Definitions.Models;

public record CharacterDefinition(int Id, string Name, int Color, string Emoji)
{
    public string RoleName => $"{Name} Fan {Emoji}";
}