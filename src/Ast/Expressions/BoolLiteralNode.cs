namespace Ast;

public class BoolLiteralNode : ExpressionNode
{
    public BoolLiteralNode(bool value)
    {
        Value = value;
    }

    public bool Value { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitBoolLiteralNode(this);
    }
}