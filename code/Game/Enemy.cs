using System.Collections.Generic;
using System.Linq;
using Sandbox;

public sealed class Enemy : BaseComponent, BaseComponent.ITriggerListener
{
	[Property] public float AggroTime { get; set; } = 5f;
	[Property] public float MovementSpeed { get; set; } = 80f;
	[Property] public float MovementLerp { get; set; } = 5f;

	[Property] CharacterController characterController { get; set; }

	public bool IsAggro
	{
		get
		{
			return nearbyTargets.Count > 0 || aggroTimer > 0;
		}
	}

	List<GameObject> nearbyTargets = new();
	TimeUntil aggroTimer = 0;

	public override void Update()
	{
		if ( characterController is null ) return;

		bool moving = false;
		if ( IsAggro )
		{
			GameObject nearestTarget = nearbyTargets.OrderBy( obj => (obj.Transform.Position - Transform.Position).Length ).FirstOrDefault();
			if ( nearestTarget is not null )
			{
				Vector3 targetVelocity = (nearestTarget.Transform.Position - Transform.Position).WithZ( 0 ).Normal * MovementSpeed;
				characterController.Velocity = characterController.Velocity.LerpTo( targetVelocity, 1f - MathF.Pow( 0.5f, MovementLerp * Time.Delta ) );
				moving = true;
			}
		}
		if ( !moving )
		{
			characterController.Velocity = characterController.Velocity.LerpTo( Vector3.Zero, 1f - MathF.Pow( 0.5f, MovementLerp * Time.Delta ) );
		}
		characterController.Move();
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
