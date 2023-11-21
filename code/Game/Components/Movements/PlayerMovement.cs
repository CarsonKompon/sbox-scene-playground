using System.Runtime.CompilerServices;
using Sandbox;

namespace Home;

[Title( "Player Movement" )]
[Category( "Home - Movements" )]
[Icon( "directions_run", "red", "white" )]
public class PlayerMovement : Movement
{

	// Properties
	[Property] public Vector3 Gravity { get; set; } = new Vector3( 0, 0, 800 );

	// Movement Properties
	[Property] public float GroundControl { get; set; } = 4.0f;
	[Property] public float AirControl { get; set; } = 0.1f;
	[Property] public float MaxForce { get; set; } = 50f;
	[Property] public float Speed { get; set; } = 160f;
	[Property] public float RunSpeed { get; set; } = 290f;
	[Property] public float WalkSpeed { get; set; } = 90f;
	[Property] public float JumpForce { get; set; } = 400f;

	public bool IsCrouching { get; private set; } = false;
	TimeUntil crouchTimer = 0f;

	public override void Update()
	{
		if ( IsProxy ) return;

		// Jumping
		if ( characterController.IsOnGround && Input.Pressed( "Jump" ) )
		{
			characterController.Punch( Vector3.Up * JumpForce );
			Player.OnJump();
		}
	}

	public override void FixedUpdate()
	{
		if ( characterController is null ) return;

		if ( !IsProxy )
		{
			BuildWishVelocity();

			// Crouching
			if ( IsCrouching )
			{
				var duckTrace = Physics.Trace.Ray( Transform.Position, Transform.Position + Vector3.Up * (72f * Player.Height) )
					.WithoutTags( "trigger" )
					.Run();
				if ( duckTrace.Hit ) crouchTimer = 0.25f;
				if ( !Input.Down( "Crouch" ) && crouchTimer )
				{
					characterController.Height = 72f * Player.Height;
					IsCrouching = false;
				}
			}
			else if ( Input.Down( "Crouch" ) )
			{
				characterController.Height = 72f * Player.Height * 0.5f;
				crouchTimer = 0f;
				IsCrouching = true;
			}
		}

		// Apply Friction/Acceleration
		if ( characterController.IsOnGround )
		{
			characterController.Velocity = characterController.Velocity.WithZ( 0 );
			characterController.Accelerate( WishVelocity );
			characterController.ApplyFriction( GroundControl );
		}
		else
		{
			characterController.Velocity -= Gravity * Time.Delta * 0.5f;
			characterController.Accelerate( WishVelocity.ClampLength( MaxForce ) );
			characterController.ApplyFriction( AirControl );
		}

		// Move the character controller
		characterController.Move();

		// Apply the second half of gravity
		if ( !characterController.IsOnGround )
		{
			characterController.Velocity -= Gravity * Time.Delta * 0.5f;
		}
		else
		{
			characterController.Velocity = characterController.Velocity.WithZ( 0 );
		}
	}

	void BuildWishVelocity()
	{
		WishVelocity = 0;

		var rot = Player.EyeAngles.ToRotation();
		if ( Input.Down( "Forward" ) ) WishVelocity += rot.Forward;
		if ( Input.Down( "Backward" ) ) WishVelocity += rot.Backward;
		if ( Input.Down( "Left" ) ) WishVelocity += rot.Left;
		if ( Input.Down( "Right" ) ) WishVelocity += rot.Right;

		WishVelocity = WishVelocity.WithZ( 0 );

		if ( !WishVelocity.IsNearZeroLength ) WishVelocity = WishVelocity.Normal;

		if ( Input.Down( "Run" ) ) WishVelocity *= RunSpeed;
		else if ( Input.Down( "Walk" ) ) WishVelocity *= WalkSpeed;
		else WishVelocity *= Speed;
	}

	public override void Write( ref ByteStream stream )
	{
		stream.Write( WishVelocity );
		stream.Write( IsCrouching );
	}

	public override void Read( ByteStream stream )
	{
		WishVelocity = stream.Read<Vector3>();
		IsCrouching = stream.Read<bool>();
	}
}