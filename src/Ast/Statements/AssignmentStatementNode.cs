namespace Ast;

public class AssignmentStatementNode : StatementNode
{
    public AssignmentStatementNode(string name, ExpressionNode value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; }

    public ExpressionNode Value { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitAssignmentStatementNode(this);
    }
}