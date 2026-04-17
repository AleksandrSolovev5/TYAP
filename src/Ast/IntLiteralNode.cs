namespace Ast;

public class IntLiteralNode : ExpressionNode
{
    public IntLiteralNode(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitIntLiteralNode(this);
    }
}