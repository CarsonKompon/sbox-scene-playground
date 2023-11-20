using Home;
using Home.UI.Arcade;
using Sandbox;

namespace Home.Arcade;

[Title( "Carson's Web Arcade Machine" )]
[Category( "Home - Arcade" )]
[Icon( "videogame_asset", "red", "white" )]
public sealed class CarsonsWebArcadeComponent : BaseComponent
{
	[Property] public Interactable Interactable { get; set; }
	[Property] public GameObject Seat { get; set; }
	[Property] public GameObject ArcadeUI { get; set; }
	[Property] public HomePlayer User { get; set; }

	GameObject uiInstance = null;

	public override void OnStart()
	{
		if ( Interactable is not null )
			Interactable.OnInteract += OnInteract;
	}

	void OnInteract( HomePlayer player )
	{
		if ( User is not null ) return;

		if ( player.MovementController is PlayerMovement )
		{
			player.InteractLocks.Add( "arcade-" + GameObject.Name );

			uiInstance = SceneUtility.Instantiate( ArcadeUI );
			var arcadePanel = uiInstance.GetComponent<CarsonsWebArcadePanel>();
			arcadePanel.OnClose += Leave;

			player.SetMovement<StationaryMovement>();
			if ( player.MovementController is StationaryMovement stationaryMovement )
			{
				stationaryMovement.SetTarget( Seat );
				// stationaryMovement.OnCrouch += Leave;
			}

			Interactable.SetCantInteract();
			User = player;
		}
	}

	void Leave()
	{
		// if ( User.MovementController is StationaryMovement stationaryMovement )
		// {
		// 	stationaryMovement.OnCrouch -= Leave;
		// }

		if ( uiInstance is not null )
		{
			uiInstance.Destroy();
			uiInstance = null;
		}

		Interactable.SetCanInteract();

		User.InteractLocks.Remove( "arcade-" + GameObject.Name );
		User.RestoreMovement();

		User = null;
	}

}
