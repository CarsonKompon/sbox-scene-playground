using Sandbox;

namespace Home;

[Title( "Base Movement" )]
[Category( "Home - Movements" )]
[Icon( "directions_walk", "red", "white" )]
public class Movement : BaseComponent, INetworkSerializable
{

	// References
	[Property] public HomePlayer Player { get; set; }
	[Property] public CharacterController characterController { get; set; }

	// Variables
	public Vector3 WishVelocity { get; protected set; } = Vector3.Zero;

	// Delegates
	public delegate void OnCrouchDelegate( HomePlayer player, bool isCrouching );
	public OnCrouchDelegate OnCrouch;

	public virtual void UpdateAnimations( CitizenAnimation helper )
	{

	}

	public override void Update()
	{
		if ( IsProxy ) return;

		if ( Input.Pressed( "Crouch" ) ) OnCrouch?.Invoke( Player, true );
		else if ( Input.Released( "Crouch" ) ) OnCrouch?.Invoke( Player, false );
	}

	public virtual void Write( ref ByteStream stream )
	{
		stream.Write( WishVelocity );
	}

	public virtual void Read( ByteStream stream )
	{
		WishVelocity = stream.Read<Vector3>();
	}

}
