using VideoLibrary;

namespace Marimetso;

public class VideoLibClient : IDisposable
{
    private VideoLibrary.Client<YouTubeVideo> _client;

    public VideoLibClient()
    {
        _client = Client.For(new YouTube());
    }

    private string GetYouTubeUrl(string videoId)
    {
        return $"https://youtu.be/{videoId}";
    }

    public Task<YouTubeVideo> GetVideoAsync(string videoId)
    {
        var url = this.GetYouTubeUrl(videoId);
        return _client.GetVideoAsync(url);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}