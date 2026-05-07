namespace Ast;

public class OutputStatementNode : StatementNode
{
    public OutputStatementNode(List<ExpressionNode> arguments)
    {
        Arguments = arguments;
    }

    public List<ExpressionNode> Arguments { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitOutputStatementNode(this);
    }
}