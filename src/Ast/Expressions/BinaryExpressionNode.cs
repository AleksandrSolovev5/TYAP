namespace Ast;

public class BinaryExpressionNode : ExpressionNode
{
    public BinaryExpressionNode(
        ExpressionNode left,
        BinaryOperator operatorType,
        ExpressionNode right)
    {
        Left = left;
        OperatorType = operatorType;
        Right = right;
    }

    public ExpressionNode Left { get; }

    public BinaryOperator OperatorType { get; }

    public ExpressionNode Right { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitBinaryExpressionNode(this);
    }
}