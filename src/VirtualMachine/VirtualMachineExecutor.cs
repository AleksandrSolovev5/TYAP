using System.Globalization;

using Bytecode;

namespace VirtualMachine;

public class VirtualMachineExecutor
{
    private readonly List<Instruction> _instructions;
    private readonly IRuntimeEnvironment _environment;
    private readonly Stack<RuntimeValue> _stack = [];
    private readonly Stack<Dictionary<string, RuntimeVariable>> _scopes = [];

    private int _instructionPointer;

    public VirtualMachineExecutor(List<Instruction> instructions)
        : this(instructions, new ConsoleRuntimeEnvironment())
    {
    }

    public VirtualMachineExecutor(
        List<Instruction> instructions,
        IRuntimeEnvironment environment)
    {
        _instructions = instructions;
        _environment = environment;
        _instructionPointer = 0;

        EnterScope();
    }

    public void Run()
    {
        while (_instructionPointer < _instructions.Count)
        {
            Instruction instruction = _instructions[_instructionPointer];
            _instructionPointer++;

            switch (instruction.InstructionType)
            {
                case InstructionType.PushInt:
                    _stack.Push(RuntimeValue.CreateInt((int)instruction.Operand!));
                    break;

                case InstructionType.PushFloat:
                    _stack.Push(RuntimeValue.CreateFloat((double)instruction.Operand!));
                    break;

                case InstructionType.PushString:
                    _stack.Push(RuntimeValue.CreateString((string)instruction.Operand!));
                    break;

                case InstructionType.Add:
                    ExecuteAdd();
                    break;

                case InstructionType.Subtract:
                    ExecuteSubtract();
                    break;

                case InstructionType.Multiply:
                    ExecuteMultiply();
                    break;

                case InstructionType.Divide:
                    ExecuteDivide();
                    break;

                case InstructionType.Mod:
                    ExecuteMod();
                    break;

                case InstructionType.UnaryMinus:
                    ExecuteUnaryMinus();
                    break;

                case InstructionType.DefineVariable:
                    ExecuteDefineVariable(instruction);
                    break;

                case InstructionType.LoadVariable:
                    ExecuteLoadVariable(instruction);
                    break;

                case InstructionType.StoreVariable:
                    ExecuteStoreVariable(instruction);
                    break;

                case InstructionType.Input:
                    ExecuteInput(instruction);
                    break;

                case InstructionType.Output:
                    ExecuteOutput();
                    break;

                case InstructionType.EnterScope:
                    EnterScope();
                    break;

                case InstructionType.ExitScope:
                    ExitScope();
                    break;

                case InstructionType.StringLength:
                    ExecuteStringLength();
                    break;

                case InstructionType.StringIndex:
                    ExecuteStringIndex();
                    break;

                case InstructionType.Halt:
                    return;

                default:
                    throw new Exception("Unknown instruction type: " + instruction.InstructionType);
            }
        }
    }

    private void ExecuteAdd()
    {
        RuntimeValue right = PopStack();
        RuntimeValue left = PopStack();

        if (left.Type == BytecodeValueType.Int && right.Type == BytecodeValueType.Int)
        {
            _stack.Push(RuntimeValue.CreateInt(left.AsInt() + right.AsInt()));
            return;
        }

        if (left.Type == BytecodeValueType.Float && right.Type == BytecodeValueType.Float)
        {
            _stack.Push(RuntimeValue.CreateFloat(left.AsFloat() + right.AsFloat()));
            return;
        }

        if (left.Type == BytecodeValueType.String && right.Type == BytecodeValueType.String)
        {
            _stack.Push(RuntimeValue.CreateString(left.AsString() + right.AsString()));
            return;
        }

        throw new Exception("Operator '+' cannot be applied to values of different or unsupported types.");
    }

