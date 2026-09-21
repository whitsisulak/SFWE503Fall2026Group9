using Clms.Api.Data;
using Clms.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Clms.Api.Instruments;

public sealed class InstrumentWatcherOptions
{
    /// <summary>Folder the instruments drop result files into.</summary>
    public string DropPath { get; set; } = "/data/instrument-drop";
    public int PollSeconds { get; set; } = 5;
    public string SearchPattern { get; set; } = "*.txt";
}

/// <summary>
/// Watches a folder for instrument output files, parses them, and attaches the
/// results to the matching TestOrder by barcode (ConOps 6.3.5).
///
/// Runs in-process with the API. It's a BackgroundService rather than request-driven
/// code so it can be unit tested and reasoned about independently — and so a slow
/// or malformed batch never blocks an HTTP request.
///
/// Processed files move to _processed/; files with any bad lines move to _failed/
/// alongside a .error.txt describing what went wrong. Nothing is deleted.
/// </summary>
public sealed class InstrumentFileWatcher(
    IServiceScopeFactory scopeFactory,
    InstrumentWatcherOptions options,
    ILogger<InstrumentFileWatcher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var drop = options.DropPath;
        var processed = Path.Combine(drop, "_processed");
        var failed = Path.Combine(drop, "_failed");

        Directory.CreateDirectory(drop);
        Directory.CreateDirectory(processed);
        Directory.CreateDirectory(failed);

        logger.LogInformation("Instrument watcher polling {Drop} every {Seconds}s",
            drop, options.PollSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                foreach (var path in Directory.EnumerateFiles(drop, options.SearchPattern))
                    await ProcessFileAsync(path, processed, failed, stoppingToken);
            }
            catch (Exception ex)
            {
                // Never let one bad sweep kill the worker.
                logger.LogError(ex, "Instrument watcher sweep failed");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(options.PollSeconds), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task ProcessFileAsync(
        string path, string processedDir, string failedDir, CancellationToken ct)
    {
        string contents;
        try
        {
            contents = await File.ReadAllTextAsync(path, ct);
        }
        catch (IOException)
        {
            // Instrument may still be writing it; pick it up on the next sweep.
            logger.LogDebug("File {Path} is locked, retrying next sweep", path);
            return;
        }

        var parsed = InstrumentMessageParser.ParseFile(contents);
        var instrumentId = Path.GetFileNameWithoutExtension(path);

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClmsDbContext>();

        var unmatched = new List<string>();
        var applied = 0;

        foreach (var message in parsed.Messages)
        {
            var order = await db.TestOrders
                .Include(o => o.Results)
                .FirstOrDefaultAsync(o => o.Barcode == message.Barcode, ct);

            if (order is null)
            {
                unmatched.Add($"No test order found for barcode '{message.Barcode}'.");
                continue;
            }

            // Idempotency: re-dropping the same file must not duplicate results.
            var alreadyRecorded = order.Results.Any(r =>
                r.TestCode == message.TestId && r.ResultedAtUtc == message.ResultedAtUtc);

            if (alreadyRecorded) continue;

            order.Results.Add(new TestResult
            {
                TestCode = message.TestId,
                TestDescription = message.TestDescription,
                Value = message.Value,
                ResultedAtUtc = message.ResultedAtUtc,
                InstrumentId = instrumentId
            });

            if (order.Status < TestOrderStatus.Resulted)
                order.Status = TestOrderStatus.Resulted;

            db.AuditEntries.Add(new AuditEntry
            {
                Actor = $"instrument:{instrumentId}",
                Action = "ResultReceived",
                EntityName = nameof(TestOrder),
                EntityId = order.Id.ToString(),
                Details = $"{message.TestId} ({message.TestDescription}) = {message.Value}"
            });

            applied++;
        }

        await db.SaveChangesAsync(ct);

        var problems = parsed.Failures
            .Select(f => $"line {f.LineNumber}: {f.Error} -- {f.RawLine}")
            .Concat(unmatched)
            .ToList();

        var fileName = Path.GetFileName(path);
        var stamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");

        if (problems.Count > 0)
        {
            var dest = Path.Combine(failedDir, $"{stamp}_{fileName}");
            File.Move(path, dest, overwrite: true);
            await File.WriteAllLinesAsync($"{dest}.error.txt", problems, ct);

            logger.LogWarning(
                "Instrument file {File}: applied {Applied} result(s), {Problems} problem(s)",
                fileName, applied, problems.Count);
        }
        else
        {
            File.Move(path, Path.Combine(processedDir, $"{stamp}_{fileName}"), overwrite: true);
            logger.LogInformation(
                "Instrument file {File}: applied {Applied} result(s)", fileName, applied);
        }
    }
}
