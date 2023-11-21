using System.IO.Compression;
using Sandbox;

namespace Home;

[Title( "Grabbable" )]
[Category( "Home" )]
[Icon( "pan_tool", "red", "white" )]
public sealed class Grabbable : BaseComponent, INetworkBaby
{
	public Guid Holder = Guid.Empty;
	public bool IsGrabbed => Holder != Guid.Empty;

	PhysicsComponent rigidBody;

	public override void Update()
	{
		if ( IsProxy ) return;

		var player = HomePlayer.Local;
		if ( IsGrabbed && player is not null && Holder == player.GameObject.Id )
		{
			if ( !Input.Down( "Action1" ) || Transform.Position.Distance( player.Head.Transform.Position ) > 250f )
			{
				StopGrabbing( player );
			}
		}
	}

	public override void FixedUpdate()
	{
		if ( IsProxy ) return;

		GameObject playerObj = Scene.GetAllObjects( true ).Where( x => x.Id == Holder ).FirstOrDefault();

		if ( IsGrabbed && playerObj is not null )
		{
			var player = playerObj.GetComponent<HomePlayer>();
			if ( player is null ) return;

			// Move towards a position in front of the holder's head
			var targetPos = player.Head.Transform.Position + player.Head.Transform.Rotation.Forward * 100f;
			var targetRot = player.Head.Transform.Rotation;

			var delta = targetPos - Transform.Position;
			var distance = delta.Length;

			rigidBody ??= GetComponent<PhysicsComponent>( false, true );

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
		}

		if ( rigidBody is not null )
		{
			rigidBody.Gravity = Holder == Guid.Empty;
		}
	}

	[Broadcast]
	public void SetGrabbing( Guid grabber )
	{
		bool grabbing = (grabber != Guid.Empty);
		if ( grabbing )
		{
			GameObject.Tags.Add( "player" );
		}
		else
		{
			GameObject.Tags.Remove( "player" );
		}

		Holder = grabber;
	}

	public void StartGrabbing( HomePlayer player )
	{
		if ( player.Grabbing != Guid.Empty ) return;
		if ( IsGrabbed ) return;

		SetGrabbing( player.GameObject.Id );
		player.Grabbing = GameObject.Id;
	}

	public void StopGrabbing( HomePlayer player )
	{
		if ( !IsGrabbed ) return;

		if ( player.GameObject.Id == Holder || player.Grabbing == GameObject.Id )
		{
			SetGrabbing( Guid.Empty );
			player.Grabbing = Guid.Empty;
		}
	}

	public void Write( ref ByteStream stream )
	{
		stream.Write( Holder );
	}

	public void Read( ByteStream stream )
	{
		Holder = stream.Read<Guid>();
	}
}
