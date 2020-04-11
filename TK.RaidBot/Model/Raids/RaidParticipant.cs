namespace TK.RaidBot.Model.Raids
{
    public class RaidParticipant
    {
        public ulong UserId { get; set; }

        public ParticipationStatus Status { get; set; }

        public int Role { get; set; }
    }
}
