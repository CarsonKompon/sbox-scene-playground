using System.Runtime.InteropServices;
using Sandbox;

public sealed class HomePlayer : BaseComponent
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

	// References
	[Property] GameObject Body { get; set; }
	[Property] GameObject Head { get; set; }
	[Property] CitizenAnimation AnimationHelper { get; set; }

	public Vector3 WishVelocity { get; private set; } = Vector3.Zero;
	public Angles EyeAngles = new Angles( 0, 0, 0 );

	public bool IsFirstPerson => CameraZoom == 0f;
	public bool IsCrouching { get; private set; } = false;
	TimeUntil crouchTimer = 0f;

	private float CameraZoom = 0f;

	CameraComponent camera;
	CharacterController characterController;

	public override void Update()
	{
		// Eye input
		EyeAngles.pitch += Input.MouseDelta.y * 0.1f;
		EyeAngles.yaw -= Input.MouseDelta.x * 0.1f;
		EyeAngles.roll = 0;

		// Zoom input
		CameraZoom = Math.Clamp( CameraZoom - Input.MouseWheel * 32f, 0f, 256f );

		// Update camera position
		camera ??= GameObject.GetComponent<CameraComponent>( true, true );
		if ( camera is not null )
		{
			var camPos = Head.Transform.Position - (EyeAngles.ToRotation().Forward * CameraZoom);

			if ( IsFirstPerson ) camPos = Head.Transform.Position + EyeAngles.ToRotation().Forward * 8;

			camera.Transform.Position = camPos;
			camera.Transform.Rotation = EyeAngles.ToRotation();
		}

		characterController ??= GameObject.GetComponent<CharacterController>();
		if ( characterController is null ) return;

		float rotateDifference = 0;

		// Rotate body to look angles
		if ( Body is not null )
		{
			var targetAngle = new Angles( 0, EyeAngles.yaw, 0 ).ToRotation();
			var vel = characterController.Velocity.WithZ( 0 );

			// if ( vel.Length > Speed )
			// {
			// 	targetAngle = Rotation.LookAt( vel, Vector3.Up );
			// }

			rotateDifference = Body.Transform.Rotation.Distance( targetAngle );

			if ( rotateDifference > 50f || characterController.Velocity.Length > 10f )
			{
				Body.Transform.Rotation = Rotation.Lerp( Body.Transform.Rotation, targetAngle, Time.Delta * 2f );
			}
		}

		// Jumping
		if ( characterController.IsOnGround && Input.Pressed( "Jump" ) )
		{
			characterController.Punch( Vector3.Up * JumpForce );
			AnimationHelper?.TriggerJump();
		}

		if ( AnimationHelper is not null )
		{
			AnimationHelper.WithVelocity( characterController.Velocity );
			AnimationHelper.IsGrounded = characterController.IsOnGround;
			AnimationHelper.FootShuffle = rotateDifference;
			AnimationHelper.WithLook( EyeAngles.Forward, 1, 0.75f, 0.5f );
			AnimationHelper.MoveStyle = CitizenAnimation.MoveStyles.Auto;
			AnimationHelper.DuckLevel = IsCrouching ? 1f : 0f;
		}

		Body.Enabled = !IsFirstPerson;
	}

	public override void FixedUpdate()
	{
		BuildWishVelocity();

		if ( characterController is null ) return;

		// Crouching
		if ( IsCrouching )
		{
			var duckTrace = Physics.Trace.Ray( Transform.Position, Transform.Position + Vector3.Up * (72f * AnimationHelper.Height) )
				.WithoutTags( "trigger" )
				.Run();
			if ( duckTrace.Hit ) crouchTimer = 0.25f;
			if ( !Input.Down( "Crouch" ) && crouchTimer )
			{
				characterController.Height = 72f * AnimationHelper.Height;
				IsCrouching = false;
			}
		}
		else if ( Input.Down( "Crouch" ) )
		{
			characterController.Height = 72f * AnimationHelper.Height * 0.5f;
			crouchTimer = 0f;
			IsCrouching = true;
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

		var rot = EyeAngles.ToRotation();
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
}
