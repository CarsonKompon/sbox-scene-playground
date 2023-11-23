using Home;
using Home.UI.Arcade;
using Sandbox;
using MediaHelpers;
using System.Diagnostics;

namespace Home.Entities;

[Title( "Home Media Player" )]
[Category( "Home - Entities" )]
[Icon( "ondemand_video", "red", "white" )]
public sealed class MediaPlayer : BaseComponent, INetworkSerializable
{
    // Properties
    [Property] public Interactable Interactable { get; set; }
    [Property] public VideoPlayerComponent VideoPlayer { get; set; }
    [Property] public PanelComponent MediaPlayerUI { get; set; }

    // Variables
    public List<MediaVideo> Queue { get; set; } = new List<MediaVideo>();
    public MediaVideo CurrentlyPlaying { get; set; } = null;
    public float CurrentLength { get; set; } = 5;
    public RealTimeSince CurrentTime { get; set; } = 0;

    public bool IsPlaying = false;
    public bool IsPaused = false;
    bool IsLoading = false;

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
        if ( !IsPlaying && CurrentlyPlaying is not null )
        {
            LoadCurrentVideo();
        }

        if ( !GameObject.IsProxy )
        {
            if ( !IsPlaying && !IsLoading && Queue.Count > 0 )
            {
                CurrentlyPlaying = Queue[0];
                Queue.RemoveAt( 0 );
            }
            else if ( IsPlaying && CurrentTime > CurrentLength )
            {
                Stop();
            }
        }
    }

    async void LoadCurrentVideo()
    {
        IsLoading = true;

        string streamUrl = CurrentlyPlaying.Url;
        if ( MediaHelper.IsYoutubeUrl( streamUrl ) )
        {
            YoutubePlayerResponse youtube = await MediaHelper.GetYoutubePlayerResponseFromUrl( streamUrl );
            CurrentLength = youtube.DurationSeconds + 1f;
            CurrentTime = 0f;
            streamUrl = youtube.GetStreamUrl();
        }

        if ( VideoPlayer is not null )
        {
            VideoPlayer.Play( streamUrl );
        }

        IsLoading = false;

        IsPlaying = true;
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

    public void Stop()
    {
        IsPlaying = false;
        CurrentlyPlaying = null;
        VideoPlayer?.Stop();
    }

    [Broadcast]
    public void Seek( float position )
    {
        CurrentTime = position;
        VideoPlayer.Seek( position );
    }

    [Broadcast]
    public void TogglePause()
    {
        if ( CurrentlyPlaying is null ) return;
        IsPaused = !IsPaused;
        VideoPlayer.SetPause( IsPaused );
    }

    void OnVideoStart( VideoPlayer videoPlayer, SoundHandle soundHandle )
    {
        mySoundHandle = soundHandle;
        myVideoPlayer = videoPlayer;

        if ( CurrentlyPlaying is null ) return; // TODO: Make sure this isn't a problem

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

        MediaPlayerUI.Enabled = !MediaPlayerUI.Enabled;
    }

    public override void Update()
    {
        if ( MediaPlayerUI.Enabled && HomePlayer.Local is HomePlayer )
        {
            if ( HomePlayer.Local.Transform.Position.Distance( Transform.Position ) > 400 )
            {
                MediaPlayerUI.Enabled = false;
            }
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
        stream.Write( CurrentlyPlaying?.Url ?? "" );

        List<string> urls = new List<string>();
        foreach ( var video in Queue )
        {
            urls.Add( video.Url );
        }

        stream.Write( urls.Count );
        foreach ( var url in urls )
        {
            stream.Write( url );
        }
    }

    public void Read( ByteStream stream )
    {
        List<MediaVideo> queue = new List<MediaVideo>();

        string currentUrl = stream.Read<string>();
        int queueCount = stream.Read<int>();
        for ( int i = 0; i < queueCount; i++ )
        {
            string url = stream.Read<string>();
            queue.Add( MediaVideo.CreateFromUrl( url ) );
        }


        if ( CurrentlyPlaying.Url != currentUrl )
        {
            CurrentlyPlaying = MediaVideo.CreateFromUrl( currentUrl );
        }

        Queue.Clear();
        Queue.AddRange( queue );

    }

}
