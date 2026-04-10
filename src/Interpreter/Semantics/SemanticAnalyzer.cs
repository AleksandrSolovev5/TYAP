using Interpreter.Ast;

namespace Interpreter.Semantics;

public class SemanticAnalyzer : IAstVisitor
{
    public void Analyze(ProgramNode program)
    {
        program.Accept(this);
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

            if (argument.ResultType != DataType.Int &&
                argument.ResultType != DataType.Float &&
                argument.ResultType != DataType.String)
            {
                throw new Exception("Output supports only int, float and string.");
            }
        }
    }

    public void VisitIntLiteralNode(IntLiteralNode intLiteralNode)
    {
        intLiteralNode.ResultType = DataType.Int;
    }

    public void VisitFloatLiteralNode(FloatLiteralNode floatLiteralNode)
    {
        floatLiteralNode.ResultType = DataType.Float;
    }

    public void VisitStringLiteralNode(StringLiteralNode stringLiteralNode)
    {
        stringLiteralNode.ResultType = DataType.String;
    }
}