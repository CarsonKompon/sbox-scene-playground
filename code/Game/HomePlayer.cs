using System.Runtime.InteropServices;
using Sandbox;

namespace Home;

public sealed class HomePlayer : BaseComponent
{
	// References
	[Property] public GameObject Body { get; set; }
	[Property] public GameObject Head { get; set; }
	[Property] CameraComponent LocalCamera { get; set; }
	[Property] public CameraComponent FaceCamera { get; set; }
	[Property] public CitizenAnimation AnimationHelper { get; set; }

	public Angles EyeAngles = new Angles( 0, 0, 0 );

	public GameObject Grabbing = null;
	public bool CanGrab = false;
	public bool CanInteract = true;

	public bool IsFirstPerson => CameraZoom == 0f;
	public float Height => AnimationHelper?.Height ?? 1.0f;

	public Movement MovementController => GetComponent<Movement>( true, true );
	public bool IsCrouching
	{
		get
		{
			if ( MovementController is PlayerMovement playerMovement )
			{
				return playerMovement.IsCrouching;
			}
			return false;
		}
	}

	private float CameraZoom = 0f;

	CharacterController characterController;

	GameObject interactPrompt;

	public override void Update()
	{
		// Eye input
		EyeAngles.pitch += Input.MouseDelta.y * 0.1f;
		EyeAngles.yaw -= Input.MouseDelta.x * 0.1f;
		EyeAngles.roll = 0;
		EyeAngles.pitch = Math.Clamp( EyeAngles.pitch, -89.9f, 89.9f );

		// Zoom input
		CameraZoom = Math.Clamp( CameraZoom - Input.MouseWheel * 32f, 0f, 256f );

		// Update camera position
		UpdateCamera();

		// See if we can grab/interact with something
		CheckForInteracts();

		characterController ??= GameObject.GetComponent<CharacterController>();
		if ( characterController is null ) return;

		float rotateDifference = 0;

		// Rotate body to look angles
		if ( Body is not null )
		{
			var targetAngle = new Angles( 0, EyeAngles.yaw, 0 ).ToRotation();
			var vel = characterController.Velocity.WithZ( 0 );

			rotateDifference = Body.Transform.Rotation.Distance( targetAngle );

			if ( rotateDifference > 50f || characterController.Velocity.Length > 10f )
			{
				Body.Transform.Rotation = Rotation.Lerp( Body.Transform.Rotation, targetAngle, Time.Delta * 2f );
			}
		}

		// Animation
		if ( AnimationHelper is not null )
		{
			AnimationHelper.WithWishVelocity( MovementController?.WishVelocity ?? 0 );
			AnimationHelper.WithVelocity( characterController.Velocity );
			// TODO: Add VoiceLevel once multiplayer is pog
			AnimationHelper.AimAngle = EyeAngles.ToRotation();
			AnimationHelper.IsGrounded = characterController.IsOnGround;
			AnimationHelper.FootShuffle = rotateDifference;
			AnimationHelper.WithLook( EyeAngles.Forward, 1, 0.75f, 0.5f );
			AnimationHelper.MoveStyle = Input.Down( "Walk" ) ? CitizenAnimation.MoveStyles.Walk : CitizenAnimation.MoveStyles.Auto;
			AnimationHelper.DuckLevel = IsCrouching ? 1f : 0f;
			MovementController?.UpdateAnimations( AnimationHelper );
		}

		// Hide playermodel if in first person
		var modelRenderer = Body.GetComponent<AnimatedModelComponent>( false );
		if ( modelRenderer is not null )
			modelRenderer.Enabled = !IsFirstPerson;
	}

	void CheckForInteracts()
	{
		var interactTrace = Physics.Trace.Ray( Head.Transform.Position, Head.Transform.Position + EyeAngles.Forward * 100f )
			.WithTag( "interact" )
			.Run();
		CanGrab = false;
		if ( interactTrace.Hit && interactTrace.Body.GameObject is GameObject interactObject )
		{
			var grabbable = interactObject.GetComponent<Grabbable>();
			if ( grabbable is not null )
			{
				CanGrab = true;
				if ( Input.Pressed( "Action1" ) )
				{
					grabbable.StartGrabbing( this );
				}
			}

			var interactable = interactObject.GetComponent<Interactable>();
			if ( interactable is not null && !CanGrab && CanInteract )
			{
				if ( interactPrompt is null )
				{
					interactPrompt = interactable.CreateUIPrompt();
				}

				if ( Input.Pressed( "Interact" ) )
				{
					interactable.Interact( this );
				}
			}
			else if ( interactPrompt is not null )
			{
				interactPrompt.Destroy();
				interactPrompt = null;
			}

		}
		else if ( interactPrompt is not null )
		{
			interactPrompt.Destroy();
			interactPrompt = null;
		}

		if ( Grabbing is not null && (!Input.Down( "Action1" ) || Grabbing.Transform.Position.Distance( Head.Transform.Position ) > 250f) )
		{
			Grabbing.GetComponent<Grabbable>()?.StopGrabbing( this );
		}
	}

	void UpdateCamera()
	{
		// Lerp head position
		var localHeadPos = Head.Transform.LocalPosition;
		Head.Transform.LocalPosition = localHeadPos.WithZ( MathX.Lerp( localHeadPos.z, 64 * (IsCrouching ? 0.5f : 1f), 10f * Time.Delta ) );

		// Update camera position
		if ( LocalCamera is not null )
		{
			var camPos = Head.Transform.Position;
			if ( !IsFirstPerson )
			{
				var camForward = EyeAngles.ToRotation().Forward;
				var camTrace = Physics.Trace.Ray( camPos, camPos - (camForward * CameraZoom) )
					.WithoutTags( "trigger" )
					.Run();
				if ( camTrace.Hit ) camPos = camTrace.HitPosition + camForward * 1f;
				else camPos = camTrace.EndPosition;
			}

			Rotation rotation = EyeAngles.ToRotation();

			LocalCamera.Transform.Position = camPos;
			LocalCamera.Transform.Rotation = rotation;

			Head.Transform.Rotation = rotation;
		}
	}

	public void OnJump()
	{
		AnimationHelper?.TriggerJump();
	}
}
