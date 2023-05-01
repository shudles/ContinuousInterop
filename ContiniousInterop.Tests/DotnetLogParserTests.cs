using ContinuousInterop.Cli;
using System;
using System.Text;
using System.Text.RegularExpressions;
using static ContiniousInterop.Tests.LogTestTexts;

namespace ContiniousInterop.Tests;

[TestClass]
public class DotnetLogParserTests
{
    private StreamReader _streamReader = default!;
    private MemoryStream _memorySteam = default!;

    public static Match useRegex(String input)
    {
        var regex = new Regex("\\[[^\\]]*\\]   [A-Za-z]+ [A-Za-z0-9]+ \\([^)]*\\) \\[[^\\]]*\\]", RegexOptions.IgnoreCase);
        return regex.Match(input);
    }



    [TestInitialize]
    public void TestInit()
    {
        _memorySteam = GenerateStream(FullLog);
        _streamReader = new StreamReader(_memorySteam);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _streamReader.Dispose();
        _memorySteam.Dispose();
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

        public ContinuousInterop.Cli.TestResult Result { get; set; }

        public int Time { get; set; }
    }

    public bool TryParse(string line, out ParsedTestLine parsed)
    {
        parsed = null;
        line = line.Trim();
        var split = line.Split(' ').Where(s => !string.IsNullOrEmpty(s)).ToArray();
        // [time] [Result] [Name] [NameSuffix(?)] [Time]

        // we need at least 3 splits
        if (split.Length < 3)
            return false;

        var resultSplitIndex = 0;
        var nameSplitIndex = 1;
        var timeSplitIndex = 2;

        // if we've got a time component, move everything up an index
        if (split[0].StartsWith('['))
        {
            // we now need at least 4 splits
            if (split.Length < 4)
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
        var potentialNameSuffix = split[timeSplitIndex];
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

    [TestMethod]
    public void Foop()
    {
        var input = "[2023-04-24T01:30:33.370Z]   Passed TestMultipleExportedInterfaceShared (,0,) [< 1 ms]";

        Assert.IsTrue(TryParse(input, out var parsed));

        Assert.AreEqual("TestMultipleExportedInterfaceShared (,0,)", parsed.Name);
        Assert.AreEqual(ContinuousInterop.Cli.TestResult.Passed, parsed.Result);
        Assert.AreEqual(1, parsed.Time);
    }


    [TestMethod]
    public async Task ParsesBaicLogIntoBlocks()
    {
        var handler = new LogHandler(new DotnetLogParser());
        var logContents = await handler.ReadLog(_streamReader).ToListAsync();

        Assert.AreEqual(6, logContents.Count);
    }
}