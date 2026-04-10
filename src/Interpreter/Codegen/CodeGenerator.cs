using Interpreter.Ast;
using Interpreter.VirtualMachine;

namespace Interpreter.Codegen;

public class CodeGenerator : IAstVisitor
{
    private List<Instruction> _instructions = [];

    public List<Instruction> Generate(ProgramNode programNode)
    {
        _instructions = new List<Instruction>();

        programNode.Accept(this);
        _instructions.Add(new Instruction(InstructionType.Halt));

        return _instructions;
    }

    public void VisitProgramNode(ProgramNode programNode)
    {
        foreach (StatementNode statement in programNode.Statements)
        {
            statement.Accept(this);
        }
    }

    public void VisitOutputStatementNode(OutputStatementNode outputStatementNode)
    {
        foreach (ExpressionNode argument in outputStatementNode.Arguments)
        {
            argument.Accept(this);
            _instructions.Add(new Instruction(InstructionType.Output));
        }
    }

    public void VisitIntLiteralNode(IntLiteralNode intLiteralNode)
    {
        _instructions.Add(new Instruction(InstructionType.PushInt, intLiteralNode.Value));
    }

    public void VisitFloatLiteralNode(FloatLiteralNode floatLiteralNode)
    {
        _instructions.Add(new Instruction(InstructionType.PushFloat, floatLiteralNode.Value));
    }

    public void VisitStringLiteralNode(StringLiteralNode stringLiteralNode)
    {
        _instructions.Add(new Instruction(InstructionType.PushString, stringLiteralNode.Value));
    }
}