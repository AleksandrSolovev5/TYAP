using System.Diagnostics;
using Xunit;

namespace TYAP.tests;

public class AcceptanceTests
{
    private readonly string m_runnerPath;
    private readonly string m_testDataPath;

    public AcceptanceTests()
    {
        m_runnerPath = Path.GetFullPath(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
                "..", "..", "..", "..", "src", "Runner", "bin", "Debug", "net10.0", "Runner.exe")
        );
    
        m_testDataPath = AppDomain.CurrentDomain.BaseDirectory;
    }

    private string RunInterpreter(string relativeFilePath)
    {
        var fullPath = Path.Combine(m_testDataPath, relativeFilePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"test file not found: {fullPath}");
        }

        var process = new Process{
            StartInfo = new ProcessStartInfo
            {
                FileName = m_runnerPath,
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
        var output = RunInterpreter("Features/OutputInteger.clvr");
        Assert.Equal("42 /", output);
    }
    
    [Fact]
    public void FeatureOutputFloatingPoint()
    {
        var output = RunInterpreter("Features/OutputFloatingPoint.clvr");
        Assert.Contains("3.14159", output);
        Assert.Contains("2.7118281828", output);
    }

    [Fact]
    public void FeatureOutputStringLiteral()
    {
        var output = RunInterpreter("Features/OutputStringLiteral.clvr");
        Assert.Contains("abcdefghijklmnopqrstuvwxyz", output);
        Assert.Contains("ABCDEFGHIJKLMNOPQRSTUVWXYZ", output);
    }
    
    [Fact]
    public void FeatureCommentSingleLine()
    {
        var output = RunInterpreter("Features/CommentSingleLine.clvr");
        Assert.Equal("хорошыйтест", output);
        Assert.DoesNotContain("коммент", output);
    }
    
    [Fact]
    public void FeatureCommentMultiLine()
    {
        var output = RunInterpreter("Features/CommentMultiLine.clvr");
        Assert.Equal("MAX", output);
        Assert.DoesNotContain("VPN", output);
    }

    /// <summary>
    /// приёмочные тесты для полноценных программ
    /// </summary>
    
    [Fact]
    public void ProgramHelloWorld()
    {
        var output = RunInterpreter("Programs/HelloWorld.clvr");
        Assert.Equal("Hello, World!", output);
    }

    [Fact]
    public void ProgramComplexOutput()
    {
        var output = RunInterpreter("Programs/ComplexOutput.clvr");
        string expected = "3.14 / 42 / Lorem ipsum dolor sit amet, consectetur adipiscing elit";
        Assert.Equal(expected, output);
    }

    [Fact]
    public void ProgramSequentialOutput()
    {
        var output = RunInterpreter("Programs/SequentialOutput.clvr");
        string expected = "Hello, World!  /  Hello, World!";
        Assert.Equal(expected, output);
    }
}