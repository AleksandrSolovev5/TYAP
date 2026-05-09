namespace Ast;

public class StringIndexExpressionNode : ExpressionNode
{
    public StringIndexExpressionNode(ExpressionNode value, ExpressionNode index)
    {
        Value = value;
        Index = index;
    }

    public ExpressionNode Value { get; }

    public ExpressionNode Index { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitStringIndexExpressionNode(this);
    }
}