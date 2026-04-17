using System.Globalization;

using Bytecode;

namespace VirtualMachine;

public class VirtualMachineExecutor
{
    private readonly List<Instruction> _instructions;
    private readonly Stack<object> _stack;
    private int _instructionPointer;

    public VirtualMachineExecutor(List<Instruction> instructions)
    {
        _instructions = instructions;
        _stack = new Stack<object>();
        _instructionPointer = 0;
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
                    _instructionPointer++;
                    break;

                case InstructionType.PushFloat:
                    _stack.Push(instruction.Operand!);
                    _instructionPointer++;
                    break;

                case InstructionType.PushString:
                    _stack.Push(instruction.Operand!);
                    _instructionPointer++;
                    break;

                case InstructionType.Output:
                    ExecuteOutput();
                    _instructionPointer++;
                    break;

                case InstructionType.Halt:
                    return;

                default:
                    throw new Exception("Unknown instruction type.");
            }
        }
    }

    private void ExecuteOutput()
    {
        object value = _stack.Pop();

        if (value is double)
        {
            double number = (double)value;
            Console.Write(number.ToString(CultureInfo.InvariantCulture));
            return;
        }

        Console.Write(value);
    }
}