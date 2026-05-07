namespace Bytecode;

public enum InstructionType
{
    PushInt,
    PushFloat,
    PushString,
    Output,
    Halt,
    Add,
    Subtract,
    Multiply,
    Divide,
    Mod,
    UnaryMinus,
    DefineVariable,
    LoadVariable,
    StoreVariable,
    Input,
    EnterScope,
    ExitScope,
}