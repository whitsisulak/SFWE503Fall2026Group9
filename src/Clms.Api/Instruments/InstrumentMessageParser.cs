using System.Globalization;

namespace Clms.Api.Instruments;

/// <summary>
/// One parsed line of instrument output.
/// </summary>
/// <param name="Barcode">Sample barcode — joins the result back to a TestOrder.</param>
/// <param name="MessageType">Message type token, e.g. "Test".</param>
/// <param name="TestId">Instrument test code, e.g. "B123".</param>
/// <param name="TestDescription">Human-readable test name, e.g. "T3 Uptake".</param>
/// <param name="Value">Result value — quantitative ("28") or qualitative ("Absent").</param>
/// <param name="ResultedAtUtc">Timestamp the instrument reported.</param>
public sealed record InstrumentMessage(
    string Barcode,
    string MessageType,
    string TestId,
    string TestDescription,
    string Value,
    DateTime ResultedAtUtc);

/// <summary>
/// Parses the simplified ORU-HL7 format defined in the CLMS ConOps (6.3.5):
///
///     BARCODE|Message type (Test)^Test ID^Test description|Value|Date and time
///
/// Examples from the spec:
///     1234|Test^B123^T3 Uptake|28|202208221340
///     5678|Test^U123^Nitrite|Absent|202208221340
///
/// Deliberately tolerant of surrounding whitespace and blank lines, but strict
/// about field counts and the timestamp format — a malformed line should be
/// reported and quarantined, never silently guessed at.
/// </summary>
public static class InstrumentMessageParser
{
    private const string TimestampFormat = "yyyyMMddHHmm";
    private const int ExpectedFieldCount = 4;
    private const int ExpectedTestSegmentCount = 3;

    public static bool TryParseLine(string line, out InstrumentMessage? message, out string? error)
    {
        message = null;
        error = null;

        if (string.IsNullOrWhiteSpace(line))
        {
            error = "Line is empty.";
            return false;
        }

        var fields = line.Trim().Split('|');
        if (fields.Length != ExpectedFieldCount)
        {
            error = $"Expected {ExpectedFieldCount} pipe-delimited fields but found {fields.Length}.";
            return false;
        }

        var barcode = fields[0].Trim();
        if (barcode.Length == 0)
        {
            error = "Barcode field is empty.";
            return false;
        }

        var testSegments = fields[1].Split('^');
        if (testSegments.Length != ExpectedTestSegmentCount)
        {
            error = $"Expected {ExpectedTestSegmentCount} caret-delimited test segments " +
                    $"but found {testSegments.Length}.";
            return false;
        }

        var messageType = testSegments[0].Trim();
        var testId = testSegments[1].Trim();
        var testDescription = testSegments[2].Trim();

        if (testId.Length == 0)
        {
            error = "Test ID is empty.";
            return false;
        }

        var value = fields[2].Trim();
        if (value.Length == 0)
        {
            error = "Result value is empty.";
            return false;
        }

        var rawTimestamp = fields[3].Trim();
        if (!DateTime.TryParseExact(
                rawTimestamp,
                TimestampFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var resultedAt))
        {
            error = $"Timestamp '{rawTimestamp}' does not match format {TimestampFormat}.";
            return false;
        }

        message = new InstrumentMessage(
            barcode, messageType, testId, testDescription, value, resultedAt);
        return true;
    }

    /// <summary>
    /// Parses a whole file's contents, returning successfully parsed messages and
    /// per-line failures separately so the caller can decide what to do with each.
    /// </summary>
    public static InstrumentFileParseResult ParseFile(string contents)
    {
        var messages = new List<InstrumentMessage>();
        var failures = new List<InstrumentLineFailure>();

        var lines = contents.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].TrimEnd('\r');
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (TryParseLine(line, out var message, out var error))
                messages.Add(message!);
            else
                failures.Add(new InstrumentLineFailure(i + 1, line, error!));
        }

        return new InstrumentFileParseResult(messages, failures);
    }
}

public sealed record InstrumentLineFailure(int LineNumber, string RawLine, string Error);

public sealed record InstrumentFileParseResult(
    IReadOnlyList<InstrumentMessage> Messages,
    IReadOnlyList<InstrumentLineFailure> Failures);
