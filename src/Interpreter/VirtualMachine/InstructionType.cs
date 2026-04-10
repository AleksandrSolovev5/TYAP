namespace Interpreter.VirtualMachine;

public enum InstructionType
{
    PushInt,
    PushFloat,
    PushString,
    Output,
    Halt,
}