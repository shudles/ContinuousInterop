using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace ContinuousInterop.Cli;

public interface ILogWriter
{
    public Task WriteLog(ILogBlockContent logBlockContent);
}
internal class TeamCityLogWriter : ILogWriter
{
    public async Task StartTest(string testName) => await Console.Out.WriteLineAsync($"##teamcity[testStarted name='{testName}']");
    public async Task SkipTest(string testName) => await Console.Out.WriteLineAsync($"##teamcity[testSkipped name='{testName}']");

    public async Task FailedTest(string testName) => await Console.Out.WriteLineAsync($"##teamcity[testFailed name='{testName}']");

    public async Task EndTest(string testName, int durationMs) => await Console.Out.WriteLineAsync($"##teamcity[testFinished name='{testName}' duration='{durationMs}']");

    public async Task WriteLog(ILogBlockContent logBlockContent)
    {
        switch (logBlockContent)
        {
            case TestLogBlockContent testLog:
                switch (testLog.Type)
                {
                    case TestResult.Skipped:
                        await SkipTest(testLog.Name);
                        break;
                    case TestResult.Passed:
                    case TestResult.Failed:
                        await StartTest(testLog.Name);
                        if (testLog.Type is TestResult.Failed)
                        {
                            await Console.Out.WriteLineAsync(testLog.Text); // write as error?
                            await FailedTest(testLog.Name);
                        }
                        else
                        {
                            await Console.Out.WriteLineAsync(testLog.Text);
                        }

                        await EndTest(testLog.Name, testLog.Duration);
                        break;
                }
                
                break;
            case StandardOutBlockContent stdOut:
                await Console.Out.WriteLineAsync(stdOut.Text);
                break;
        }
    }
}
