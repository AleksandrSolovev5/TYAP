namespace VirtualMachine;

public class ConsoleRuntimeEnvironment : IRuntimeEnvironment
{
    public string? ReadLine()
    {
        return Console.ReadLine();
    }

    public void Write(string text)
    {
        Console.Write(text);
    }
}