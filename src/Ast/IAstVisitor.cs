namespace Ast;

public interface IAstVisitor
{
    void VisitProgramNode(ProgramNode programNode);

    void VisitCompoundStatementNode(CompoundStatementNode compoundStatementNode);

    void VisitVariableDeclarationNode(VariableDeclarationNode variableDeclarationNode);

    void VisitConstantDefinitionNode(ConstantDefinitionNode constantDefinitionNode);

    void VisitAssignmentStatementNode(AssignmentStatementNode assignmentStatementNode);

    void VisitInputStatementNode(InputStatementNode inputStatementNode);

    void VisitOutputStatementNode(OutputStatementNode outputStatementNode);

    void VisitIdentifierExpressionNode(IdentifierExpressionNode identifierExpressionNode);

    void VisitBinaryExpressionNode(BinaryExpressionNode binaryExpressionNode);

    void VisitUnaryExpressionNode(UnaryExpressionNode unaryExpressionNode);

    void VisitIntLiteralNode(IntLiteralNode intLiteralNode);

    void VisitFloatLiteralNode(FloatLiteralNode floatLiteralNode);

    void VisitStringLiteralNode(StringLiteralNode stringLiteralNode);

    void VisitStringLengthExpressionNode(StringLengthExpressionNode stringLengthExpressionNode);

    void VisitStringIndexExpressionNode(StringIndexExpressionNode stringIndexExpressionNode);

    void VisitBoolLiteralNode(BoolLiteralNode boolLiteralNode);

    void VisitIfStatementNode(IfStatementNode ifStatementNode);
}