using Sandbox;
using System.Threading.Tasks;

namespace Home;

[Title( "Video Player" )]
[Category( "Light" )]
[Icon( "videocam", "red", "white" )]
public class VideoPlayerComponent : BaseComponent
{
    [Property]
    public DynamicTextureComponent TextureTarget { get; set; }
    [Property, ResourceType( "mp4" )]
    public string VideoPath { get; set; }
    [Property] public bool PlayOnStart { get; set; } = true;
    [Property]
    public bool Loop
    {
        get => _loop;
        set
        {
            _loop = value;
            if ( VideoPlayer is not null )
            {
                VideoPlayer.Repeat = value;
            }
        }
    }
    private bool _loop = false;
    protected bool IsInitializing { get; set; } = true;
    public virtual bool VideoLoaded { get; protected set; }
    public virtual bool AudioLoaded { get; protected set; }
    protected VideoPlayer VideoPlayer;
    protected SoundHandle SoundHandle;
    protected TimeSince VideoLastUpdated { get; set; }

    // OnVideoStart delegate
    public delegate void OnVideoStartDelegate( VideoPlayer videoPlayer, SoundHandle soundHandle );
    public OnVideoStartDelegate OnVideoStart { get; set; }

    public override void OnStart()
    {
        base.OnStart();

        if ( PlayOnStart )
            Play( VideoPath );
    }

    protected virtual void OnTextureData( ReadOnlySpan<byte> span, Vector2 size )
    {
        TextureTarget.UpdateTextureData( span, size );
        VideoLoaded = true;
        VideoLastUpdated = 0;
    }

    protected virtual async Task WaitUntilReady()
    {
        if ( !IsInitializing )
            return;

        while ( !(VideoLoaded && AudioLoaded) )
        {
            await GameTask.DelaySeconds( Time.Delta );
        }

        OnVideoStart?.Invoke( VideoPlayer, SoundHandle );
        IsInitializing = false;
    }

    public override void Update()
    {
        base.Update();

        VideoPlayer?.Present();
    }

    public virtual void Stop()
    {
        if ( VideoPlayer == null )
            return;

        // CurrentlyPlayingAudio?.Stop( true );
        AudioLoaded = false;
        VideoPlayer.Stop();
        VideoPlayer.Dispose();
        VideoLoaded = false;
        VideoPlayer = null;
    }

    public virtual void Play( string filePath )
    {
        IsInitializing = true;

        VideoPlayer = new VideoPlayer();
        VideoPlayer.OnAudioReady += () =>
        {
            SoundHandle = VideoPlayer.PlayAudio();
            AudioLoaded = true;
        };
        VideoPlayer.Repeat = Loop;
        VideoPlayer.OnTextureData += OnTextureData;
        VideoPlayer.Play( FileSystem.Mounted, filePath );

        WaitUntilReady();
    }
}