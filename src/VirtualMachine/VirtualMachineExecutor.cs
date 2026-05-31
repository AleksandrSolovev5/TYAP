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

                case InstructionType.PushBool:
                    _stack.Push(RuntimeValue.CreateBool((bool)instruction.Operand!));
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

                case InstructionType.CompareEqual:
                    ExecuteCompare(InstructionType.CompareEqual);
                    break;

                case InstructionType.CompareNotEqual:
                    ExecuteCompare(InstructionType.CompareNotEqual);
                    break;

                case InstructionType.CompareLess:
                    ExecuteCompare(InstructionType.CompareLess);
                    break;

                case InstructionType.CompareGreater:
                    ExecuteCompare(InstructionType.CompareGreater);
                    break;

                case InstructionType.CompareLessEqual:
                    ExecuteCompare(InstructionType.CompareLessEqual);
                    break;

                case InstructionType.CompareGreaterEqual:
                    ExecuteCompare(InstructionType.CompareGreaterEqual);
                    break;

                case InstructionType.LogicalNot:
                    ExecuteLogicalNot();
                    break;

                case InstructionType.Jump:
                    _instructionPointer = GetOperand<int>(instruction);
                    break;

                case InstructionType.JumpIfFalse:
                    ExecuteJumpIfFalse(instruction);
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

        if (value.Type == BytecodeValueType.Bool)
        {
            _environment.Write(value.AsBool() ? "true" : "false");
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
            BytecodeValueType.Bool => RuntimeValue.CreateBool(ParseBoolInput(input)),
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

    private void ExecuteCompare(InstructionType compareType)
    {
        RuntimeValue right = PopStack();
        RuntimeValue left = PopStack();

        if (left.Type != right.Type)
        {
            throw new Exception("Cannot compare values of different types.");
        }

        bool result;

        if (left.Type == BytecodeValueType.Int)
        {
            int leftInt = left.AsInt();
            int rightInt = right.AsInt();

            result = compareType switch
            {
                InstructionType.CompareEqual => leftInt == rightInt,
                InstructionType.CompareNotEqual => leftInt != rightInt,
                InstructionType.CompareLess => leftInt < rightInt,
                InstructionType.CompareGreater => leftInt > rightInt,
                InstructionType.CompareLessEqual => leftInt <= rightInt,
                InstructionType.CompareGreaterEqual => leftInt >= rightInt,
                _ => throw new Exception("Unknown comparison type."),
            };
        }
        else if (left.Type == BytecodeValueType.Float)
        {
            double leftFloat = left.AsFloat();
            double rightFloat = right.AsFloat();

            result = compareType switch
            {
                InstructionType.CompareEqual => leftFloat == rightFloat,
                InstructionType.CompareNotEqual => leftFloat != rightFloat,
                InstructionType.CompareLess => leftFloat < rightFloat,
                InstructionType.CompareGreater => leftFloat > rightFloat,
                InstructionType.CompareLessEqual => leftFloat <= rightFloat,
                InstructionType.CompareGreaterEqual => leftFloat >= rightFloat,
                _ => throw new Exception("Unknown comparison type."),
            };
        }
        else if (left.Type == BytecodeValueType.String)
        {
            string leftString = left.AsString();
            string rightString = right.AsString();

            result = compareType switch
            {
                InstructionType.CompareEqual => leftString == rightString,
                InstructionType.CompareNotEqual => leftString != rightString,
                _ => throw new Exception("Operator is not supported for string values."),
            };
        }
        else if (left.Type == BytecodeValueType.Bool)
        {
            bool leftBool = left.AsBool();
            bool rightBool = right.AsBool();

            result = compareType switch
            {
                InstructionType.CompareEqual => leftBool == rightBool,
                InstructionType.CompareNotEqual => leftBool != rightBool,
                _ => throw new Exception("Operator is not supported for bool values."),
            };
        }
        else
        {
            throw new Exception("Unsupported type for comparison.");
        }

        _stack.Push(RuntimeValue.CreateBool(result));
    }

    private void ExecuteLogicalNot()
    {
        RuntimeValue value = PopStack();
        _stack.Push(RuntimeValue.CreateBool(!value.AsBool()));
    }

    private void ExecuteJumpIfFalse(Instruction instruction)
    {
        int target = GetOperand<int>(instruction);
        RuntimeValue value = PopStack();

        if (!value.AsBool())
        {
            _instructionPointer = target;
        }
    }

    private static bool ParseBoolInput(string input)
    {
        if (input == "true")
        {
            return true;
        }

        if (input == "false")
        {
            return false;
        }

        throw new Exception("Invalid bool input: " + input);
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