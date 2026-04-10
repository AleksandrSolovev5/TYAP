using Interpreter;

namespace Runner;

public static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("Usage: Runner <file-path>");
            return 1;
        }

        string sourcePath = args[0];
        if (!File.Exists(sourcePath))
        {
            Console.Error.WriteLine($"Error: source file '{sourcePath}' not found.");
            return 1;
        }

        try
        {
            string sourceCode = File.ReadAllText(sourcePath);
            InterpreterEngine interpreter = new();
            interpreter.Execute(sourceCode);

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"{ex.GetType().Name}: {ex.Message}");
            return 1;
        }
    }
}