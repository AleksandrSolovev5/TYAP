namespace VirtualMachine;

public interface IRuntimeEnvironment
{
    string? ReadLine();

    void Write(string text);
}