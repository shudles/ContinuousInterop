using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace ContinuousInterop.Cli;


public interface ILogParser
{
    public Task<ILogBlockContent> ParseNextBlock(Func<Task<string>> readNextLine, Action yielder);
}

public enum LogBlockType
{
    Info,
    Warning, // std error?
    Error,
    Step,
    Artifact,
    Suite,
    Test
}

public enum TestResult
{
    Passed,
    Failed,
    Skipped,
}

public interface ILogBlockContent
{

}

public record class TestLogBlockContent : ILogBlockContent
{
    public TestResult Type { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Text { get; init; } = string.Empty; // stdout and stderror?

    public int Duration { get; init; }
}

public record class StandardOutBlockContent : ILogBlockContent
{
    public string Text { get; init; } = string.Empty;
}

/// <summary>
/// Is returned when an <see cref="ILogParser"/> has no interest in the block
/// </summary
public struct EmptyLogContent : ILogBlockContent { }


// more of a Jenkins parser
public class DotnetLogParser : ILogParser
{

    public async Task<ILogBlockContent> ParseNextBlock(Func<Task<string>> readNextLine, Action yielder)
    {
        // keep reading until we see something that is a test line
        var line = await readNextLine();
        var builder = new StringBuilder(line);
        builder.AppendLine();
        if (TryParseTestLine(line, out var parsed))
        {
            switch (parsed.Result)
            {
                case TestResult.Passed:
                    return new TestLogBlockContent() { Type = parsed.Result, Name = parsed.Name, Duration = parsed.Time, Text = builder.ToString() };
                case TestResult.Skipped:
                    return new TestLogBlockContent() { Type = parsed.Result, Name = parsed.Name, Duration = parsed.Time, Text = builder.ToString()};
                case TestResult.Failed:
                    // keep reading until another test, or the test summary? or cheat for now and just use an empty line
                    while (true)
                    {
                        line = await readNextLine();
                        if (string.IsNullOrEmpty(line))
                        {
                            yielder();
                            return new TestLogBlockContent() { Type = parsed.Result, Name = parsed.Name, Duration = parsed.Time, Text = builder.ToString() };
                        }
                        builder.AppendLine(line);
                    }
            }
        }
        while (true)
        {
            line = await readNextLine();
            if (line is null || TryParseTestLine(line, out var _))
            {
                yielder();
                return new StandardOutBlockContent() { Text = builder.ToString() };
            }
            builder.AppendLine(line);
        }
    }

    public bool ParseResult(string input, out ContinuousInterop.Cli.TestResult result)
    {
        return Enum.TryParse(input, out result);
    }

    public bool ParseName(string input, out string name)
    {
        name = input;
        return true;
    }

    public bool ParseTime(string input, out int time)
    {
        var numerics = string.Join(string.Empty, input.Where(char.IsDigit));
        return int.TryParse(numerics, out time);
    }

    public record class ParsedTestLine
    {
        public string Name { get; set; }

        public TestResult Result { get; set; }

        public int Time { get; set; }
    }

    public bool TryParseTestLine(string line, out ParsedTestLine parsed)
    {
        parsed = null;
        line = line.Trim();
        var split = line.Split(' ').Where(s => !string.IsNullOrEmpty(s)).ToArray();
        // [time] [Result] [Name] [NameSuffix(?)] [Time]

        // we need at least 2 splits
        if (split.Length < 2)
            return false;

        var resultSplitIndex = 0;
        var nameSplitIndex = 1;
        var timeSplitIndex = 2;

        // if we've got a time component, move everything up an index
        if (split[0].StartsWith('['))
        {
            // we now need at least 3 splits
            if (split.Length < 3)
                return false;

            resultSplitIndex++;
            nameSplitIndex++;
            timeSplitIndex++;
        }

        if (!ParseResult(split[resultSplitIndex], out var result))
        {
            return false;
        }

        var nameStr = split[nameSplitIndex];
        
        var potentialNameSuffix = split.Length < timeSplitIndex ? split[timeSplitIndex] : "[";
        if (!potentialNameSuffix.StartsWith('['))
        {
            // if the time split isn't in its slot because there's a suffix there
            // it means there's a name suffix; bump it up and index
            // and merge the name
            timeSplitIndex++;
            nameStr = $"{nameStr} {potentialNameSuffix}";
        }

        if (!ParseName(nameStr, out var name))
        {
            return false;
        }

        if (result is TestResult.Skipped)
        {
            parsed = new ParsedTestLine() { Name = name, Result = result, Time = 0 };
            return true;
        }

        // time is now just the next int we can find
        for (var i = timeSplitIndex; i < split.Length; i++)
        {
            if (ParseTime(split[i], out var time))
            {
                parsed = new ParsedTestLine() { Name = name, Result = result, Time = time };
                return true;
            }
        }
        return false;
    }
}
