using Sandbox;

namespace Home;
using System.Text.Json;
using Home.Data;

partial class HomePlayer
{
    RealTimeSince playtimeCheck = 0;
    int playtimeMinutesAccumulated = 0;

    public async void Initialize( long steamId )
    {
        if ( !IsController ) return;

        if ( Data is null )
        {
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
        playtimeMinutesAccumulated++;
        if ( playtimeMinutesAccumulated % 5 == 0 )
        {
            HomeDb.GiveMoney( Data, 200 );
        }

        var playtime = await HomeDb.UpdatePlaytime( Data, 1 );
        if ( playtime is not null )
        {
            //Data.Playtime = playtime;
        }
    }

    async void UpdateLastSeen()
    {
        var playtime = await HomeDb.UpdatePlaytime( Data, 0 );
        if ( playtime is not null )
        {
            // Data.Playtime = playtime;
        }
    }
}