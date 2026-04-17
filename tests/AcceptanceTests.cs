using System.Diagnostics;

using Interpreter;

using Xunit;

namespace Tests;

public class AcceptanceTests
{
    private readonly string _testDataPath;

    public AcceptanceTests()
    {
        _testDataPath = AppDomain.CurrentDomain.BaseDirectory;
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

    private string RunInterpreter(string relativeFilePath)
    {
        string fullPath = Path.Combine(_testDataPath, relativeFilePath);
        if (!File.Exists(fullPath))
        {
            throw new Exception($"test file not found: {fullPath}");
        }

        string sourceCode = File.ReadAllText(fullPath);

        StringWriter stringWriter = new StringWriter();
        TextWriter originalOut = Console.Out;
        TextWriter originalError = Console.Error;

        try
        {
            Console.SetOut(stringWriter);
            Console.SetError(stringWriter);

            InterpreterEngine interpreter = new InterpreterEngine();
            interpreter.Execute(sourceCode);

            return stringWriter.ToString().TrimEnd();
        }
        catch (Exception ex)
        {
            return stringWriter.ToString() + ex.Message.TrimEnd();
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
            stringWriter.Dispose();
        }
    }
}