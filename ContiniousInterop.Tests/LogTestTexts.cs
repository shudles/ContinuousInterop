namespace ContiniousInterop.Tests;

public static class LogTestTexts
{
    public const string FullLog = """
        Test run for C:\Code\ContinuousInterop\ContiniousInterop.Tests\bin\Debug\net7.0\ContiniousInterop.Tests.dll (.NETCoreApp,Version=v7.0)
        Microsoft (R) Test Execution Command Line Tool Version 17.5.0 (x64)
        Copyright (c) Microsoft Corporation.  All rights reserved.

        Starting test execution, please wait...
        A total of 1 test files matched the specified pattern.
          Passed WillPass [1 ms]
          Failed WillFail [11 ms]
          Error Message:
           Assert.Fail failed.
          Stack Trace:
             at ContiniousInterop.Tests.DotnetLogParserTests.WillFail() in C:\Code\ContinuousInterop\ContiniousInterop.Tests\DotnetLogParserTests.cs:line 16
           at System.RuntimeMethodHandle.InvokeMethod(Object target, Void** arguments, Signature sig, Boolean isConstructor)
           at System.Reflection.MethodInvoker.Invoke(Object obj, IntPtr* args, BindingFlags invokeAttr)

          Skipped Willignore

        Test Run Failed.
        Total tests: 3
             Passed: 1
             Failed: 1
            Skipped: 1
         Total time: 0.5214 Seconds
               VSTest:
                 MSB4181: The "Microsoft.TestPlatform.Build.Tasks.VSTestTask" task returned false but did not log an error.
             1>Done Building Project "C:\Code\ContinuousInterop\ContiniousInterop.Tests\ContiniousInterop.Tests.csproj" (VSTest
                target(s)) -- FAILED.
        """;

    public static MemoryStream GenerateStream(string text)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(text);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}
