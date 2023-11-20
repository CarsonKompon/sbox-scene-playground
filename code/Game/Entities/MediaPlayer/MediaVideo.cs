using MediaHelpers;
using Sandbox;

namespace Home;

public partial class MediaVideo
{
    public string Url { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }

    public YoutubePlayerResponse YoutubePlayer { get; set; } = null;

    public string ThumbnailUrl { get; set; } = "";
    public Texture Thumbnail
    {
        get
        {
            if ( _thumbnail == null ) _thumbnail = Texture.Load( ThumbnailUrl );
            return _thumbnail;
        }
        set
        {
            _thumbnail = value;
        }
    }
    private Texture _thumbnail = null;

    public async void LoadFromYoutube( string url )
    {
        YoutubePlayer = await MediaHelper.GetYoutubePlayerResponseFromUrl( url );
        Title = YoutubePlayer.Title;
        Author = YoutubePlayer.Author;
        Description = YoutubePlayer.Description;
        if ( YoutubePlayer.Thumbnails.Count > 0 ) ThumbnailUrl = YoutubePlayer.Thumbnails[0].Url;
    }

    public static MediaVideo CreateFromUrl( string url )
    {
        MediaVideo video = new MediaVideo();
        video.Url = url;
        video.Title = url;

        if ( MediaHelper.IsYoutubeUrl( url ) )
        {
            video.LoadFromYoutube( url );
        }

        return video;
    }

    public void Write( ref ByteStream stream )
    {
        stream.Write( Author );
        stream.Write( Description );
        stream.Write( Title );
        stream.Write( Url );
        stream.Write( ThumbnailUrl );
    }

    public void Read( ByteStream stream )
    {
        Author = stream.Read<string>();
        Description = stream.Read<string>();
        Title = stream.Read<string>();
        Url = stream.Read<string>();
        ThumbnailUrl = stream.Read<string>();
    }
}