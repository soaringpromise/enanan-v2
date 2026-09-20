using System.Collections.Frozen;
using EnananV2.Definitions.Models;

namespace EnananV2.Definitions.Data;

public static class CharacterCatalog
{
    public static readonly FrozenDictionary<int, UnitDefinition> Units = new[] 
        {
            new UnitDefinition(1, "Leo/need", 0x4455DD, "💫",
                [
                    new CharacterDefinition(1, "Ichika Hoshino", 0x33AAEE, "🎸"),
                    new CharacterDefinition(2, "Saki Tenma", 0xFFDD44, "🎹"),
                    new CharacterDefinition(3, "Honami Mochizuki", 0xEE6666, "🥁"),
                    new CharacterDefinition(4, "Shiho Hinomori", 0xBBDD22, "🍜")
                ]),

            new UnitDefinition(2, "MORE MORE JUMP!", 0x88DD44, "🍀",
                [
                    new CharacterDefinition(5, "Minori Hanasato", 0xFFCCAA, "🌸"),
                    new CharacterDefinition(6, "Haruka Kiritani", 0x99CCFF, "🐧"),
                    new CharacterDefinition(7, "Airi Momoi", 0xFFAACC, "🍑"),
                    new CharacterDefinition(8, "Shizuku Hinomori", 0x99EEDD, "💧")
                ]),

            new UnitDefinition(3, "Vivid BAD SQUAD", 0xEE1166, "🎤",
                [
                    new CharacterDefinition(9, "Kohane Azusawa", 0xFF6699, "🐹"),
                    new CharacterDefinition(10, "An Shiraishi", 0x00BBDD, "🎧"),
                    new CharacterDefinition(11, "Akito Shinonome", 0xFF7722, "🥞"),
                    new CharacterDefinition(12, "Toya Aoyagi", 0x0077DD, "☕")
                ]),

            new UnitDefinition(4, "Wonderlands x Showtime", 0xFF9900, "🎪",
                [
                    new CharacterDefinition(13, "Tsukasa Tenma", 0xFFBB00, "🌟"),
                    new CharacterDefinition(14, "Emu Otori", 0xFF66BB, "🍬"),
                    new CharacterDefinition(15, "Nene Kusanagi", 0x33DD99, "🎮"),
                    new CharacterDefinition(16, "Rui Kamishiro", 0xBB88EE, "🎈")
                ]),

            new UnitDefinition(5, "Nightcord at 25:00", 0x884499, "💻",
                [
                    new CharacterDefinition(17, "Kanade Yoisaki", 0xBB6688, "🎼"),
                    new CharacterDefinition(18, "Mafuyu Asahina", 0x8888CC, "❄️"),
                    new CharacterDefinition(19, "Ena Shinonome", 0xCCAA88, "🎨"),
                    new CharacterDefinition(20, "Mizuki Akiyama", 0xDDAACC, "🎀")
                ]),

            new UnitDefinition(6, "Virtual Singers", 0x00CCBB, "🎶",
                [
                    new CharacterDefinition(21, "Hatsune Miku", 0x33CCBB, "🎵"),
                    new CharacterDefinition(22, "Kagamine Rin", 0xFFCC11, "🍊"),
                    new CharacterDefinition(23, "Kagamine Len", 0xFFEE11, "🍌"),
                    new CharacterDefinition(24, "Megurine Luka", 0xFFBBCC, "🐙"),
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