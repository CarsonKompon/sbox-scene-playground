using Sandbox;

public sealed class QuestPlayer : BaseComponent
{
	// Properties
	[Property] public Vector3 Gravity { get; set; } = new Vector3( 0, 0, 800 );
	[Property, Range( 1000f, 2000f )] public float CameraDistance { get; set; } = 1400f;

	// References
	[Property] GameObject AimCursor { get; set; }
	[Property] GameObject Body { get; set; }
	[Property] CitizenAnimation AnimationHelper { get; set; }

	public Vector3 WishVelocity { get; private set; }
	public CameraComponent LocalCamera { get; private set; }

	public override void OnAwake()
	{
		base.OnAwake();
		var cameraObj = Scene.GetAllObjects( true ).FirstOrDefault( obj => obj.Tags.Has( "camera" ) );
		if ( cameraObj is not null )
		{
			LocalCamera = cameraObj.GetComponent<CameraComponent>();
		}
		else
		{
			Log.Error( "No camera found in scene!" );
		}
	}

	public override void Update()
	{
		// Lerp camera position
		float camLerp = 1.0f - MathF.Pow( 0.5f, Time.Delta * 10.0f );
		var camPos = LocalCamera.Transform.Position.LerpTo( Transform.Position.WithY( Transform.Position.y - CameraDistance ), camLerp );
		LocalCamera.Transform.Position = camPos;

		// Set cursor position
		var tr = Physics.Trace.Ray( new Ray( LocalCamera.Transform.Position, Screen.GetDirection( Mouse.Position ) ), 1000 )
			.WithTag( "solid" )
			.Run();

		AimCursor.Transform.Position = new Vector3( tr.EndPosition.x, AimCursor.Transform.Position.y, tr.EndPosition.z );
	}
}