    private void ExecuteSubtract()
    {
        RuntimeValue right = PopStack();
        RuntimeValue left = PopStack();

        if (left.Type == BytecodeValueType.Int && right.Type == BytecodeValueType.Int)
        {
            _stack.Push(RuntimeValue.CreateInt(left.AsInt() - right.AsInt()));
            return;
        }

        if (left.Type == BytecodeValueType.Float && right.Type == BytecodeValueType.Float)
        {
            _stack.Push(RuntimeValue.CreateFloat(left.AsFloat() - right.AsFloat()));
            return;
        }

        throw new Exception("Operator '-' supports only int and float values.");
    }

    private void ExecuteMultiply()
    {
        RuntimeValue right = PopStack();
        RuntimeValue left = PopStack();

        if (left.Type == BytecodeValueType.Int && right.Type == BytecodeValueType.Int)
        {
            _stack.Push(RuntimeValue.CreateInt(left.AsInt() * right.AsInt()));
            return;
        }

        if (left.Type == BytecodeValueType.Float && right.Type == BytecodeValueType.Float)
        {
            _stack.Push(RuntimeValue.CreateFloat(left.AsFloat() * right.AsFloat()));
            return;
        }

        throw new Exception("Operator '*' supports only int and float values.");
    }

    private void ExecuteDivide()
    {
        RuntimeValue right = PopStack();
        RuntimeValue left = PopStack();

        if (left.Type == BytecodeValueType.Int && right.Type == BytecodeValueType.Int)
        {
            int rightInt = right.AsInt();
            if (rightInt == 0)
            {
                throw new Exception("Division by zero.");
            }

            _stack.Push(RuntimeValue.CreateInt(left.AsInt() / rightInt));
            return;
        }

        if (left.Type == BytecodeValueType.Float && right.Type == BytecodeValueType.Float)
        {
            double rightDouble = right.AsFloat();
            if (rightDouble == 0.0)
            {
                throw new Exception("Division by zero.");
            }

            _stack.Push(RuntimeValue.CreateFloat(left.AsFloat() / rightDouble));
            return;
        }

        throw new Exception("Operator '/' supports only int and float values.");
    }

    private void ExecuteMod()
    {
        RuntimeValue right = PopStack();
        RuntimeValue left = PopStack();

        if (left.Type == BytecodeValueType.Int && right.Type == BytecodeValueType.Int)
        {
            int rightInt = right.AsInt();
            if (rightInt == 0)
            {
                throw new Exception("Division by zero.");
            }

            _stack.Push(RuntimeValue.CreateInt(left.AsInt() % rightInt));
            return;
        }

        throw new Exception("Operator '%' supports only int values.");
    }

    private void ExecuteUnaryMinus()
    {
        RuntimeValue value = PopStack();

        if (value.Type == BytecodeValueType.Int)
        {
            _stack.Push(RuntimeValue.CreateInt(-value.AsInt()));
            return;
        }

        if (value.Type == BytecodeValueType.Float)
        {
            _stack.Push(RuntimeValue.CreateFloat(-value.AsFloat()));
            return;
        }

        throw new Exception("Unary '-' supports only int and float values.");
    }

    private void ExecuteDefineVariable(Instruction instruction)
    {
        VariableDefinition definition = GetOperand<VariableDefinition>(instruction);

        if (_scopes.Peek().ContainsKey(definition.Name))
        {
            throw new Exception("Variable '" + definition.Name + "' is already defined in this scope.");
        }

        RuntimeValue value = PopStack();

        CheckValueType(value, definition.Type);

        RuntimeVariable variable = new RuntimeVariable(
            definition.Type,
            value,
            definition.IsConstant);

        _scopes.Peek().Add(definition.Name, variable);
    }

    private void ExecuteLoadVariable(Instruction instruction)
    {
        string name = GetOperand<string>(instruction);

        RuntimeVariable variable = FindVariableOrThrow(name);

        _stack.Push(variable.Value);
    }

    private void ExecuteStoreVariable(Instruction instruction)
    {
        string name = GetOperand<string>(instruction);

        RuntimeVariable variable = FindVariableOrThrow(name);

        if (variable.IsConstant)
        {
            throw new Exception("Cannot assign value to constant '" + name + "'.");
        }

        RuntimeValue value = PopStack();

        CheckValueType(value, variable.Type);

        variable.Value = value;
    }

