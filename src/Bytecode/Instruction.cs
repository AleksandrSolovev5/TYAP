namespace Bytecode;

public class Instruction
{
    public Instruction(InstructionType instructionType)
    {
        InstructionType = instructionType;
        Operand = null;
    }

    public Instruction(InstructionType instructionType, object operand)
    {
        InstructionType = instructionType;
        Operand = operand;
    }

    public InstructionType InstructionType { get; }

    public object? Operand { get; }

    public override string ToString()
    {
        if (Operand == null)
        {
            return InstructionType.ToString();
        }

        return InstructionType + " " + Operand;
    }
}