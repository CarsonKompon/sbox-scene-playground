using System;
using Sandbox;
using Sandbox.UI;

public partial class Minimap
{
    public const float MINIMAP_SCALE = 0.04f;

    public Texture Canvas { get; set; }
    public int CanvasWidth = 160;
    public int CanvasHeight = 90;

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

        DrawToCanvas( Canvas, CanvasWidth, CanvasHeight, MINIMAP_SCALE );
    }

    public static void DrawToCanvas( Texture canvas, int canvasWidth, int canvasHeight, float scale, Vector2 offset = default )
    {
        // Clear the Canvas
        canvas.Update( Color.Transparent, new Rect( 0, 0, canvasWidth, canvasHeight ) );

        // Get Camera position
        var camPos = Camera.Main.Position + new Vector3( offset.x, 0, -offset.y ) / scale;

        // Draw all rooms
        foreach ( var room in GameManager.ActiveScene.GetAllObjects( false ).Where( x => x.GetComponent<QuestRoom>() != null ) )
        {
            var roomCollider = room.GetComponent<ColliderBoxComponent>();
            var roomPos = roomCollider.Transform.Position;
            canvas.Update( new Color( 0.6f, 0.6f, 0.6f ), new Rect(
                (int)((roomPos.x - camPos.x - roomCollider.Scale.x / 2) * scale) + canvasWidth / 2,
                (int)(-(roomPos.z - camPos.z + roomCollider.Scale.z / 2) * scale) + canvasHeight / 2,
                (int)(roomCollider.Scale.x * scale),
                (int)(roomCollider.Scale.z * scale)
            ) );
        }

        // Draw all players
        foreach ( var player in GameManager.ActiveScene.GetAllObjects( false ).Where( x => x.Tags.Has( "player" ) ) )
        {
            canvas.Update( Color.Blue, new Rect(
                (int)((player.Transform.Position.x - camPos.x) * scale) + (canvasWidth / 2) - 1,
                (int)(-(player.Transform.Position.z - camPos.z) * scale) + (canvasHeight / 2) - 1,
                3,
                3
            ) );
        }
    }

    public void OpenMap()
    {
        QuestHudPanel.Instance.ToggleMap();
    }

    protected override int BuildHash()
    {
        return HashCode.Combine( Time.Now );
    }
}