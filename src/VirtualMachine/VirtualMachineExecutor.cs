using System.Globalization;

using Bytecode;

namespace VirtualMachine;

public class VirtualMachineExecutor
{
    private readonly List<Instruction> _instructions;
    private readonly IRuntimeEnvironment _environment;
    private readonly Stack<object> _stack = [];
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

            switch (instruction.InstructionType)
            {
                case InstructionType.PushInt:
                    _stack.Push(instruction.Operand!);
                    MoveNext();
                    break;

                case InstructionType.PushFloat:
                    _stack.Push(instruction.Operand!);
                    MoveNext();
                    break;

                case InstructionType.PushString:
                    _stack.Push(instruction.Operand!);
                    MoveNext();
                    break;

                case InstructionType.Add:
                    ExecuteAdd();
                    MoveNext();
                    break;

                case InstructionType.Subtract:
                    ExecuteSubtract();
                    MoveNext();
                    break;

                case InstructionType.Multiply:
                    ExecuteMultiply();
                    MoveNext();
                    break;

                case InstructionType.Divide:
                    ExecuteDivide();
                    MoveNext();
                    break;

                case InstructionType.Mod:
                    ExecuteMod();
                    MoveNext();
                    break;

                case InstructionType.UnaryMinus:
                    ExecuteUnaryMinus();
                    MoveNext();
                    break;

                case InstructionType.DefineVariable:
                    ExecuteDefineVariable(instruction);
                    MoveNext();
                    break;

                case InstructionType.LoadVariable:
                    ExecuteLoadVariable(instruction);
                    MoveNext();
                    break;

                case InstructionType.StoreVariable:
                    ExecuteStoreVariable(instruction);
                    MoveNext();
                    break;

                case InstructionType.Input:
                    ExecuteInput(instruction);
                    MoveNext();
                    break;

                case InstructionType.Output:
                    ExecuteOutput();
                    MoveNext();
                    break;

                case InstructionType.EnterScope:
                    EnterScope();
                    MoveNext();
                    break;

                case InstructionType.ExitScope:
                    ExitScope();
                    MoveNext();
                    break;

                case InstructionType.StringLength:
                    ExecuteStringLength();
                    MoveNext();
                    break;

                case InstructionType.StringIndex:
                    ExecuteStringIndex();
                    MoveNext();
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
        object right = PopStack();
        object left = PopStack();

        if (left is int leftInt && right is int rightInt)
        {
            _stack.Push(leftInt + rightInt);
            return;
        }

        if (left is double leftDouble && right is double rightDouble)
        {
            _stack.Push(leftDouble + rightDouble);
            return;
        }

        if (left is string leftString && right is string rightString)
        {
            _stack.Push(leftString + rightString);
            return;
        }

        throw new Exception("Operator '+' cannot be applied to values of different or unsupported types.");
    }

    private void ExecuteSubtract()
    {
        object right = PopStack();
        object left = PopStack();

        if (left is int leftInt && right is int rightInt)
        {
            _stack.Push(leftInt - rightInt);
            return;
        }

        if (left is double leftDouble && right is double rightDouble)
        {
            _stack.Push(leftDouble - rightDouble);
            return;
        }

        throw new Exception("Operator '-' supports only int and float values.");
    }

    private void ExecuteMultiply()
    {
        object right = PopStack();
        object left = PopStack();

        if (left is int leftInt && right is int rightInt)
        {
            _stack.Push(leftInt * rightInt);
            return;
        }

        if (left is double leftDouble && right is double rightDouble)
        {
            _stack.Push(leftDouble * rightDouble);
            return;
        }

        throw new Exception("Operator '*' supports only int and float values.");
    }

    private void ExecuteDivide()
    {
        object right = PopStack();
        object left = PopStack();

        if (left is int leftInt && right is int rightInt)
        {
            if (rightInt == 0)
            {
                throw new Exception("Division by zero.");
            }

            _stack.Push(leftInt / rightInt);
            return;
        }

        if (left is double leftDouble && right is double rightDouble)
        {
            if (rightDouble == 0.0)
            {
                throw new Exception("Division by zero.");
            }

            _stack.Push(leftDouble / rightDouble);
            return;
        }

        throw new Exception("Operator '/' supports only int and float values.");
    }

    private void ExecuteMod()
    {
        object right = PopStack();
        object left = PopStack();

        if (left is int leftInt && right is int rightInt)
        {
            if (rightInt == 0)
            {
                throw new Exception("Division by zero.");
            }

            _stack.Push(leftInt % rightInt);
            return;
        }

        throw new Exception("Operator '%' supports only int values.");
    }

    private void ExecuteUnaryMinus()
    {
        object value = PopStack();

        if (value is int intValue)
        {
            _stack.Push(-intValue);
            return;
        }

        if (value is double doubleValue)
        {
            _stack.Push(-doubleValue);
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

        object value = PopStack();

        CheckValueType(value, definition.Type);

        RuntimeVariable variable = new(
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

        object value = PopStack();

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
        object value = PopStack();

        if (value is double number)
        {
            _environment.Write(number.ToString(CultureInfo.InvariantCulture));
            return;
        }

        _environment.Write(value.ToString() ?? string.Empty);
    }

    private object ParseInputValue(string input, BytecodeValueType type)
    {
        return type switch
        {
            BytecodeValueType.Int => ParseIntInput(input),
            BytecodeValueType.Float => ParseFloatInput(input),
            BytecodeValueType.String => input,
            _ => throw new Exception("Unknown input type."),
        };
    }

    private static int ParseIntInput(string input)
    {
        if (int.TryParse(
                input,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int result))
        {
            return result;
        }

        throw new Exception("Invalid int input: " + input);
    }

    private static double ParseFloatInput(string input)
    {
        if (double.TryParse(
                input,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double result))
        {
            return result;
        }

        throw new Exception("Invalid float input: " + input);
    }

    private void CheckValueType(object value, BytecodeValueType expectedType)
    {
        if (expectedType == BytecodeValueType.Int && value is int)
        {
            return;
        }

        if (expectedType == BytecodeValueType.Float && value is double)
        {
            return;
        }

        if (expectedType == BytecodeValueType.String && value is string)
        {
            return;
        }

        throw new Exception(
            "Runtime type mismatch. Expected " +
            expectedType +
            ", got " +
            value.GetType().Name +
            ".");
    }

    private void ExecuteStringLength()
    {
        object value = PopStack();

        if (value is not string text)
        {
            throw new Exception("Function 'len' expects string argument.");
        }

        _stack.Push(text.Length);
    }

    private void ExecuteStringIndex()
    {
        object indexValue = PopStack();
        object value = PopStack();

        if (value is not string text)
        {
            throw new Exception("String index operator expects string value.");
        }

        if (indexValue is not int index)
        {
            throw new Exception("String index must be int.");
        }

        if (index < 0 || index >= text.Length)
        {
            throw new Exception("String index is out of bounds.");
        }

        _stack.Push(text[index].ToString());
    }

    private RuntimeVariable FindVariableOrThrow(string name)
    {
        foreach (Dictionary<string, RuntimeVariable> scope in _scopes)
        {
            if (scope.TryGetValue(name, out RuntimeVariable? variable))
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

    private object PopStack()
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
            "Instruction " +
            instruction.InstructionType +
            " has invalid operand.");
    }

    private void MoveNext()
    {
        _instructionPointer++;
    }

    private class RuntimeVariable(
        BytecodeValueType type,
        object value,
        bool isConstant)
    {
        public BytecodeValueType Type { get; } = type;

        public object Value { get; set; } = value;

        public bool IsConstant { get; } = isConstant;
    }
}