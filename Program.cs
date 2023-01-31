using Spectre.Console;
using StackExchange.Redis;

namespace Marimetso;

public static class Program
{
    private static string WAITING_MSG = "listening for enqueued videos...";
    private static string QUEUE_KEY = "video:queue";

    public static async Task RunDownloadLoopAsync(StatusContext ctx)
    {
        using (var vlClient = new VideoLibClient())
        using (var redis = ConnectionMultiplexer.Connect("localhost"))
        {
            ctx.Status(WAITING_MSG);
            var db = redis.GetDatabase();

            while (true)
            {
                var nextVideoId = await db.ListLeftPopAsync(QUEUE_KEY);
                if (!nextVideoId.HasValue)
                {
                    ctx.Status(WAITING_MSG);
                    await Task.Delay(2000);
                    continue;
                }

                WriteLogMessage($"de-queued: {nextVideoId}");

                try
                {
                    WriteLogMessage($"getting info for {nextVideoId}");
                    var video = await vlClient.GetVideoAsync(nextVideoId.ToString());

                    if (video == null)
                    {
                        throw new Exception("Could not fetch video");
                    }

                    var track = new VideoTrack(video, nextVideoId.ToString());
                    ctx.Status($"downloading {track.VideoInfoString}");

                    await db.HashSetAsync(track.PersistenceKey, track.PersistenceEntries);

                    await track.WriteToFile(WriteLogMessage);
                }
                catch (System.Exception e)
                {
                    WriteLogMessage(e.Message, "error");
                }
            }

        }
    }

    public static async Task Main()
    {
        await AnsiConsole.Status()
            .AutoRefresh(true)
            .Spinner(Spinner.Known.BouncingBar)
            .StartAsync("initializing...", async ctx =>
            {
                await RunDownloadLoopAsync(ctx);
            });
    }

    private static void WriteLogMessage(string message, string level = "log")
    {
        var (color, label) = ("grey", "LOG");
        if (level == "error")
        {
            (color, label) = ("red", "ERROR");
        }

        AnsiConsole.MarkupLine($"[{color}]{label}:[/] {message.EscapeMarkup()}");
    }
}