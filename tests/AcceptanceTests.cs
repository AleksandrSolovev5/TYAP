using System.Diagnostics;
using Xunit;

namespace tests;

public class AcceptanceTests
{
    private readonly string _runnerPath;
    private readonly string _testDataPath;

    public AcceptanceTests()
    {
        _runnerPath = Path.GetFullPath(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..", "src", "Runner", "bin", "Debug", "net10.0", "Runner.exe")
        );

        _testDataPath = AppDomain.CurrentDomain.BaseDirectory;
    }

    private string RunInterpreter(string relativeFilePath)
    {
        string fullPath = Path.Combine(_testDataPath, relativeFilePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"test file not found: {fullPath}");
        }

        Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _runnerPath,
                Arguments = $"\"{fullPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return output.TrimEnd();
    }

    /// <summary>
    /// приёмочные тесты для отдельных функций языка
    /// </summary>
    [Fact]
    public void FeatureOutputInteger()
    {
        string output = RunInterpreter("Features/OutputInteger.clvr");
        Assert.Equal("42 /", output);
    }

    [Fact]
    public void FeatureOutputFloatingPoint()
    {
        string output = RunInterpreter("Features/OutputFloatingPoint.clvr");
        Assert.Contains("3.14159", output);
        Assert.Contains("2.7118281828", output);
    }

    [Fact]
    public void FeatureOutputStringLiteral()
    {
        string output = RunInterpreter("Features/OutputStringLiteral.clvr");
        Assert.Contains("abcdefghijklmnopqrstuvwxyz", output);
        Assert.Contains("ABCDEFGHIJKLMNOPQRSTUVWXYZ", output);
    }

    [Fact]
    public void FeatureCommentSingleLine()
    {
        string output = RunInterpreter("Features/CommentSingleLine.clvr");
        Assert.Equal("хорошыйтест", output);
        Assert.DoesNotContain("коммент", output);
    }

    [Fact]
    public void FeatureCommentMultiLine()
    {
        string output = RunInterpreter("Features/CommentMultiLine.clvr");
        Assert.Equal("MAX", output);
        Assert.DoesNotContain("VPN", output);
    }

    /// <summary>
    /// приёмочные тесты для полноценных программ
    /// </summary>
    [Fact]
    public void ProgramHelloWorld()
    {
        string output = RunInterpreter("Programs/HelloWorld.clvr");
        Assert.Equal("Hello, World!", output);
    }

    [Fact]
    public void ProgramComplexOutput()
    {
        string output = RunInterpreter("Programs/ComplexOutput.clvr");
        const string expected = "3.14 / 42 / Lorem ipsum dolor sit amet, consectetur adipiscing elit";
        Assert.Equal(expected, output);
    }

    [Fact]
    public void ProgramSequentialOutput()
    {
        string output = RunInterpreter("Programs/SequentialOutput.clvr");
        const string expected = "Hello, World!  /  Hello, World!";
        Assert.Equal(expected, output);
    }
}