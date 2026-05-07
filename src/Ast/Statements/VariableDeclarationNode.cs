namespace Ast;

public class VariableDeclarationNode : StatementNode
{
    public VariableDeclarationNode(
        DataType type,
        string name,
        ExpressionNode? initializer)
    {
        Type = type;
        Name = name;
        Initializer = initializer;
    }

    public DataType Type { get; }

    public string Name { get; }

    public ExpressionNode? Initializer { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitVariableDeclarationNode(this);
    }
}