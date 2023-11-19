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
    }
}
