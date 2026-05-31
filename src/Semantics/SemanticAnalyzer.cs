using Ast;

namespace Semantics;

public class SemanticAnalyzer : IAstVisitor
{
    private readonly Stack<Dictionary<string, SymbolInfo>> _scopes = new();

    public void Analyze(ProgramNode program)
    {
        program.Accept(this);
    }

    public void VisitProgramNode(ProgramNode programNode)
    {
        EnterScope();

        foreach (StatementNode statement in programNode.Statements)
        {
            statement.Accept(this);
        }

        ExitScope();
    }

    public void VisitCompoundStatementNode(CompoundStatementNode compoundStatementNode)
    {
        EnterScope();

        foreach (StatementNode statement in compoundStatementNode.Statements)
        {
            statement.Accept(this);
        }

        ExitScope();
    }

    public void VisitVariableDeclarationNode(VariableDeclarationNode variableDeclarationNode)
    {
        if (IsDeclaredInCurrentScope(variableDeclarationNode.Name))
        {
            throw new Exception("Variable '" + variableDeclarationNode.Name + "' is already declared in this scope.");
        }

        if (variableDeclarationNode.Initializer is not null)
        {
            variableDeclarationNode.Initializer.Accept(this);

            if (variableDeclarationNode.Initializer.ResultType != variableDeclarationNode.Type)
            {
                throw new Exception(
                    "Cannot initialize variable '" + variableDeclarationNode.Name +
                    "' of type " + variableDeclarationNode.Type +
                    " with expression of type " + variableDeclarationNode.Initializer.ResultType + ".");
            }
        }

        Declare(variableDeclarationNode.Name, variableDeclarationNode.Type, isConstant: false);
    }

    public void VisitConstantDefinitionNode(ConstantDefinitionNode constantDefinitionNode)
    {
        if (IsDeclaredInCurrentScope(constantDefinitionNode.Name))
        {
            throw new Exception("Constant '" + constantDefinitionNode.Name + "' is already declared in this scope.");
        }

        constantDefinitionNode.Value.Accept(this);

        if (constantDefinitionNode.Value.ResultType != constantDefinitionNode.Type)
        {
            throw new Exception(
                "Cannot initialize constant '" + constantDefinitionNode.Name +
                "' of type " + constantDefinitionNode.Type +
                " with expression of type " + constantDefinitionNode.Value.ResultType + ".");
        }

        Declare(constantDefinitionNode.Name, constantDefinitionNode.Type, isConstant: true);
    }

    public void VisitAssignmentStatementNode(AssignmentStatementNode assignmentStatementNode)
    {
        SymbolInfo symbol = FindSymbolOrThrow(assignmentStatementNode.Name);

        if (symbol.IsConstant)
        {
            throw new Exception("Cannot assign value to constant '" + assignmentStatementNode.Name + "'.");
        }

        assignmentStatementNode.Value.Accept(this);

        if (assignmentStatementNode.Value.ResultType != symbol.Type)
        {
            throw new Exception(
                "Cannot assign expression of type " + assignmentStatementNode.Value.ResultType +
                " to variable '" + assignmentStatementNode.Name +
                "' of type " + symbol.Type + ".");
        }
    }

    public void VisitInputStatementNode(InputStatementNode inputStatementNode)
    {
        foreach (string name in inputStatementNode.Names)
        {
            SymbolInfo symbol = FindSymbolOrThrow(name);

            if (symbol.IsConstant)
            {
                throw new Exception("Cannot input value into constant '" + name + "'.");
            }
        }
    }

    public void VisitOutputStatementNode(OutputStatementNode outputStatementNode)
    {
        foreach (ExpressionNode argument in outputStatementNode.Arguments)
        {
            argument.Accept(this);

            if (argument.ResultType != DataType.Int &&
                argument.ResultType != DataType.Float &&
                argument.ResultType != DataType.String &&
                argument.ResultType != DataType.Bool)
            {
                throw new Exception("Output supports only int, float, string and bool.");
            }
        }
    }

    public void VisitIdentifierExpressionNode(IdentifierExpressionNode identifierExpressionNode)
    {
        SymbolInfo symbol = FindSymbolOrThrow(identifierExpressionNode.Name);

        identifierExpressionNode.ResultType = symbol.Type;
    }

    public void VisitBinaryExpressionNode(BinaryExpressionNode binaryExpressionNode)
    {
        binaryExpressionNode.Left.Accept(this);
        binaryExpressionNode.Right.Accept(this);

        DataType? leftType = binaryExpressionNode.Left.ResultType;
        DataType? rightType = binaryExpressionNode.Right.ResultType;

        if (leftType != rightType)
        {
            throw new Exception(
                "Binary operator '" + binaryExpressionNode.OperatorType +
                "' cannot be applied to operands of types " +
                leftType + " and " + rightType + ".");
        }

        BinaryOperator op = binaryExpressionNode.OperatorType;

        bool isComparison = op == BinaryOperator.Less ||
                            op == BinaryOperator.Greater ||
                            op == BinaryOperator.LessEqual ||
                            op == BinaryOperator.GreaterEqual;

        bool isEquality = op == BinaryOperator.Equal ||
                          op == BinaryOperator.NotEqual;

        bool isArithmetic = op == BinaryOperator.Add ||
                            op == BinaryOperator.Subtract ||
                            op == BinaryOperator.Multiply ||
                            op == BinaryOperator.Divide ||
                            op == BinaryOperator.Mod;

        if (isComparison)
        {
            if (leftType != DataType.Int && leftType != DataType.Float)
            {
                throw new Exception(
                    "Comparison operator '" + op +
                    "' can only be applied to int or float operands.");
            }

            binaryExpressionNode.ResultType = DataType.Bool;
            return;
        }

        if (isEquality)
        {
            if (leftType != DataType.Int &&
                leftType != DataType.Float &&
                leftType != DataType.String &&
                leftType != DataType.Bool)
            {
                throw new Exception(
                    "Equality operator '" + op +
                    "' cannot be applied to operands of type " + leftType + ".");
            }

            binaryExpressionNode.ResultType = DataType.Bool;
            return;
        }

        if (isArithmetic)
        {
            if (leftType == DataType.Bool)
            {
                throw new Exception(
                    "Arithmetic operator '" + op +
                    "' cannot be applied to bool operands.");
            }

            if (leftType == DataType.Int)
            {
                binaryExpressionNode.ResultType = DataType.Int;
                return;
            }

            if (leftType == DataType.Float)
            {
                if (op == BinaryOperator.Mod)
                {
                    throw new Exception("Operator '%' cannot be applied to float operands.");
                }

                binaryExpressionNode.ResultType = DataType.Float;
                return;
            }

            if (leftType == DataType.String)
            {
                if (op != BinaryOperator.Add)
                {
                    throw new Exception(
                        "Operator '" + op + "' cannot be applied to string operands.");
                }

                binaryExpressionNode.ResultType = DataType.String;
                return;
            }
        }

        throw new Exception("Unsupported binary expression type.");
    }

