using Sandbox;

[Category( "Quest" )]
public sealed class QuestPlayer : BaseComponent
{
	public static QuestPlayer Local { get; private set; }

	// Properties
	[Property] public Vector3 Gravity { get; set; } = new Vector3( 0, 0, 800 );
	[Property, Range( 1000f, 2000f )] public float CameraDistance { get; set; } = 1400f;

	// Movement Properties
	[Property] public float GroundControl { get; set; } = 4.0f;
	[Property] public float AirControl { get; set; } = 0.1f;
	[Property] public float MaxForce { get; set; } = 50f;
	[Property] public float RunSpeed { get; set; } = 320f;
	[Property] public float WalkSpeed { get; set; } = 150f;
	[Property] public float JumpForce { get; set; } = 400f;

	// References
	[Property] public GameObject AimCursor { get; set; }
	[Property] public GameObject Body { get; set; }
	[Property] CitizenAnimation AnimationHelper { get; set; }
	[Property] public HealthComponent Health { get; set; }
	[Property] GameObject EmptyReference { get; set; }

	public int Level = 1;
	public double Xp = 0;

	public Vector3 WishVelocity { get; private set; }
	public CameraComponent LocalCamera { get; private set; }
	public BaseWeapon Weapon { get; private set; }

	CharacterController characterController;

	bool ducking = false;
	TimeUntil duckTimer = 0.0f;

	public override void OnAwake()
	{
		base.OnAwake();
		var cameraObj = Scene.GetAllObjects( true ).FirstOrDefault( obj => obj.Tags.Has( "camera" ) );
		if ( cameraObj is not null )
		{
			LocalCamera = cameraObj.GetComponent<CameraComponent>();
		}
		else
		{
			Log.Error( "No camera found in scene!" );
		}

		// TODO: Make this only pull if it's the local player
		Local = this;
	}

	public override void Update()
	{
		// Lerp camera position
		float camLerp = 1.0f - MathF.Pow( 0.5f, Time.Delta * 10.0f );
		var camPos = Transform.Position.WithZ( Transform.Position.z + 70f ).WithY( Transform.Position.y - CameraDistance );
		camPos = LocalCamera.Transform.Position.LerpTo( camPos, camLerp );
		LocalCamera.Transform.Position = camPos;

		// Set cursor position
		var tr = Physics.Trace.Ray( new Ray( LocalCamera.Transform.Position, Screen.GetDirection( Mouse.Position ) ), 1000 )
			.WithTag( "solid" )
			.Run();
		AimCursor.Transform.Position = new Vector3( tr.EndPosition.x, AimCursor.Transform.Position.y, tr.EndPosition.z );

		if ( characterController is null ) return;

		// Face towards cursor
		float rotLerp = 1.0f - MathF.Pow( 0.5f, Time.Delta * 2.0f );
		if ( AimCursor.Transform.Position.x < Transform.Position.x )
		{
			Body.Transform.LocalRotation = Rotation.Slerp( Body.Transform.LocalRotation, Rotation.FromYaw( 180 ), rotLerp );
		}
		else
		{
			Body.Transform.LocalRotation = Rotation.Slerp( Body.Transform.LocalRotation, Rotation.FromYaw( 0 ), rotLerp );
		}

		// Jumping
		// Press Jump
		if ( characterController.IsOnGround && Input.Pressed( "Jump" ) )
		{
			float flGroundFactor = 1.0f;
			characterController.Punch( Vector3.Up * JumpForce * flGroundFactor );
			AnimationHelper?.TriggerJump();
		}
		else if ( !characterController.IsOnGround && Input.Released( "Jump" ) && characterController.Velocity.z > 0 )
		{
			characterController.Punch( Vector3.Down * characterController.Velocity.z * 0.5f );
		}

		// Set Animation Paramaters
		if ( AnimationHelper is not null )
		{
			AnimationHelper.WithVelocity( characterController.Velocity );
			AnimationHelper.IsGrounded = characterController.IsOnGround;
			AnimationHelper.MoveStyle = Input.Down( "Run" ) ? CitizenAnimation.MoveStyles.Run : CitizenAnimation.MoveStyles.Walk;
			AnimationHelper.HoldType = CitizenAnimation.HoldTypes.Pistol;
			AnimationHelper.Handedness = CitizenAnimation.Hand.Right;
			AnimationHelper.DuckLevel = ducking ? 1.0f : 0.0f;
		}
	}

	public override void FixedUpdate()
	{
		BuildWishVelocity();

		// Make sure player always has a weapon
		if ( Weapon is null ) GiveWeapon( typeof( PistolWeapon ) );

		characterController ??= GameObject.GetComponent<CharacterController>();
		if ( characterController is null ) return;

		// Crouching
		if ( ducking )
		{
			var duckTrace = Physics.Trace.Ray( Transform.Position, Transform.Position + Vector3.Up * 72f )
				.WithoutTags( "trigger" )
				.Run();
			if ( duckTrace.Hit ) duckTimer = 0.25f;
			if ( !Input.Down( "Duck" ) && duckTimer )
			{
				characterController.Height = 64f;
				ducking = false;
			}
		}
		else
		{
			if ( Input.Down( "Duck" ) )
			{
				characterController.Height = 32f;
				duckTimer = 0f;
				ducking = true;
			}
		}

		// Apply friction/acceleration
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

	public void BuildWishVelocity()
	{
		WishVelocity = 0;
		if ( Input.Down( "right" ) )
		{
			WishVelocity += Vector3.Forward;
		}
		if ( Input.Down( "left" ) )
		{
			WishVelocity += Vector3.Backward;
		}

		WishVelocity = WishVelocity.WithZ( 0 );
		if ( !WishVelocity.IsNearZeroLength ) WishVelocity = WishVelocity.Normal;

		if ( ducking ) WishVelocity *= WalkSpeed * 0.6f;
		else if ( Input.Down( "run" ) ) WishVelocity *= RunSpeed;
		else WishVelocity *= WalkSpeed;
	}

	public void GiveWeapon( Type weaponType )
	{
		// TODO: Destroy existing weapon or something

		var weaponObj = SceneUtility.Instantiate( EmptyReference );
		weaponObj.Name = weaponType.Name;
		Weapon = weaponObj.AddComponent( TypeLibrary.GetType<BaseComponent>( weaponType.Name ) ) as BaseWeapon;
		Weapon.Initialize( this );
	}
}
