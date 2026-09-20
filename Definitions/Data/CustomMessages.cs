using EnananV2.Definitions.Models;

namespace EnananV2.Definitions.Data;

public static class CustomMessages
{
    private static readonly string[] Welcome =
    [
        "Oh— hey. Didn't expect company, <@{0}>.",
        "Huh. New face, huh? Welcome, <@{0}>.",
        "Alright, alright… welcome in, <@{0}>.",
        "So you're the new one everyone was talking about, <@{0}>?",
        "Welcome, <@{0}>. Try not to break anything, okay?",
        "Oh wow, someone actually joined. Hey there, <@{0}>.",
        "Guess I should say welcome, huh? Hi, <@{0}>.",
        "New arrival detected… yeah, yeah. Welcome, <@{0}>.",
        "Hey. Yeah, you. <@{0}>. Welcome.",
        "Alright, make yourself comfortable, <@{0}>. Or don't. Up to you."
    ];

    private static readonly string[] ColorList =
    [
        "**{0}**, you want me to list… EVERY color? Yeah, okay. That's not happening. Just check this out instead: <{1}>",
        "Hey, wait a second **{0}**! I'm not crazy enough to list all 148 colors myself. I already have something for that. Here: <{1}>",
        "**{0}**… I am not typing out a color encyclopedia by hand. I have standards. Anyway, here: <{1}>"
    ];

    private static readonly string[] Donation =
    [
        "If you like my work, you can support me here! {0}",
        "Art isn't free, freelancers need financial support! You can donate here: {0}",
        "Anyway, if you wanna help keep things running, here's the link: {0}"
    ];

    private static readonly string[] Invite =
    [
        "Oh… you want to invite me somewhere else? I guess that's kinda nice. Here:\n**[Invite Enanan to your server!](<{0}>)**",
        "Another server, huh? Alright… I'll go.\n**[Invite Enanan to your server!](<{0}>)**",
        "Well… if you think I'll be useful, then fine. Click here to invite me:\n**[Invite Enanan to your server!](<{0}>)**",
        "I mean, I *am* pretty helpful. You can invite me with this:\n**[Invite Enanan to your server!](<{0}>)**",
        "Guess I can handle one more place. Just don't expect miracles.\n**[Invite Enanan to your server!](<{0}>)**",
        "Thanks for wanting to bring me along. Here's the invite link:\n**[Invite Enanan to your server!](<{0}>)**"
    ];

    public static string GetRandomMessage(CustomMessageType type)
    {
        return type switch
        {
            CustomMessageType.Welcome => Welcome[Random.Shared.Next(Welcome.Length)],
            CustomMessageType.ColorList => ColorList[Random.Shared.Next(ColorList.Length)],
            CustomMessageType.Donation => Donation[Random.Shared.Next(Donation.Length)],
            CustomMessageType.Invite => Invite[Random.Shared.Next(Invite.Length)],
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}