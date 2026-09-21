using Clms.Api.Instruments;
using Xunit;

namespace Clms.Tests;

public class InstrumentMessageParserTests
{
    // ------------------------------------------------------------------ happy path
    // Both examples come verbatim from CLMS ConOps 6.3.5.

    [Fact]
    public void Parses_quantitative_result_from_spec()
    {
        var ok = InstrumentMessageParser.TryParseLine(
            "1234|Test^B123^T3 Uptake|28|202208221340", out var msg, out var error);

        Assert.True(ok, error);
        Assert.NotNull(msg);
        Assert.Equal("1234", msg!.Barcode);
        Assert.Equal("Test", msg.MessageType);
        Assert.Equal("B123", msg.TestId);
        Assert.Equal("T3 Uptake", msg.TestDescription);
        Assert.Equal("28", msg.Value);
        Assert.Equal(new DateTime(2022, 8, 22, 13, 40, 0, DateTimeKind.Utc), msg.ResultedAtUtc);
    }

    [Fact]
    public void Parses_qualitative_result_from_spec()
    {
        var ok = InstrumentMessageParser.TryParseLine(
            "5678|Test^U123^Nitrite|Absent|202208221340", out var msg, out var error);

        Assert.True(ok, error);
        Assert.Equal("Absent", msg!.Value);
        Assert.Equal("U123", msg.TestId);
    }

    [Fact]
    public void Tolerates_surrounding_whitespace()
    {
        var ok = InstrumentMessageParser.TryParseLine(
            "  1234|Test^B123^T3 Uptake|28|202208221340  ", out var msg, out _);

        Assert.True(ok);
        Assert.Equal("1234", msg!.Barcode);
    }

    // --------------------------------------------------------------- malformed input

    [Theory]
    [InlineData("", "empty")]
    [InlineData("   ", "empty")]
    [InlineData("1234|Test^B123^T3 Uptake|28", "3")]                  // too few fields
    [InlineData("1234|Test^B123^T3 Uptake|28|202208221340|extra", "5")] // too many fields
    public void Rejects_wrong_field_count(string line, string _)
    {
        var ok = InstrumentMessageParser.TryParseLine(line, out var msg, out var error);

        Assert.False(ok);
        Assert.Null(msg);
        Assert.NotNull(error);
    }

    [Fact]
    public void Rejects_wrong_test_segment_count()
    {
        var ok = InstrumentMessageParser.TryParseLine(
            "1234|Test^B123|28|202208221340", out _, out var error);

        Assert.False(ok);
        Assert.Contains("caret-delimited", error);
    }

    [Theory]
    [InlineData("|Test^B123^T3 Uptake|28|202208221340")]    // no barcode
    [InlineData("1234|Test^^T3 Uptake|28|202208221340")]     // no test id
    [InlineData("1234|Test^B123^T3 Uptake||202208221340")]   // no value
    public void Rejects_empty_required_fields(string line)
    {
        Assert.False(InstrumentMessageParser.TryParseLine(line, out _, out _));
    }

    [Theory]
    [InlineData("2022-08-22 13:40")]
    [InlineData("20220822")]
    [InlineData("202213221340")]  // month 13
    [InlineData("notadate")]
    public void Rejects_bad_timestamps(string timestamp)
    {
        var ok = InstrumentMessageParser.TryParseLine(
            $"1234|Test^B123^T3 Uptake|28|{timestamp}", out _, out var error);

        Assert.False(ok);
        Assert.Contains("Timestamp", error);
    }

    // ------------------------------------------------------------------- whole files

    [Fact]
    public void ParseFile_separates_good_lines_from_bad()
    {
        var contents = string.Join('\n',
            "1234|Test^B123^T3 Uptake|28|202208221340",
            "",                                            // skipped, not an error
            "GARBAGE LINE",                                // failure on line 3
            "5678|Test^U123^Nitrite|Absent|202208221340");

        var result = InstrumentMessageParser.ParseFile(contents);

        Assert.Equal(2, result.Messages.Count);
        var failure = Assert.Single(result.Failures);
        Assert.Equal(3, failure.LineNumber);
        Assert.Equal("GARBAGE LINE", failure.RawLine);
    }

    [Fact]
    public void ParseFile_handles_windows_line_endings()
    {
        var contents = "1234|Test^B123^T3 Uptake|28|202208221340\r\n" +
                       "5678|Test^U123^Nitrite|Absent|202208221340\r\n";

        var result = InstrumentMessageParser.ParseFile(contents);

        Assert.Equal(2, result.Messages.Count);
        Assert.Empty(result.Failures);
    }

    [Fact]
    public void ParseFile_on_empty_input_yields_nothing()
    {
        var result = InstrumentMessageParser.ParseFile("");

        Assert.Empty(result.Messages);
        Assert.Empty(result.Failures);
    }
}
