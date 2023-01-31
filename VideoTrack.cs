using StackExchange.Redis;
using VideoLibrary;

namespace Marimetso;

public class VideoTrack
{
    private static string TRACK_KEY_PREFIX = "video";
    private static string TITLE_KEY = "title";
    private static string LENGTH_KEY = "length";
    
    private YouTubeVideo _video;
    private string _id;

    public VideoTrack(YouTubeVideo video, string videoId)
    {
        this._video = video;
        this._id = videoId;
    }

    private int _Bitrate
    {
        get { return this._video.AudioBitrate; }
    }

    private double _EstimatedBytes
    {
        get { return this.LengthSeconds * this._Bitrate * 2 / 8; }
    }

    private string _Uri
    {
        get { return this._video.Uri; }
    }

    public double LengthSeconds
    {
        get { return (double)(this._video.Info.LengthSeconds ?? 0); }
    }

    public string Title
    {
      get { return this._video.Info.Title; }
    }

    public string VideoInfoString
    {
        get
        {
            var duration = TimeSpan.FromSeconds(this.LengthSeconds).ToString();
            var trackInfo = $"({this._id}) {this.Title} {duration}";
            return $"({this._id}) {this.Title} {duration}";
        }
    }

    public string ToSummaryString()
    {
        var contents = new string[] {
            $":::: {this.VideoInfoString} ::::",
            $"uri: {this._Uri}",
            $"bitrate: {this._Bitrate}",
            $"seconds: {this.LengthSeconds}",
            $"bytes: {this._EstimatedBytes}",
        };
        return String.Join("\n", contents);
    }

    public async Task WriteToFile(Action<string, string> WriteToLog)
    {
        WriteToLog(this.ToSummaryString(), "log");
        await FFMpegCore.FFMpegArguments
            .FromUrlInput(new Uri(this._Uri))
            .OutputToFile($"/Users/albert/Desktop/{this._id}.mp4")
            .ProcessAsynchronously();

        WriteToLog($"downloaded {this._id}", "log");
    }

    public HashEntry[] PersistenceEntries
    {
        get 
        {
            return new HashEntry[] {
                new HashEntry(TITLE_KEY, this.Title),
                new HashEntry(LENGTH_KEY, this.LengthSeconds)
            };
        }
    }

    public string PersistenceKey
    {
        get { return $"{TRACK_KEY_PREFIX}:{this._id}"; }
    }
}