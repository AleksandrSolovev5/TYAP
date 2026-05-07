namespace Ast;

public class ConstantDefinitionNode : StatementNode
{
    public ConstantDefinitionNode(
        DataType type,
        string name,
        ExpressionNode value)
    {
        Type = type;
        Name = name;
        Value = value;
    }

    public DataType Type { get; }

    public string Name { get; }

    public ExpressionNode Value { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitConstantDefinitionNode(this);
    }
}