using Home.Data;
using Sandbox;
using Sandbox.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Home;

public static class HomeDb
{
    public static string API_URL = "https://sboxhome.website/api";

    private static string SandboxToken = "";
    private static TimeSince TimeSinceTokenRefresh = 2f;

    public static PlayerData LocalPlayerData = null;

    public static async Task<PlayerData> FetchLocalPlayerData()
    {
        if ( LocalPlayerData is null )
        {
            LocalPlayerData = await FetchPlayerData( Game.SteamId );
        }
        return LocalPlayerData;
    }

    public static async Task<PlayerData> FetchPlayerData( long steamId )
    {
        try
        {
            var data = await Http.RequestJsonAsync<PlayerData>( $"{API_URL}/players/{steamId}" );
            Log.Info( data );
            return data;
        }
        catch ( Exception e )
        {
            Log.Warning( e.ToString() );
            return null;
        }
    }

    public static async void PostPlayerData( long steamId, PlayerData data )
    {
        Dictionary<string, string> headers = new Dictionary<string, string>();
        //headers.Add( "sboxhash", await GetToken() ); // TODO: Use s&box auth
        headers.Add( "hash", "anything" );
        try
        {
            var response = await Http.RequestAsync( $"{API_URL}/players/{steamId}", "POST", Http.CreateJsonContent( data ), headers );
            Log.Info( response );
        }
        catch ( Exception e )
        {
            Log.Warning( e.ToString() );
        }
    }

    public static async Task<PlayerPlaytime> UpdatePlaytime( PlayerData data, long minutes )
    {
        Dictionary<string, string> headers = new Dictionary<string, string>();
        //headers.Add( "sboxhash", await GetToken() ); // TODO: Use s&box auth
        headers.Add( "hash", "anything" );
        headers.Add( "minutes", minutes.ToString() );
        headers.Add( "name", new Friend( data.SteamId ).Name );
        try
        {
            return await Http.RequestJsonAsync<PlayerPlaytime>( $"{API_URL}/players/{data.SteamId}/playtime", "POST", null, headers );
        }
        catch ( Exception e )
        {
            Log.Warning( e.ToString() );
            return null;
        }
    }

    public static async void GiveMoney( PlayerData data, long amount )
    {
        Dictionary<string, string> headers = new Dictionary<string, string>();
        //headers.Add( "sboxhash", await GetToken() ); // TODO: Use s&box auth
        headers.Add( "hash", "anything" );
        headers.Add( "money", amount.ToString() );
        try
        {
            var response = await Http.RequestAsync( $"{API_URL}/players/{data.SteamId}/money", "POST", null, headers );
            if ( response.IsSuccessStatusCode )
            {
                data.Money += amount;
            }
        }
        catch ( Exception e )
        {
            Log.Warning( e.ToString() );
        }
    }

    public static async Task<string> GetToken()
    {
        if ( TimeSinceTokenRefresh > 1.2f || string.IsNullOrEmpty( SandboxToken ) )
        {
            SandboxToken = await Auth.GetToken( "Home" );
            TimeSinceTokenRefresh = 0f;
        }
        return SandboxToken;
    }

    static async void Example()
    {
        // GET request that returns the response as a string
        string response = await Http.RequestStringAsync( "https://google.com" );

        // POST request of JSON content ignoring any response
        await Http.RequestAsync( "https://api.facepunch.com/my/method", "POST", Http.CreateJsonContent( "" ) );
    }

}