    private void ExecuteInput(Instruction instruction)
    {
        string name = GetOperand<string>(instruction);

        RuntimeVariable variable = FindVariableOrThrow(name);

        if (variable.IsConstant)
        {
            throw new Exception("Cannot input value into constant '" + name + "'.");
        }

        string? input = _environment.ReadLine();

        if (input is null)
        {
            throw new Exception("Input expected, but end of stream was reached.");
        }

        variable.Value = ParseInputValue(input, variable.Type);
    }

    private void ExecuteOutput()
    {
        RuntimeValue value = PopStack();

        if (value.Type == BytecodeValueType.Float)
        {
            _environment.Write(value.AsFloat().ToString(CultureInfo.InvariantCulture));
            return;
        }

        _environment.Write(value.ToString());
    }

    private RuntimeValue ParseInputValue(string input, BytecodeValueType type)
    {
        return type switch
        {
            BytecodeValueType.Int => RuntimeValue.CreateInt(ParseIntInput(input)),
            BytecodeValueType.Float => RuntimeValue.CreateFloat(ParseFloatInput(input)),
            BytecodeValueType.String => RuntimeValue.CreateString(input),
            _ => throw new Exception("Unknown input type."),
        };
    }

    private static int ParseIntInput(string input)
    {
        int result;
        if (int.TryParse(
                input,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out result))
        {
            return result;
        }

        throw new Exception("Invalid int input: " + input);
    }

    private static double ParseFloatInput(string input)
    {
        double result;
        if (double.TryParse(
                input,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out result))
        {
            return result;
        }

        throw new Exception("Invalid float input: " + input);
    }

    private void CheckValueType(RuntimeValue value, BytecodeValueType expectedType)
    {
        if (value.Type != expectedType)
        {
            throw new Exception(
                "Runtime type mismatch. Expected " + expectedType + ", got " + value.Type + ".");
        }
    }

    private void ExecuteStringLength()
    {
        RuntimeValue value = PopStack();
        string text = value.AsString();

        StringInfo stringInfo = new StringInfo(text);
        _stack.Push(RuntimeValue.CreateInt(stringInfo.LengthInTextElements));
    }

    private void ExecuteStringIndex()
    {
        RuntimeValue indexValue = PopStack();
        RuntimeValue value = PopStack();

        string text = value.AsString();
        int index = indexValue.AsInt();

        StringInfo stringInfo = new StringInfo(text);

        if (index < 0 || index >= stringInfo.LengthInTextElements)
        {
            throw new Exception("String index is out of bounds.");
        }

        _stack.Push(RuntimeValue.CreateString(stringInfo.SubstringByTextElements(index, 1)));
    }

    private RuntimeVariable FindVariableOrThrow(string name)
    {
        foreach (Dictionary<string, RuntimeVariable> scope in _scopes)
        {
            RuntimeVariable? variable;
            if (scope.TryGetValue(name, out variable))
            {
                return variable;
            }
        }

        throw new Exception("Variable '" + name + "' is not defined.");
    }

    private void EnterScope()
    {
        _scopes.Push([]);
    }

    private void ExitScope()
    {
        if (_scopes.Count == 1)
        {
            throw new Exception("Cannot exit global scope.");
        }

        _scopes.Pop();
    }

    private RuntimeValue PopStack()
    {
        if (_stack.Count == 0)
        {
            throw new Exception("Stack is empty.");
        }

        return _stack.Pop();
    }

    private T GetOperand<T>(Instruction instruction)
    {
        if (instruction.Operand is T operand)
        {
            return operand;
        }

        throw new Exception(
            "Instruction " + instruction.InstructionType + " has invalid operand.");
    }

    private class RuntimeVariable
    {
        public RuntimeVariable(BytecodeValueType type, RuntimeValue value, bool isConstant)
        {
            Type = type;
            Value = value;
            IsConstant = isConstant;
        }

        public BytecodeValueType Type { get; }

        public RuntimeValue Value { get; set; }

        public bool IsConstant { get; }
    }
}