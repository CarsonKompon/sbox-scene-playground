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

			localSeat?.Destroy();
			localSeat = SceneUtility.Instantiate( Seat, Seat.Transform.Position );
			localSeat.SetParent( player.GameObject );
		}
		else
		{
			localSeat?.Destroy();
			PlayerMovement playerMovement = player.GetComponent<PlayerMovement>( false, true );
			playerMovement.Enabled = true;
		}
	}
}
