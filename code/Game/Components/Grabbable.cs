using Sandbox;

namespace Home;

public sealed class Grabbable : BaseComponent
{
	public HomePlayer Holder = null;
	public bool IsGrabbed => Holder is not null;

	PhysicsComponent rigidBody;

	public override void OnAwake()
	{
		rigidBody = GetComponent<PhysicsComponent>( false, true );
	}

	public override void Update()
	{
		if ( IsGrabbed )
		{
			// Move towards a position in front of the holder's head
			var targetPos = Holder.Head.Transform.Position + Holder.Head.Transform.Rotation.Forward * 100f;
			var targetRot = Holder.Head.Transform.Rotation;

			var delta = targetPos - Transform.Position;
			var distance = delta.Length;

			if ( distance > 10f )
			{
				var direction = delta.Normal;
				var speed = distance * 5f;

				if ( speed > 800f ) speed = 800f;

				var wishVelocity = direction * speed;

				if ( rigidBody is not null )
				{
					rigidBody.Velocity = wishVelocity;
				}
			}
			else
			{
				if ( rigidBody is not null )
				{
					rigidBody.Velocity = Vector3.Zero;
				}
			}

			Transform.Position = Transform.Position.LerpTo( targetPos, Time.Delta * 10f );
		}
	}

	public void StartGrabbing( HomePlayer player )
	{
		if ( player.Grabbing is not null ) return;

		Holder = player;
		player.Grabbing = GameObject;
		GameObject.Tags.Add( "player" );

		if ( rigidBody is not null )
		{
			rigidBody.Gravity = false;
		}
	}

	public void StopGrabbing( HomePlayer player )
	{
		if ( player.Grabbing != GameObject ) return;

		Log.Info( "let go!" );

		Holder = null;
		player.Grabbing = null;
		GameObject.Tags.Remove( "player" );

		if ( rigidBody is not null )
		{
			rigidBody.Gravity = true;
		}
	}
}
