namespace Ast;

public class IfStatementNode : StatementNode
{
    public IfStatementNode(
        ExpressionNode condition,
        StatementNode thenBranch,
        StatementNode? elseBranch)
    {
        Condition = condition;
        ThenBranch = thenBranch;
        ElseBranch = elseBranch;
    }

    public ExpressionNode Condition { get; }

    public StatementNode ThenBranch { get; }

    public StatementNode? ElseBranch { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitIfStatementNode(this);
    }
}