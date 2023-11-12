using Sandbox;

public sealed class Bullet : BaseComponent, BaseComponent.ITriggerListener
{
	[Property] public GameObject DestroyParticle { get; set; }

	public Vector3 Velocity { get; set; }

	public override void Update()
	{
		var tr = Physics.Trace.Ray( Transform.Position, Transform.Position + Velocity * Time.Delta )
			.WithoutTags( "trigger" )
			.Run();

		Transform.Position += Velocity * Time.Delta;

		if ( tr.Hit )
		{
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

	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		if ( other.GameObject.Tags.Has( "solid" ) )
		{
			GameObject.Destroy();
		}
	}

	void ITriggerListener.OnTriggerExit( Collider other )
	{

	}
}
