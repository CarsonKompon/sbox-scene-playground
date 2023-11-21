using Sandbox;

[Title( "World Network Object" )]
[Category( "Home - Networking" )]
[Icon( "public", "red", "white" )]
public sealed class WorldNetworkObject : BaseComponent
{
	public override void OnStart()
	{
		// if ( GameObject.GetComponent<NetworkObject>() is null )
		// {
		// 	GameObject.AddComponent<NetworkObject>();
		// }

		// NetworkObject.Instantiate( GameObject );
		// Destroy();

		GameObject.NetworkSpawn();
		Destroy();
	}
}