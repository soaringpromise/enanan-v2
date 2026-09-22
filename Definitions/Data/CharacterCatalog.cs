using System.Collections.Frozen;
using EnananV2.Definitions.Models;

namespace EnananV2.Definitions.Data;

public static class CharacterCatalog
{
    public static readonly FrozenDictionary<int, UnitDefinition> Units = new[] 
        {
            new UnitDefinition(1, "Leo/need", 0x4455DD, "💫",
                [
                    new CharacterDefinition(1, "Ichika", 0x33AAEE, "🎸"),
                    new CharacterDefinition(2, "Saki", 0xFFDD44, "🎹"),
                    new CharacterDefinition(3, "Honami", 0xEE6666, "🥁"),
                    new CharacterDefinition(4, "Shiho", 0xBBDD22, "🍜")
                ]),

            new UnitDefinition(2, "MORE MORE JUMP!", 0x88DD44, "🍀",
                [
                    new CharacterDefinition(5, "Minori", 0xFFCCAA, "🌸"),
                    new CharacterDefinition(6, "Haruka", 0x99CCFF, "🐧"),
                    new CharacterDefinition(7, "Airi", 0xFFAACC, "🍑"),
                    new CharacterDefinition(8, "Shizuku", 0x99EEDD, "💧")
                ]),

            new UnitDefinition(3, "Vivid BAD SQUAD", 0xEE1166, "🎤",
                [
                    new CharacterDefinition(9, "Kohane", 0xFF6699, "🐹"),
                    new CharacterDefinition(10, "An", 0x00BBDD, "🎧"),
                    new CharacterDefinition(11, "Akito", 0xFF7722, "🥞"),
                    new CharacterDefinition(12, "Toya", 0x0077DD, "☕")
                ]),

            new UnitDefinition(4, "Wonderlands x Showtime", 0xFF9900, "🎪",
                [
                    new CharacterDefinition(13, "Tsukasa", 0xFFBB00, "🌟"),
                    new CharacterDefinition(14, "Emu", 0xFF66BB, "🍬"),
                    new CharacterDefinition(15, "Nene", 0x33DD99, "🎮"),
                    new CharacterDefinition(16, "Rui", 0xBB88EE, "🎈")
                ]),

            new UnitDefinition(5, "Nightcord at 25:00", 0x884499, "💻",
                [
                    new CharacterDefinition(17, "Kanade", 0xBB6688, "🎼"),
                    new CharacterDefinition(18, "Mafuyu", 0x8888CC, "❄️"),
                    new CharacterDefinition(19, "Ena", 0xCCAA88, "🎨"),
                    new CharacterDefinition(20, "Mizuki", 0xDDAACC, "🎀")
                ]),

            new UnitDefinition(6, "Virtual Singers", 0x00CCBB, "🎶",
                [
                    new CharacterDefinition(21, "Miku", 0x33CCBB, "🎵"),
                    new CharacterDefinition(22, "Rin", 0xFFCC11, "🍊"),
                    new CharacterDefinition(23, "Len", 0xFFEE11, "🍌"),
                    new CharacterDefinition(24, "Luka", 0xFFBBCC, "🐙"),
                    new CharacterDefinition(25, "MEIKO", 0xDD4444, "🍷"),
                    new CharacterDefinition(26, "KAITO", 0x3366CC, "🍨")
                ])
        }
        .ToFrozenDictionary(x => x.Id);

    public static readonly FrozenDictionary<int, CharacterDefinition> Characters =
        Units.Values
            .SelectMany(x => x.Characters)
            .ToFrozenDictionary(x => x.Id);
}