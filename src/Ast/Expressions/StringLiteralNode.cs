namespace Ast;

public class StringLiteralNode : ExpressionNode
{
    public StringLiteralNode(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitStringLiteralNode(this);
    }
}