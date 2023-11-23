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
		if ( player is not null && GameObject.Net.Owner == player.GameObject.Id )
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

		GameObject playerObj = Scene.GetAllObjects( true ).Where( x => x.Id == GameObject.Net.Owner ).FirstOrDefault();

		if ( GameObject.Net.IsOwner && playerObj is not null )
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
			rigidBody.Gravity = GameObject.Net.IsUnowned;
		}
	}

	public void StartGrabbing( HomePlayer player )
	{
		if ( !GameObject.Net.IsUnowned ) return;
		if ( player.Grabbing.IsValid() ) return;

		GameObject.Tags.Add( "player" );

		player.Grabbing = GameObject;
	}

	public void StopGrabbing( HomePlayer player )
	{
		if ( !GameObject.Net.IsOwner ) return;

		if ( player.GameObject.Id == GameObject.Net.Owner || player.Grabbing.Id == GameObject.Id )
		{
			GameObject.Tags.Remove( "player" );
			player.Grabbing = null;
		}
	}
}
