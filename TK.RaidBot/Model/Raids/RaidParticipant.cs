namespace TK.RaidBot.Model.Raids
{
    public class RaidParticipant
    {
        public ulong UserId { get; set; }

        public ParticipationStatus Status { get; set; }

        public RaidRole Role { get; set; }
    }
}
