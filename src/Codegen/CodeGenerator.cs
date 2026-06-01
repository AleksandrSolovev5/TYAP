using Ast;

using Bytecode;

namespace Codegen;

public class CodeGenerator : IAstVisitor
{
    private List<Instruction> _instructions = [];

    public List<Instruction> Generate(ProgramNode programNode)
    {
        _instructions = [];

        programNode.Accept(this);

        _instructions.Add(new Instruction(InstructionType.Halt));

        return _instructions;
    }

    public void VisitProgramNode(ProgramNode programNode)
    {
        foreach (StatementNode statement in programNode.Statements)
        {
            statement.Accept(this);
        }
    }

    public void VisitCompoundStatementNode(CompoundStatementNode compoundStatementNode)
    {
        _instructions.Add(new Instruction(InstructionType.EnterScope));

        foreach (StatementNode statement in compoundStatementNode.Statements)
        {
            statement.Accept(this);
        }

        _instructions.Add(new Instruction(InstructionType.ExitScope));
    }

    public void VisitVariableDeclarationNode(VariableDeclarationNode variableDeclarationNode)
    {
        if (variableDeclarationNode.Initializer is not null)
        {
            variableDeclarationNode.Initializer.Accept(this);
        }
        else
        {
            PushDefaultValue(variableDeclarationNode.Type);
        }

        VariableDefinition variableDefinition = new(
            variableDeclarationNode.Name,
            ToBytecodeValueType(variableDeclarationNode.Type),
            isConstant: false);

        _instructions.Add(new Instruction(
            InstructionType.DefineVariable,
            variableDefinition));
    }

    public void VisitConstantDefinitionNode(ConstantDefinitionNode constantDefinitionNode)
    {
        constantDefinitionNode.Value.Accept(this);

        VariableDefinition variableDefinition = new(
            constantDefinitionNode.Name,
            ToBytecodeValueType(constantDefinitionNode.Type),
            isConstant: true);

        _instructions.Add(new Instruction(
            InstructionType.DefineVariable,
            variableDefinition));
    }

    public void VisitAssignmentStatementNode(AssignmentStatementNode assignmentStatementNode)
    {
        assignmentStatementNode.Value.Accept(this);

        _instructions.Add(new Instruction(
            InstructionType.StoreVariable,
            assignmentStatementNode.Name));
    }

    public void VisitInputStatementNode(InputStatementNode inputStatementNode)
    {
        foreach (string name in inputStatementNode.Names)
        {
            _instructions.Add(new Instruction(
                InstructionType.Input,
                name));
        }
    }

    public void VisitOutputStatementNode(OutputStatementNode outputStatementNode)
    {
        foreach (ExpressionNode argument in outputStatementNode.Arguments)
        {
            argument.Accept(this);

            _instructions.Add(new Instruction(InstructionType.Output));
        }
    }

    public void VisitIdentifierExpressionNode(IdentifierExpressionNode identifierExpressionNode)
    {
        _instructions.Add(new Instruction(
            InstructionType.LoadVariable,
            identifierExpressionNode.Name));
    }

    public void VisitBinaryExpressionNode(BinaryExpressionNode binaryExpressionNode)
    {
        if (binaryExpressionNode.OperatorType == BinaryOperator.And)
        {
            GenerateShortCircuitAnd(binaryExpressionNode);
            return;
        }

        if (binaryExpressionNode.OperatorType == BinaryOperator.Or)
        {
            GenerateShortCircuitOr(binaryExpressionNode);
            return;
        }

        binaryExpressionNode.Left.Accept(this);
        binaryExpressionNode.Right.Accept(this);

        InstructionType instructionType = ToInstructionType(binaryExpressionNode.OperatorType);

        _instructions.Add(new Instruction(instructionType));
    }

    public void VisitUnaryExpressionNode(UnaryExpressionNode unaryExpressionNode)
    {
        unaryExpressionNode.Operand.Accept(this);

        if (unaryExpressionNode.OperatorType == UnaryOperator.Minus)
        {
            _instructions.Add(new Instruction(InstructionType.UnaryMinus));
        }
        else if (unaryExpressionNode.OperatorType == UnaryOperator.Not)
        {
            _instructions.Add(new Instruction(InstructionType.LogicalNot));
        }
    }

    public void VisitIntLiteralNode(IntLiteralNode intLiteralNode)
    {
        _instructions.Add(new Instruction(
            InstructionType.PushInt,
            intLiteralNode.Value));
    }

    public void VisitFloatLiteralNode(FloatLiteralNode floatLiteralNode)
    {
        _instructions.Add(new Instruction(
            InstructionType.PushFloat,
            floatLiteralNode.Value));
    }

    public void VisitStringLiteralNode(StringLiteralNode stringLiteralNode)
    {
        _instructions.Add(new Instruction(
            InstructionType.PushString,
            stringLiteralNode.Value));
    }

    public void VisitStringLengthExpressionNode(StringLengthExpressionNode stringLengthExpressionNode)
    {
        stringLengthExpressionNode.Value.Accept(this);

        _instructions.Add(new Instruction(InstructionType.StringLength));
    }