    public void VisitUnaryExpressionNode(UnaryExpressionNode unaryExpressionNode)
    {
        unaryExpressionNode.Operand.Accept(this);

        if (unaryExpressionNode.OperatorType == UnaryOperator.Not)
        {
            if (unaryExpressionNode.Operand.ResultType != DataType.Bool)
            {
                throw new Exception(
                    "Unary operator '!' cannot be applied to expression of type " +
                    unaryExpressionNode.Operand.ResultType + ".");
            }

            unaryExpressionNode.ResultType = DataType.Bool;
            return;
        }

        if (unaryExpressionNode.Operand.ResultType != DataType.Int &&
            unaryExpressionNode.Operand.ResultType != DataType.Float)
        {
            throw new Exception(
                "Unary operator '" + unaryExpressionNode.OperatorType +
                "' cannot be applied to expression of type " + unaryExpressionNode.Operand.ResultType + ".");
        }

        unaryExpressionNode.ResultType = unaryExpressionNode.Operand.ResultType;
    }

    public void VisitStringLengthExpressionNode(StringLengthExpressionNode stringLengthExpressionNode)
    {
        stringLengthExpressionNode.Value.Accept(this);

        if (stringLengthExpressionNode.Value.ResultType != DataType.String)
        {
            throw new Exception(
                "Function 'len' expects string argument, but got " + stringLengthExpressionNode.Value.ResultType);
        }

        stringLengthExpressionNode.ResultType = DataType.Int;
    }

    public void VisitStringIndexExpressionNode(StringIndexExpressionNode stringIndexExpressionNode)
    {
        stringIndexExpressionNode.Value.Accept(this);
        stringIndexExpressionNode.Index.Accept(this);

        if (stringIndexExpressionNode.Value.ResultType != DataType.String)
        {
            throw new Exception(
                "String index operator expects string value, but got " + stringIndexExpressionNode.Value.ResultType);
        }

        if (stringIndexExpressionNode.Index.ResultType != DataType.Int)
        {
            throw new Exception(
                "String index must be int, but got " + stringIndexExpressionNode.Index.ResultType + ".");
        }

        stringIndexExpressionNode.ResultType = DataType.String;
    }

    public void VisitIntLiteralNode(IntLiteralNode intLiteralNode)
    {
        intLiteralNode.ResultType = DataType.Int;
    }

    public void VisitFloatLiteralNode(FloatLiteralNode floatLiteralNode)
    {
        floatLiteralNode.ResultType = DataType.Float;
    }

    public void VisitStringLiteralNode(StringLiteralNode stringLiteralNode)
    {
        stringLiteralNode.ResultType = DataType.String;
    }

    public void VisitBoolLiteralNode(BoolLiteralNode boolLiteralNode)
    {
        boolLiteralNode.ResultType = DataType.Bool;
    }

    public void VisitIfStatementNode(IfStatementNode ifStatementNode)
    {
        ifStatementNode.Condition.Accept(this);

        if (ifStatementNode.Condition.ResultType != DataType.Bool)
        {
            throw new Exception(
                "Condition of 'if' statement must be bool, but got " +
                ifStatementNode.Condition.ResultType + ".");
        }

        ifStatementNode.ThenBranch.Accept(this);

        if (ifStatementNode.ElseBranch is not null)
        {
            ifStatementNode.ElseBranch.Accept(this);
        }
    }

    private void EnterScope()
    {
        _scopes.Push([]);
    }

    private void ExitScope()
    {
        _scopes.Pop();
    }

    private void Declare(string name, DataType type, bool isConstant)
    {
        _scopes.Peek().Add(name, new SymbolInfo(type, isConstant));
    }

    private bool IsDeclaredInCurrentScope(string name)
    {
        return _scopes.Peek().ContainsKey(name);
    }

    private SymbolInfo FindSymbolOrThrow(string name)
    {
        foreach (Dictionary<string, SymbolInfo> scope in _scopes)
        {
            if (scope.TryGetValue(name, out SymbolInfo? symbol))
            {
                return symbol;
            }
        }

        throw new Exception("Variable '" + name + "' is not declared.");
    }

    private class SymbolInfo(DataType type, bool isConstant)
    {
        public DataType Type { get; } = type;

        public bool IsConstant { get; } = isConstant;
    }
}