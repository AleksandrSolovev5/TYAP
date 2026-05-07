namespace Ast;

public class FloatLiteralNode : ExpressionNode
{
    public FloatLiteralNode(double value)
    {
        Value = value;
    }

    public double Value { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitFloatLiteralNode(this);
    }
}