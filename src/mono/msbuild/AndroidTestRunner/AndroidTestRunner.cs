using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Microsoft.DotNet.XHarness.Tests.Runners;
using Microsoft.DotNet.XHarness.Tests.Runners.Core;

public class SimpleAndroidTestRunner : AndroidApplicationEntryPoint, IDevice
{
    private static List<string> s_testLibs = new List<string>();
    private static string? s_MainTestName;

    public static async Task<int> Main(string[] args)
    {
        s_testLibs = Directory.GetFiles(Environment.CurrentDirectory, "*.Tests.dll").ToList();
        if (s_testLibs.Count < 1)
        {
            Console.WriteLine($"Test libs were not found (*.Tests.dll was not found in {Environment.CurrentDirectory})");
            return -1;
        }
        s_MainTestName = Path.GetFileNameWithoutExtension(s_testLibs[0]);
        var simpleTestRunner = new SimpleAndroidTestRunner(true);
        await simpleTestRunner.RunAsync();
        Console.WriteLine("----- Done -----");
        return 0;
    }

    public SimpleAndroidTestRunner(bool verbose)
    {
        if (verbose)
        {
            MinimumLogLevel = MinimumLogLevel.Verbose;
            _maxParallelThreads = 1;
        }
        else
        {
            MinimumLogLevel = MinimumLogLevel.Info;
            _maxParallelThreads = Environment.ProcessorCount;
        }
    }

    protected override IEnumerable<TestAssemblyInfo> GetTestAssemblies()
    {
        foreach (string file in s_testLibs)
        {
            yield return new TestAssemblyInfo(Assembly.LoadFrom(file), file);
        }
    }

    protected override void TerminateWithSuccess() {}

    private int? _maxParallelThreads;

    protected override int? MaxParallelThreads => _maxParallelThreads;

    protected override IDevice Device => this;

    protected override TestRunnerType TestRunner => TestRunnerType.Xunit;

    protected override string? IgnoreFilesDirectory => null;

    protected override string IgnoredTraitsFilePath => "xunit-excludes.txt";

    public string BundleIdentifier => "net.dot." + s_MainTestName;

    public string? UniqueIdentifier { get; }

    public string? Name { get; }

    public string? Model { get; }

    public string? SystemName { get; }

    public string? SystemVersion { get; }

    public string? Locale { get; }

    public override TextWriter? Logger => null;

    public override string TestsResultsFinalPath
    {
        get
        {
            string? publicDir = Environment.GetEnvironmentVariable("DOCSDIR");
            if (string.IsNullOrEmpty(publicDir))
                throw new ArgumentException("DOCSDIR should not be empty");

            return Path.Combine(publicDir, "testResults.xml");
        }
    }
}
