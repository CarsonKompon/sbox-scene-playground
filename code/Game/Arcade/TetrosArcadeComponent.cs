using Home;
using Sandbox;

[Title( "Tetros Arcade Machine" )]
[Category( "Arcade" )]
public sealed class TetrosArcadeComponent : BaseComponent
{
	[Property] public Interactable Interactable { get; set; }
	[Property] public GameObject Seat { get; set; }

	GameObject localSeat;

	public override void OnAwake()
	{
		Interactable.OnInteract += OnInteract;
	}

	void OnInteract( HomePlayer player )
	{
		if ( player.MovementController is PlayerMovement )
		{
			player.MovementController.Enabled = false;
			player.InteractLocks.Add( "arcade-" + GameObject.Name );

			localSeat?.Destroy();
			localSeat = SceneUtility.Instantiate( Seat, Seat.Transform.Position );
			localSeat.SetParent( player.GameObject );

			var movement = localSeat.GetComponent<StationaryMovement>();
			if ( movement is not null )
			{
				movement.OnCrouch += Leave;
			}
		}
	}

	void Leave( bool isCrouching )
	{
		if ( !isCrouching || localSeat is null ) return;

		var player = localSeat.Parent.GetComponent<HomePlayer>();
		player.InteractLocks.Remove( "arcade-" + GameObject.Name );

		localSeat?.Destroy();
		localSeat = null;

		player.RestoreMovement();
	}
}
