namespace Home.Data
{
    public class PlayerPlaytime
    {
        public long StartedPlaying { get; set; }
        public long LastSeen { get; set; }
        public long MinutesPlayed { get; set; }

        public PlayerPlaytime()
        {
            StartedPlaying = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            LastSeen = StartedPlaying;
            MinutesPlayed = 0;
        }
    }
}
