using System;
using System.IO;
using Fody;
using ObjectApproval;
using Xunit;
// ReSharper disable UnusedVariable

public class WeaverTestHelperTests
{
    [Fact]
    public void Run()
    {
        var weaver = new TargetWeaver();
        var result = weaver.ExecuteTestRun("DummyAssembly.dll");
        Verify(result);
    }

    static void Verify(TestResult result)
    {
        ObjectApprover.VerifyWithJson(new
            {
                result.Errors,
                result.Messages,
                result.Warnings,
                result.AssemblyPath,
                result.Assembly.FullName
            },
            ScrubCurrentDirectory);
    }

    static string ScrubCurrentDirectory(string s)
    {
        return s.Replace(@"\\", @"\").Replace(Environment.CurrentDirectory, "");
    }

    [Fact]
    public void WithCustomAssemblyName()
    {
        var assemblyPath = Path.Combine(Environment.CurrentDirectory, "DummyAssembly.dll");
        var weaver = new TargetWeaver();
        var result = weaver.ExecuteTestRun(
            assemblyPath: assemblyPath,
            assemblyName: "NewName");
        Verify(result);
    }

    [Fact]
    public void WeaverUsingSymbols()
    {
        var start = DateTime.Now;
        var assemblyPath = Path.Combine(Environment.CurrentDirectory, "DummyAssembly.dll");
        var weaver = new WeaverUsingSymbols();
        var result = weaver.ExecuteTestRun(assemblyPath);

        var symbolsPath = Path.ChangeExtension(result.AssemblyPath, ".pdb");
        var symbolsFileInfo = new FileInfo(symbolsPath);

        Assert.True(symbolsFileInfo.Exists);
        Assert.True(start <= symbolsFileInfo.LastWriteTime);

        Verify(result);
    }
}