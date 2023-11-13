using Sandbox;

public sealed class Bullet : BaseComponent, BaseComponent.ITriggerListener
{
	[Property] public int Damage { get; set; } = 10;
	[Property] public GameObject DestroyParticle { get; set; }

	public Vector3 Velocity { get; set; }

	public override void Update()
	{
		var tr = Physics.Trace.Ray( Transform.Position, Transform.Position + Velocity * Time.Delta )
			.WithoutTags( "trigger", "player" )
			.Run();

		Transform.Position += Velocity * Time.Delta;

		if ( tr.Hit )
		{
			if ( tr.Body.GameObject is GameObject obj )
			{
				TryAttack( obj );
			}
			Transform.Position = tr.HitPosition;
			GameObject.Destroy();
		}
	}

	public override void OnDestroy()
	{
		if ( DestroyParticle is not null )
		{
			var particle = SceneUtility.Instantiate( DestroyParticle, Transform.Position );
			particle.Transform.Position = Transform.Position;
		}
	}

	bool TryAttack( GameObject obj )
	{
		if ( !obj.Tags.HasAny( GameObject.Tags.TryGetAll().ToHashSet() ) && obj.GetComponent<HealthComponent>() is HealthComponent health )
		{
			health.Damage( Damage );
			return true;
		}
		return false;
	}

	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		if ( other.GameObject.Tags.Has( "solid" ) )
		{
			GameObject.Destroy();
		}
		else if ( other.GameObject.GetComponent<HealthComponent>() is HealthComponent health )
		{
			if ( TryAttack( other.GameObject ) )
				GameObject.Destroy();
		}
	}

	void ITriggerListener.OnTriggerExit( Collider other )
	{

	}
}
