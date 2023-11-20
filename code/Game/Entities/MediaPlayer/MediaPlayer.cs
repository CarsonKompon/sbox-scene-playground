using Home;
using Home.UI.Arcade;
using Sandbox;
using MediaHelpers;

namespace Home.Entities;

[Title( "Home Media Player" )]
[Category( "Home - Entities" )]
[Icon( "ondemand_video", "red", "white" )]
public sealed class MediaPlayer : BaseComponent, INetworkBaby
{
    // Properties
    [Property] public Interactable Interactable { get; set; }
    [Property] public VideoPlayerComponent VideoPlayer { get; set; }
    [Property] public GameObject MediaPlayerUI { get; set; }

    // Variables
    public List<MediaVideo> Queue { get; set; } = new List<MediaVideo>();
    public MediaVideo CurrentlyPlaying { get; set; } = null;
    public float CurrentLength { get; set; } = 5;
    public RealTimeSince CurrentTime { get; set; } = 0;

    public bool IsPlaying => CurrentlyPlaying is not null;
    public bool IsPaused => false; // TODO: Implement

    public float Volume
    {
        get
        {
            return mySoundHandle.Volume;
        }
        set
        {
            mySoundHandle.Volume = value;
        }
    }

    VideoPlayer myVideoPlayer;
    SoundHandle mySoundHandle;
    GameObject localUiInstance = null;

    public override void OnStart()
    {
        if ( Interactable is not null )
            Interactable.OnInteract += OnInteract;

        if ( VideoPlayer is not null )
            VideoPlayer.OnVideoStart += OnVideoStart;
    }

    public override void FixedUpdate()
    {
        if ( GameObject.IsProxy ) return;

        if ( !IsPlaying && Queue.Count > 0 )
        {
            PlayNextVideo();
        }
    }

    [Broadcast]
    void PlayNextVideo()
    {
        CurrentlyPlaying = Queue[0];
        Queue.RemoveAt( 0 );

        if ( VideoPlayer is not null )
        {
            VideoPlayer.Play( CurrentlyPlaying.Url );
        }
    }

    [Broadcast]
    public void AddToQueue( string url )
    {
        var video = MediaVideo.CreateFromUrl( url );
        Queue.Add( video );
    }

    [Broadcast]
    public void RemoveFromQueue( string url )
    {
        Queue.RemoveAll( x => x.Url == url );
    }

    [Broadcast]
    public void Seek( float position )
    {
        // TODO: Seek to position
    }

    [Broadcast]
    public void TogglePause()
    {
        // TODO: Toggle pause
    }

    void OnVideoStart( VideoPlayer videoPlayer, SoundHandle soundHandle )
    {
        mySoundHandle = soundHandle;
        myVideoPlayer = videoPlayer;

        if ( CurrentlyPlaying.YoutubePlayer is not null )
        {
            CurrentLength = CurrentlyPlaying.YoutubePlayer.DurationSeconds;
        }
        else
        {
            CurrentLength = videoPlayer.Duration;
        }
        CurrentTime = 0;
    }

    void OnInteract( HomePlayer player )
    {
        if ( localUiInstance.IsValid() ) return;

        localUiInstance = SceneUtility.Instantiate( MediaPlayerUI );
        var mediaBrowser = localUiInstance.GetComponent<MediaBrowser>();
        if ( mediaBrowser is not null )
        {
            mediaBrowser.MediaPlayer = this;
        }
    }

    public override void OnDestroy()
    {
        if ( localUiInstance is not null )
        {
            localUiInstance.Destroy();
            localUiInstance = null;
        }
    }


    public void Write( ref ByteStream stream )
    {
        stream.Write( CurrentLength );
        stream.Write( CurrentTime );
        stream.Write<int>( Queue.Count );
        foreach ( var video in Queue )
        {
            video.Write( ref stream );
        }
        stream.Write<bool>( CurrentlyPlaying is not null );
        CurrentlyPlaying?.Write( ref stream );
    }

    public void Read( ByteStream stream )
    {
        CurrentLength = stream.Read<float>();
        CurrentTime = stream.Read<RealTimeSince>();
        var queueCount = stream.Read<int>();
        for ( int i = 0; i < queueCount; i++ )
        {
            var video = new MediaVideo();
            video.Read( stream );
            Queue.Add( video );
        }

        if ( stream.Read<bool>() )
        {
            CurrentlyPlaying = new MediaVideo();
            CurrentlyPlaying.Read( stream );
        }
    }

}
