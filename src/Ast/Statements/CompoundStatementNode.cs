namespace Ast;

public class CompoundStatementNode : StatementNode
{
    public CompoundStatementNode(List<StatementNode> statements)
    {
        Statements = statements;
    }

    public List<StatementNode> Statements { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitCompoundStatementNode(this);
    }
}