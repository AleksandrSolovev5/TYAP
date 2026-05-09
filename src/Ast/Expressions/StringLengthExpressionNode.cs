namespace Ast;

public class StringLengthExpressionNode : ExpressionNode
{
    public StringLengthExpressionNode(ExpressionNode value)
    {
        Value = value;
    }

    public ExpressionNode Value { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitStringLengthExpressionNode(this);
    }
}