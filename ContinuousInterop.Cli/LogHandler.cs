using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ContinuousInterop.Cli.ExtensionUtils;

namespace ContinuousInterop.Cli;

public class LogHandler
{
    private ILogParser _logParser;
    public LogHandler(ILogParser logParser)
    {
        _logParser = logParser;
    }
    public async IAsyncEnumerable<ILogBlockContent> ReadLog(StreamReader streamReader)
    {
        string? yieldedPotential = null;
        string? yieldCache = null;

        // todo check for end of stream here? or what to do if a parser reads tot the end without giving back?
        var readNextSaveForYield = async () => yieldedPotential = await streamReader.ReadLineAsync();

        var readNext = async () => yieldCache switch
        {
            null => await readNextSaveForYield(),
            _ => NullifyAndReturn(ref yieldCache)
        };

        Action yielder = () => yieldCache = yieldedPotential;

        // or has a yieleded?
        while (!streamReader.EndOfStream)
        {
            yield return await _logParser.ParseNextBlock(readNext, yielder);
        }
    }

    public async Task Send(IAsyncEnumerable<ILogBlockContent> logBlockContents)
    {
        var foo = new TeamCityLogWriter();
        await foreach (var logBlockContent in logBlockContents)
        {
            await foo.WriteLog(logBlockContent);
        }
    }
}