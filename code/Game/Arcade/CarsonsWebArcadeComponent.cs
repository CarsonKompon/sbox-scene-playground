using Home;
using Home.Arcade;
using Sandbox;

[Title( "Carson's Web Arcade Machine" )]
[Category( "Arcade" )]
public sealed class CarsonsWebArcadeComponent : BaseComponent
{
	[Property] public Interactable Interactable { get; set; }
	[Property] public GameObject Seat { get; set; }
	[Property] public GameObject ArcadeUI { get; set; }
	[Property] public HomePlayer User { get; set; }

	GameObject uiInstance = null;

	public override void OnAwake()
	{
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

			Interactable.CanInteract = false;
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

		Interactable.CanInteract = true;

		User.InteractLocks.Remove( "arcade-" + GameObject.Name );
		User.RestoreMovement();

		User = null;
	}
}
