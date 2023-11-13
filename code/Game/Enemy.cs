using System.Collections.Generic;
using System.Linq;
using Sandbox;

public sealed class Enemy : BaseComponent, BaseComponent.ITriggerListener
{
	[Property] public float AggroTime { get; set; } = 5f;
	[Property] public float MovementSpeed { get; set; } = 80f;
	[Property] public float MovementLerp { get; set; } = 5f;

	[Property] GameObject Body { get; set; }
	[Property] CharacterController characterController { get; set; }
	[Property] CitizenAnimation AnimationHelper { get; set; }

	public bool IsAggro
	{
		get
		{
			return nearbyTargets.Count > 0 || aggroTimer > 0;
		}
	}

	List<GameObject> nearbyTargets = new();
	TimeUntil aggroTimer = 0;
	Vector3 wishVelocity = Vector3.Zero;

	public override void Update()
	{
		// Rotate towards movement direction
		if ( wishVelocity.Length > 0.1f )
		{
			var targetRot = Rotation.LookAt( wishVelocity, Vector3.Up );
			Body.Transform.LocalRotation = Rotation.Slerp( Body.Transform.LocalRotation, targetRot, 1f - MathF.Pow( 0.5f, 8f * Time.Delta ) );
		}

		// Set Animation Parameters
		if ( AnimationHelper is not null )
		{
			AnimationHelper.WithVelocity( characterController.Velocity );
			AnimationHelper.IsGrounded = characterController.IsOnGround;
		}
	}

	public override void FixedUpdate()
	{
		if ( characterController is null ) return;

		// Get Wish Velocity (Follow nearest player)
		bool moving = false;
		if ( IsAggro )
		{
			GameObject nearestTarget = nearbyTargets.OrderBy( obj => (obj.Transform.Position - Transform.Position).Length ).FirstOrDefault();
			if ( nearestTarget is not null )
			{
				Vector3 targetVelocity = (nearestTarget.Transform.Position - Transform.Position).WithZ( 0 ).Normal * MovementSpeed;
				wishVelocity = wishVelocity.LerpTo( targetVelocity, 1f - MathF.Pow( 0.5f, MovementLerp * Time.Delta ) );
				moving = true;
			}
		}
		if ( !moving )
		{
			wishVelocity = wishVelocity.LerpTo( Vector3.Zero, 1f - MathF.Pow( 0.5f, MovementLerp * Time.Delta ) );
		}

		// Apply friction/acceleration
		if ( characterController.IsOnGround )
		{
			characterController.Velocity = characterController.Velocity.WithZ( 0 );
			characterController.Accelerate( wishVelocity );
			characterController.ApplyFriction( 1f );
		}
		else
		{
			characterController.Velocity += Vector3.Down * 800f * Time.Delta * 0.5f;
			characterController.Accelerate( wishVelocity.ClampLength( 800f ) );
			characterController.ApplyFriction( 0.5f );
		}

		// Move the character controller
		characterController.Move();

		// Apply the second half of gravity
		if ( !characterController.IsOnGround )
		{
			characterController.Velocity += Vector3.Down * 800f * Time.Delta * 0.5f;
		}
		else
		{
			characterController.Velocity = characterController.Velocity.WithZ( 0f );
		}
	}

	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		if ( other.GameObject.Tags.Has( "player" ) && other.GameObject.GetComponent<HealthComponent>() != null )
		{
			Log.Info( other.GameObject.Name );
			nearbyTargets.Add( other.GameObject );
		}
	}

	void ITriggerListener.OnTriggerExit( Collider other )
	{
		if ( nearbyTargets.Contains( other.GameObject ) )
		{
			nearbyTargets.Remove( other.GameObject );

			if ( nearbyTargets.Count == 0 )
				aggroTimer = AggroTime;
		}
	}

}
