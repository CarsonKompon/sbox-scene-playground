using Sandbox;

public sealed class QuestRoom : BaseComponent, BaseComponent.ITriggerListener
{
	[Property] public string Name { get; set; } = "Quest Room";

	// TODO: Probably keep track of who is here on the server-side?

	void ITriggerListener.OnTriggerEnter( Collider other )
	{

	}

	void ITriggerListener.OnTriggerExit( Collider other )
	{

	}

}

/*

{
	[Property] public Vector3 Size { get; set; } = new Vector3( 100, 20, 100 );
	[Property] public Vector3 Offset { get; set; } = new Vector3( 0, 0, 0 );

	public override void DrawGizmos()
	{
		base.DrawGizmos();

		var bbox = new BBox( Offset - Size / 2, Offset + Size / 2 );
		Gizmo.Draw.Color = Color.Red;
		Gizmo.Draw.LineBBox( bbox );
	}
}
*/