namespace Ast;

public interface IAstVisitor
{
    void VisitProgramNode(ProgramNode programNode);

    void VisitOutputStatementNode(OutputStatementNode outputStatementNode);

    void VisitIntLiteralNode(IntLiteralNode intLiteralNode);

    void VisitFloatLiteralNode(FloatLiteralNode floatLiteralNode);

    void VisitStringLiteralNode(StringLiteralNode stringLiteralNode);
}