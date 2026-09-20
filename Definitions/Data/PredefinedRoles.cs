using NetCord.Rest;

namespace EnananV2.Definitions.Data;

public static class PredefinedRoles
{
    public static class Tiering
    {
        private static RoleProperties Scheduled => new RoleProperties().WithName("Scheduled Filler").WithMentionable();
        private static RoleProperties Emergency => new RoleProperties().WithName("Emergency Filler").WithMentionable();
        private static RoleProperties Coop => new RoleProperties().WithName("Casual Co-Op").WithMentionable();
        private static RoleProperties Natburns => new RoleProperties().WithName("Natburns").WithMentionable();

        public static RoleProperties[] All => [Scheduled, Emergency, Coop, Natburns];
    }

    public static class Identity
    {
        private static RoleProperties SheHer => new RoleProperties().WithName("She/Her").WithMentionable(false);
        private static RoleProperties HeHim => new RoleProperties().WithName("He/Him").WithMentionable(false);
        private static RoleProperties TheyThem => new RoleProperties().WithName("They/Them").WithMentionable(false);
        private static RoleProperties Neo => new RoleProperties().WithName("Neopronouns").WithMentionable(false);
        private static RoleProperties Other => new RoleProperties().WithName("Other").WithMentionable(false);
        private static RoleProperties None => new RoleProperties().WithName("None").WithMentionable(false);

        public static RoleProperties[] All => [SheHer, HeHim, TheyThem, Neo, Other, None];
    }

    public static class Utility
    {
        private static RoleProperties Announcements => new RoleProperties().WithName("Announcements").WithMentionable();
        private static RoleProperties PullParty => new RoleProperties().WithName("Pull Party").WithMentionable();
        private static RoleProperties Streams => new RoleProperties().WithName("Streams").WithMentionable();
        
        public static RoleProperties[] All => [Announcements, PullParty, Streams];
    }
}