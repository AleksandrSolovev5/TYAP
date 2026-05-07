namespace Ast;

public class IdentifierExpressionNode : ExpressionNode
{
    public IdentifierExpressionNode(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitIdentifierExpressionNode(this);
    }
}