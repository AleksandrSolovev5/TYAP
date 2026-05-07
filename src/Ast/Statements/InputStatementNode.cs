namespace Ast;

public class InputStatementNode : StatementNode
{
    public InputStatementNode(List<string> names)
    {
        Names = names;
    }

    public List<string> Names { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitInputStatementNode(this);
    }
}