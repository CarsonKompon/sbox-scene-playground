using System.IO.Compression;
using Sandbox;

namespace Home;

[Title( "Grabbable" )]
[Category( "Home" )]
[Icon( "pan_tool", "red", "white" )]
public sealed class Grabbable : BaseComponent
{
	PhysicsComponent rigidBody;

	public override void Update()
	{
		if ( !GameObject.Net.IsOwner ) return;

		var player = HomePlayer.Local;
		if ( player is not null )
		{
			if ( !Input.Down( "Action1" ) || Transform.Position.Distance( player.Head.Transform.Position ) > 250f )
			{
				StopGrabbing( player );
			}
		}
	}

	public override void FixedUpdate()
	{
		rigidBody ??= GetComponent<PhysicsComponent>( false, true );

		if ( !GameObject.Net.IsOwner ) return;

		var player = HomePlayer.Local;
		if ( player is not null )
		{
			// Move towards a position in front of the holder's head
			var targetPos = player.Head.Transform.Position + player.Head.Transform.Rotation.Forward * 100f;
			var targetRot = player.Head.Transform.Rotation;

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
		}
	}

	public void StartGrabbing( HomePlayer player )
	{
		if ( !GameObject.Net.IsUnowned ) return;
		if ( player.Grabbing.IsValid() ) return;

		GameObject.Network.TakeOwnership();
		GameObject.Tags.Add( "player" );
		rigidBody.Gravity = false;

		player.Grabbing = GameObject;
	}

	public void StopGrabbing( HomePlayer player )
	{
		if ( !GameObject.Net.IsOwner ) return;

		if ( player.GameObject.Id == GameObject.Net.Owner || player.Grabbing.Id == GameObject.Id )
		{
			GameObject.Network.DropOwnership();
			GameObject.Tags.Remove( "player" );
			player.Grabbing = null;
			rigidBody.Gravity = true;
		}
	}
}
