namespace Interpreter.Ast;

public class ProgramNode : AstNode
{
    public ProgramNode(List<StatementNode> statements)
    {
        Statements = statements;
    }

    public List<StatementNode> Statements { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitProgramNode(this);
    }
}