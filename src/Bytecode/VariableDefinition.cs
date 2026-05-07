namespace Bytecode;

public class VariableDefinition
{
    public VariableDefinition(string name, BytecodeValueType type, bool isConstant)
    {
        Name = name;
        Type = type;
        IsConstant = isConstant;
    }

    public string Name { get; }

    public BytecodeValueType Type { get; }

    public bool IsConstant { get; }

    public override string ToString()
    {
        string kind = IsConstant ? "const" : "var";

        return kind + " " + Type + " " + Name;
    }
}