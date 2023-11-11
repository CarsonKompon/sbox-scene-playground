using System;
using Sandbox;
using Sandbox.UI;

public partial class Minimap
{
    public const float MINIMAP_SCALE = 0.04f;

    public Texture Canvas { get; set; }
    public int CanvasWidth = 0;
    public int CanvasHeight = 0;

    void ResetCanvas()
    {
        if ( Canvas is not null )
            Canvas.Dispose();

        // TODO: Get the panel size proper?
        CanvasWidth = 160;
        CanvasHeight = 90;
        Texture2DBuilder canvasBuilder = Texture.Create( CanvasWidth, CanvasHeight );
        Canvas = canvasBuilder.Finish();

        Log.Info( $"Minimap canvas created: {CanvasWidth}x{CanvasHeight}" );
    }

    public override void Tick()
    {
        // Initialize Canvas if not already
        if ( Canvas is null ) ResetCanvas();

        // Clear the Canvas
        Canvas.Update( Color.Transparent, new Rect( 0, 0, CanvasWidth, CanvasHeight ) );

        // Get Camera position
        var camPos = Camera.Main.Position;

        // Draw all rooms
        foreach ( var room in GameManager.ActiveScene.GetAllObjects( false ).Where( x => x.GetComponent<QuestRoom>() != null ) )
        {
            var roomCollider = room.GetComponent<ColliderBoxComponent>();
            var roomPos = roomCollider.Transform.Position;
            Canvas.Update( new Color( 0.6f, 0.6f, 0.6f ), new Rect(
                (int)((roomPos.x - camPos.x - roomCollider.Scale.x) * MINIMAP_SCALE) + (CanvasWidth / 2),
                (int)(-(roomPos.z - camPos.z + roomCollider.Scale.z) * MINIMAP_SCALE) + (CanvasHeight / 2),
                (int)(roomCollider.Scale.x * 2 * MINIMAP_SCALE),
                (int)(roomCollider.Scale.z * 2 * MINIMAP_SCALE)
            ) );
        }

        // Draw all players
        foreach ( var player in GameManager.ActiveScene.GetAllObjects( false ).Where( x => x.Tags.Has( "player" ) ) )
        {
            Canvas.Update( Color.Blue, new Rect(
                (int)((player.Transform.Position.x - camPos.x) * MINIMAP_SCALE) + (CanvasWidth / 2) - 1,
                (int)(-(player.Transform.Position.z - camPos.z) * MINIMAP_SCALE) + (CanvasHeight / 2) - 1,
                3,
                3
            ) );
        }
    }

    protected override int BuildHash()
    {
        return HashCode.Combine( Time.Now );
    }
}