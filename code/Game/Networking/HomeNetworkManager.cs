using Sandbox;
using Sandbox.Network;

namespace Home.Networking;

public sealed class HomeNetworkManager : BaseComponent
{
    [Property] public GameObject PlayerPrefab { get; set; }
    [Property] public GameObject SpawnPoint { get; set; }

    public override void OnStart()
    {
        // Connect to a lobby
        if ( SceneNetworkSystem.Instance is null )
        {
            GameNetworkSystem.CreateLobby();
        }

        // Spawn the player object
        var myPlayerObject = SceneUtility.Instantiate( PlayerPrefab, SpawnPoint.Transform.World );
        myPlayerObject.Enabled = true;

        var homePlayer = myPlayerObject.GetComponent<HomePlayer>( false, true );
        if ( homePlayer is not null )
        {
            homePlayer.Initialize( Game.SteamId );
        }

        NetworkObject.Instantiate( myPlayerObject );
    }

    public override void Update()
    {

    }

}