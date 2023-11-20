using Sandbox;

namespace Home;

[Title( "Grabbable" )]
[Category( "Home" )]
[Icon( "pan_tool", "red", "white" )]
public sealed class Grabbable : BaseComponent, INetworkBaby
{
	public HomePlayer Holder = null;
	public bool IsGrabbed = false;

	PhysicsComponent rigidBody;

	public override void OnAwake()
	{
		rigidBody = GetComponent<PhysicsComponent>( false, true );
	}

	public override void Update()
	{
		if ( Holder is null ) return;

		// TODO: Make this check IsMine and have the grab send a network id so we can get the holder
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

	// TODO: Make this take in a network id or something so it can keep track of who is holding
	[Broadcast]
	public void NetSetGrab( int grabId )
	{
		bool grabbing = grabId == 1;
		IsGrabbed = grabbing;

		if ( grabbing )
		{
			GameObject.Tags.Add( "player" );
		}
		else
		{
			GameObject.Tags.Remove( "player" );
			Holder = null;
		}

		if ( rigidBody is not null )
		{
			rigidBody.Gravity = !grabbing;
		}
	}

	public void StartGrabbing( HomePlayer player )
	{
		if ( player.Grabbing is not null ) return;

		NetSetGrab( 1 );
		Holder = player;
		player.Grabbing = GameObject;
	}

	public void StopGrabbing( HomePlayer player )
	{
		if ( player.Grabbing != GameObject ) return;

		NetSetGrab( 0 );
		player.Grabbing = null;
	}

	public void Write( ref ByteStream stream )
	{
		stream.Write( IsGrabbed );
	}

	public void Read( ByteStream stream )
	{
		IsGrabbed = stream.Read<bool>();
	}
}