    public void VisitStringIndexExpressionNode(StringIndexExpressionNode stringIndexExpressionNode)
    {
        stringIndexExpressionNode.Value.Accept(this);
        stringIndexExpressionNode.Index.Accept(this);

        _instructions.Add(new Instruction(InstructionType.StringIndex));
    }

    public void VisitBoolLiteralNode(BoolLiteralNode boolLiteralNode)
    {
        _instructions.Add(new Instruction(
            InstructionType.PushBool,
            boolLiteralNode.Value));
    }

    public void VisitIfStatementNode(IfStatementNode ifStatementNode)
    {
        ifStatementNode.Condition.Accept(this);

        int jumpIfFalseIndex = _instructions.Count;
        _instructions.Add(new Instruction(InstructionType.JumpIfFalse, 0));

        ifStatementNode.ThenBranch.Accept(this);

        if (ifStatementNode.ElseBranch is not null)
        {
            int jumpIndex = _instructions.Count;
            _instructions.Add(new Instruction(InstructionType.Jump, 0));

            int elseStart = _instructions.Count;
            _instructions[jumpIfFalseIndex] = new Instruction(InstructionType.JumpIfFalse, elseStart);

            ifStatementNode.ElseBranch.Accept(this);

            int endIndex = _instructions.Count;
            _instructions[jumpIndex] = new Instruction(InstructionType.Jump, endIndex);
        }
        else
        {
            int endIndex = _instructions.Count;
            _instructions[jumpIfFalseIndex] = new Instruction(InstructionType.JumpIfFalse, endIndex);
        }
    }

    private void GenerateShortCircuitAnd(BinaryExpressionNode node)
    {
        node.Left.Accept(this);

        int jumpIfFalseIndex = _instructions.Count;
        _instructions.Add(new Instruction(InstructionType.JumpIfFalse, 0));

        node.Right.Accept(this);

        int jumpToEndIndex = _instructions.Count;
        _instructions.Add(new Instruction(InstructionType.Jump, 0));

        int falseLabelIndex = _instructions.Count;
        _instructions[jumpIfFalseIndex] = new Instruction(InstructionType.JumpIfFalse, falseLabelIndex);
        _instructions.Add(new Instruction(InstructionType.PushBool, false));

        int endLabelIndex = _instructions.Count;
        _instructions[jumpToEndIndex] = new Instruction(InstructionType.Jump, endLabelIndex);
    }

    private void GenerateShortCircuitOr(BinaryExpressionNode node)
    {
        node.Left.Accept(this);

        _instructions.Add(new Instruction(InstructionType.LogicalNot));

        int jumpIfFalseIndex = _instructions.Count;
        _instructions.Add(new Instruction(InstructionType.JumpIfFalse, 0));

        node.Right.Accept(this);

        int jumpToEndIndex = _instructions.Count;
        _instructions.Add(new Instruction(InstructionType.Jump, 0));

        int trueLabelIndex = _instructions.Count;
        _instructions[jumpIfFalseIndex] = new Instruction(InstructionType.JumpIfFalse, trueLabelIndex);
        _instructions.Add(new Instruction(InstructionType.PushBool, true));

        int endLabelIndex = _instructions.Count;
        _instructions[jumpToEndIndex] = new Instruction(InstructionType.Jump, endLabelIndex);
    }

    private void PushDefaultValue(DataType type)
    {
        switch (type)
        {
            case DataType.Int:
                _instructions.Add(new Instruction(InstructionType.PushInt, 0));
                break;

            case DataType.Float:
                _instructions.Add(new Instruction(InstructionType.PushFloat, 0.0));
                break;

            case DataType.String:
                _instructions.Add(new Instruction(InstructionType.PushString, string.Empty));
                break;

            case DataType.Bool:
                _instructions.Add(new Instruction(InstructionType.PushBool, false));
                break;

            default:
                throw new Exception("Unknown data type.");
        }
    }

    private static InstructionType ToInstructionType(BinaryOperator operatorType)
    {
        return operatorType switch
        {
            BinaryOperator.Add => InstructionType.Add,
            BinaryOperator.Subtract => InstructionType.Subtract,
            BinaryOperator.Multiply => InstructionType.Multiply,
            BinaryOperator.Divide => InstructionType.Divide,
            BinaryOperator.Mod => InstructionType.Mod,
            BinaryOperator.Less => InstructionType.CompareLess,
            BinaryOperator.Greater => InstructionType.CompareGreater,
            BinaryOperator.Equal => InstructionType.CompareEqual,
            BinaryOperator.NotEqual => InstructionType.CompareNotEqual,
            BinaryOperator.LessEqual => InstructionType.CompareLessEqual,
            BinaryOperator.GreaterEqual => InstructionType.CompareGreaterEqual,
            _ => throw new Exception("Unknown binary operator."),
        };
    }

    private static BytecodeValueType ToBytecodeValueType(DataType type)
    {
        return type switch
        {
            DataType.Int => BytecodeValueType.Int,
            DataType.Float => BytecodeValueType.Float,
            DataType.String => BytecodeValueType.String,
            DataType.Bool => BytecodeValueType.Bool,
            _ => throw new Exception("Unknown data type."),
        };
    }
}