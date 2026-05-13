using System.Text;

using VirtualMachine;

namespace Tests;

public class TestRuntimeEnvironment : IRuntimeEnvironment
{
    private readonly Queue<string> _inputs = [];

    private readonly StringBuilder _output = new();

    public TestRuntimeEnvironment(params string[] inputs)
    {
        _inputs = new Queue<string>(inputs);
    }

    public string? ReadLine()
    {
        if (_inputs.Count == 0)
        {
            throw new Exception("No more input lines available");
        }

        return _inputs.Dequeue();
    }

    public void Write(string text)
    {
        Console.Write(text);
        _output.Append(text);
    }

    public string GetOutput()
    {
        return _output.ToString();
    }
}