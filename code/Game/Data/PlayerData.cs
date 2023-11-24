using Sandbox;

namespace Home.Data
{
    public class PlayerData
    {
        public long SteamId { get; set; }
        public string? Name { get; set; }
        public long Money { get; set; }
        public PlayerPlaytime? Playtime { get; set; }
        public InventoryData? Inventory { get; set; }

        public PlayerData()
        {
            SteamId = -1;
            Name = null;
            Money = 0;
            Playtime = new PlayerPlaytime();
            Inventory = null;
        }

        public PlayerData( long steamId ) : this()
        {
            SteamId = steamId;
            Name = new Friend( steamId ).Name;
        }

        public void Write( ref ByteStream stream )
        {
            stream.Write( SteamId );
            stream.Write( Money );

            stream.Write( Playtime.StartedPlaying );
            stream.Write( Playtime.LastSeen );
            stream.Write( Playtime.MinutesPlayed );
        }

        public void Read( ByteStream stream )
        {
            SteamId = stream.Read<long>();
            Money = stream.Read<long>();

            if ( Playtime is null ) Playtime = new PlayerPlaytime();
            Playtime.StartedPlaying = stream.Read<long>();
            Playtime.LastSeen = stream.Read<long>();
            Playtime.MinutesPlayed = stream.Read<long>();
        }
    }
}
