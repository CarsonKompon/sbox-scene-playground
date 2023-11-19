using Home;
using Sandbox;

[Title( "Tetros Arcade Machine" )]
[Category( "Arcade" )]
public sealed class TetrosArcadeComponent : BaseComponent
{
	[Property] public Interactable Interactable { get; set; }
	[Property] public GameObject Seat { get; set; }

	public override void OnAwake()
	{
		Interactable.OnInteract += OnInteract;
	}

	void OnInteract( HomePlayer player )
	{
		if ( player.MovementController is PlayerMovement )
		{
			player.InteractLocks.Add( "arcade-" + GameObject.Name );

			player.SetMovement<StationaryMovement>();
			if ( player.MovementController is StationaryMovement stationaryMovement )
			{
				stationaryMovement.SetTarget( Seat );
				stationaryMovement.OnCrouch += Leave;
			}
		}
	}

	void Leave( HomePlayer player, bool isCrouching )
	{
		if ( !isCrouching ) return;

		if ( player.MovementController is StationaryMovement stationaryMovement )
		{
			stationaryMovement.OnCrouch -= Leave;

		}

		player.InteractLocks.Remove( "arcade-" + GameObject.Name );
		player.RestoreMovement();
	}
}
