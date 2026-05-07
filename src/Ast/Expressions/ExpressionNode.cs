namespace Ast;

public abstract class ExpressionNode : AstNode
{
    public DataType? ResultType { get; set; }
}