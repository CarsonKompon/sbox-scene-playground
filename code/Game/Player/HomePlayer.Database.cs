using Sandbox;

namespace Home;
using System.Text.Json;
using Home.Data;

partial class HomePlayer
{
    RealTimeSince playtimeCheck = 0;

    async void GetPlayerData()
    {
        if ( Data is null )
        {
            var steamId = Game.SteamId; // TODO: Make sure this is the local steamid
            Data = await HomeDb.FetchPlayerData( steamId );
            if ( Data is null )
            {
                Data = new PlayerData( steamId );
                HomeDb.PostPlayerData( steamId, Data );
            }
        }

        UpdateLastSeen();

        Log.Info( JsonSerializer.Serialize( Data ) );
    }

    void CheckForDbUpdates()
    {
        // Update playtime
        if ( playtimeCheck > 60f )
        {
            UpdatePlaytime();
            playtimeCheck = 0f;
        }
    }

    async void UpdatePlaytime()
    {
        Data.Playtime = await HomeDb.UpdatePlaytime( Data, 1 );
    }

    async void UpdateLastSeen()
    {
        Data.Playtime = await HomeDb.UpdatePlaytime( Data, 0 );
    }
}