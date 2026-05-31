using Bytecode;

namespace VirtualMachine;

public readonly struct RuntimeValue
{
    private readonly object _value;

    private RuntimeValue(BytecodeValueType type, object value)
    {
        Type = type;
        _value = value;
    }

    public BytecodeValueType Type { get; }

    public static RuntimeValue CreateInt(int value)
    {
        return new RuntimeValue(BytecodeValueType.Int, value);
    }

    public static RuntimeValue CreateFloat(double value)
    {
        return new RuntimeValue(BytecodeValueType.Float, value);
    }

    public static RuntimeValue CreateString(string value)
    {
        return new RuntimeValue(BytecodeValueType.String, value);
    }

    public static RuntimeValue CreateBool(bool value)
    {
        return new RuntimeValue(BytecodeValueType.Bool, value);
    }

    public int AsInt()
    {
        if (Type != BytecodeValueType.Int)
        {
            throw new InvalidOperationException($"Runtime type mismatch. Expected Int, but got {Type}.");
        }

        return (int)_value;
    }

    public double AsFloat()
    {
        if (Type != BytecodeValueType.Float)
        {
            throw new InvalidOperationException($"Runtime type mismatch. Expected Float, but got {Type}.");
        }

        return (double)_value;
    }

    public string AsString()
    {
        if (Type != BytecodeValueType.String)
        {
            throw new InvalidOperationException($"Runtime type mismatch. Expected String, but got {Type}.");
        }

        return (string)_value;
    }

    public bool AsBool()
    {
        if (Type != BytecodeValueType.Bool)
        {
            throw new InvalidOperationException($"Runtime type mismatch. Expected Bool, but got {Type}.");
        }

        return (bool)_value;
    }

    public override string ToString()
    {
        if (Type == BytecodeValueType.Bool)
        {
            return (bool)_value ? "true" : "false";
        }

        return _value?.ToString() ?? string.Empty;
    }
}