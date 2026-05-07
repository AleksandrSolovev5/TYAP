namespace Ast;

public class UnaryExpressionNode : ExpressionNode
{
    public UnaryExpressionNode(UnaryOperator operatorType, ExpressionNode operand)
    {
        OperatorType = operatorType;
        Operand = operand;
    }

    public UnaryOperator OperatorType { get; }

    public ExpressionNode Operand { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.VisitUnaryExpressionNode(this);
    }
}