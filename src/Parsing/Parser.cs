using System.Globalization;

using Ast;

using Lexing;

namespace Parsing;

public class Parser
{
    private readonly Lexer _lexer;
    private Token _currentToken;
    private Token _nextToken;

    public Parser(Lexer lexer)
    {
        _lexer = lexer;
        _currentToken = _lexer.NextToken();
        _nextToken = _lexer.NextToken();
    }

    public ProgramNode ParseProgram()
    {
        ProgramNode program = ParseMainFunction();

        Consume(TokenType.EndOfFile);

        return program;
    }

    private ProgramNode ParseMainFunction()
    {
        Consume(TokenType.Int);

        if (_currentToken.Type != TokenType.Identifier || _currentToken.Text != "main")
        {
            throw new Exception("Expected main function.");
        }

        Consume(TokenType.Identifier);
        Consume(TokenType.LeftParenthesis);
        Consume(TokenType.RightParenthesis);

        CompoundStatementNode body = ParseCompoundStatement();

        return new ProgramNode(body.Statements);
    }

    private List<StatementNode> ParseStatementList(TokenType endToken)
    {
        List<StatementNode> statements = [];

        while (_currentToken.Type != endToken && _currentToken.Type != TokenType.EndOfFile)
        {
            statements.AddRange(ParseStatement());

            if (_currentToken.Type == TokenType.Semicolon)
            {
                Consume(TokenType.Semicolon);
            }
            else if (_currentToken.Type != endToken && _currentToken.Type != TokenType.EndOfFile)
            {
                throw new Exception("Expected semicolon, but found: " + _currentToken.Text);
            }
        }

        return statements;
    }

    private List<StatementNode> ParseStatement()
    {
        return _currentToken.Type switch
        {
            TokenType.Int or TokenType.Float or TokenType.String or TokenType.Bool => ParseVariableDeclarations(),

            TokenType.Const => [ParseConstantDefinition()],

            TokenType.Identifier => [ParseAssignmentStatement()],

            TokenType.Input => [ParseInputStatement()],

            TokenType.Output => [ParseOutputStatement()],

            TokenType.LeftBracket => [ParseCompoundStatement()],

            TokenType.If => [ParseIfStatement()],

            _ => throw new Exception("Expected statement, but found: " + _currentToken.Text),
        };
    }

    private List<StatementNode> ParseVariableDeclarations()
    {
        List<StatementNode> declarations = [];

        while (true)
        {
            DataType type = ParseType();

            string name = _currentToken.Text;
            Consume(TokenType.Identifier);

            ExpressionNode? initializer = null;

            if (_currentToken.Type == TokenType.Equal)
            {
                Consume(TokenType.Equal);
                initializer = ParseExpression();
            }

            declarations.Add(new VariableDeclarationNode(type, name, initializer));

            if (_currentToken.Type != TokenType.Comma)
            {
                break;
            }

            Consume(TokenType.Comma);
        }

        return declarations;
    }

    private ConstantDefinitionNode ParseConstantDefinition()
    {
        Consume(TokenType.Const);

        DataType type = ParseType();

        string name = _currentToken.Text;
        Consume(TokenType.Identifier);

        Consume(TokenType.Equal);

        ExpressionNode value = ParseExpression();

        return new ConstantDefinitionNode(type, name, value);
    }

    private AssignmentStatementNode ParseAssignmentStatement()
    {
        string name = _currentToken.Text;

        Consume(TokenType.Identifier);
        Consume(TokenType.Equal);

        ExpressionNode value = ParseExpression();

        return new AssignmentStatementNode(name, value);
    }

    private InputStatementNode ParseInputStatement()
    {
        Consume(TokenType.Input);
        Consume(TokenType.LeftParenthesis);

        List<string> names = [];

        names.Add(_currentToken.Text);
        Consume(TokenType.Identifier);

        while (_currentToken.Type == TokenType.Comma)
        {
            Consume(TokenType.Comma);

            names.Add(_currentToken.Text);
            Consume(TokenType.Identifier);
        }

        Consume(TokenType.RightParenthesis);

        return new InputStatementNode(names);
    }

    private OutputStatementNode ParseOutputStatement()
    {
        Consume(TokenType.Output);
        Consume(TokenType.LeftParenthesis);

        List<ExpressionNode> arguments = [];

        if (_currentToken.Type != TokenType.RightParenthesis)
        {
            arguments = ParseExpressionList();
        }

        Consume(TokenType.RightParenthesis);

        return new OutputStatementNode(arguments);
    }

    private CompoundStatementNode ParseCompoundStatement()
    {
        Consume(TokenType.LeftBracket);

        List<StatementNode> statements = ParseStatementList(TokenType.RightBracket);

        Consume(TokenType.RightBracket);

        return new CompoundStatementNode(statements);
    }

    private IfStatementNode ParseIfStatement()
    {
        Consume(TokenType.If);
        Consume(TokenType.LeftParenthesis);

        ExpressionNode condition = ParseExpression();

        Consume(TokenType.RightParenthesis);

        List<StatementNode> thenStatements = ParseStatement();
        StatementNode thenBranch;

        if (thenStatements.Count == 1)
        {
            thenBranch = thenStatements[0];
        }
        else
        {
            thenBranch = new CompoundStatementNode(thenStatements);
        }

        StatementNode? elseBranch = null;

        if (_currentToken.Type == TokenType.Else)
        {
            Consume(TokenType.Else);

            List<StatementNode> elseStatements = ParseStatement();

            if (elseStatements.Count == 1)
            {
                elseBranch = elseStatements[0];
            }
            else
            {
                elseBranch = new CompoundStatementNode(elseStatements);
            }
        }

        return new IfStatementNode(condition, thenBranch, elseBranch);
    }

    private List<ExpressionNode> ParseExpressionList()
    {
        List<ExpressionNode> expressions = [];

        expressions.Add(ParseExpression());

        while (_currentToken.Type == TokenType.Comma)
        {
            Consume(TokenType.Comma);
            expressions.Add(ParseExpression());
        }

        return expressions;
    }

    private ExpressionNode ParseExpression()
    {
        return ParseComparisonExpression();
    }

    private ExpressionNode ParseComparisonExpression()
    {
        ExpressionNode left = ParseAdditiveExpression();

        while (_currentToken.Type is TokenType.Less or TokenType.Greater or TokenType.EqualEqual or TokenType.NotEqual or TokenType.LessEqual or TokenType.GreaterEqual)
        {
            TokenType operatorToken = _currentToken.Type;

            Consume(operatorToken);

            ExpressionNode right = ParseAdditiveExpression();

            BinaryOperator binaryOperator = operatorToken switch
            {
                TokenType.Less => BinaryOperator.Less,
                TokenType.Greater => BinaryOperator.Greater,
                TokenType.EqualEqual => BinaryOperator.Equal,
                TokenType.NotEqual => BinaryOperator.NotEqual,
                TokenType.LessEqual => BinaryOperator.LessEqual,
                TokenType.GreaterEqual => BinaryOperator.GreaterEqual,
                _ => throw new Exception("Unknown comparison operator."),
            };

            left = new BinaryExpressionNode(left, binaryOperator, right);
        }

        return left;
    }

    private ExpressionNode ParseAdditiveExpression()
    {
        ExpressionNode left = ParseMultiplicativeExpression();

        while (_currentToken.Type is TokenType.Plus or TokenType.Minus)
        {
            TokenType operatorToken = _currentToken.Type;

            Consume(operatorToken);

            ExpressionNode right = ParseMultiplicativeExpression();

            BinaryOperator binaryOperator = operatorToken switch
            {
                TokenType.Plus => BinaryOperator.Add,
                TokenType.Minus => BinaryOperator.Subtract,
                _ => throw new Exception("Unknown additive operator."),
            };

            left = new BinaryExpressionNode(left, binaryOperator, right);
        }

        return left;
    }

    private ExpressionNode ParseMultiplicativeExpression()
    {
        ExpressionNode left = ParseUnaryExpression();

        while (_currentToken.Type is TokenType.Star or TokenType.Slash or TokenType.Percent)
        {
            TokenType operatorToken = _currentToken.Type;

            Consume(operatorToken);

            ExpressionNode right = ParseUnaryExpression();

            BinaryOperator binaryOperator = operatorToken switch
            {
                TokenType.Star => BinaryOperator.Multiply,
                TokenType.Slash => BinaryOperator.Divide,
                TokenType.Percent => BinaryOperator.Mod,
                _ => throw new Exception("Unknown multiplicative operator."),
            };

            left = new BinaryExpressionNode(left, binaryOperator, right);
        }

        return left;
    }

    private ExpressionNode ParseUnaryExpression()
    {
        if (_currentToken.Type == TokenType.Plus)
        {
            Consume(TokenType.Plus);

            ExpressionNode operand = ParseUnaryExpression();

            return new UnaryExpressionNode(UnaryOperator.Plus, operand);
        }

        if (_currentToken.Type == TokenType.Minus)
        {
            Consume(TokenType.Minus);

            ExpressionNode operand = ParseUnaryExpression();

            return new UnaryExpressionNode(UnaryOperator.Minus, operand);
        }

        if (_currentToken.Type == TokenType.Exclamation)
        {
            Consume(TokenType.Exclamation);

            ExpressionNode operand = ParseUnaryExpression();

            return new UnaryExpressionNode(UnaryOperator.Not, operand);
        }

        return ParsePrimaryExpression();
    }

    private ExpressionNode ParsePrimaryExpression()
    {
        ExpressionNode expression;

        switch (_currentToken.Type)
        {
            case TokenType.IntLiteral:
                expression = new IntLiteralNode(int.Parse(_currentToken.Text, CultureInfo.InvariantCulture));
                Consume(TokenType.IntLiteral);
                break;

            case TokenType.FloatLiteral:
                expression = new FloatLiteralNode(double.Parse(_currentToken.Text, CultureInfo.InvariantCulture));
                Consume(TokenType.FloatLiteral);
                break;

            case TokenType.StringLiteral:
                expression = new StringLiteralNode(_currentToken.Text);
                Consume(TokenType.StringLiteral);
                break;

            case TokenType.BoolLiteral:
                expression = new BoolLiteralNode(_currentToken.Text == "true");
                Consume(TokenType.BoolLiteral);
                break;

            case TokenType.Len:
                Consume(TokenType.Len);
                Consume(TokenType.LeftParenthesis);
                ExpressionNode value = ParseExpression();
                Consume(TokenType.RightParenthesis);
                expression = new StringLengthExpressionNode(value);
                break;

            case TokenType.Identifier:
                expression = new IdentifierExpressionNode(_currentToken.Text);
                Consume(TokenType.Identifier);
                break;

            case TokenType.LeftParenthesis:
                Consume(TokenType.LeftParenthesis);
                expression = ParseExpression();
                Consume(TokenType.RightParenthesis);
                break;

            default:
                throw new Exception("Expected expression, but found: " + _currentToken.Text);
        }

        return ParsePostfixExpression(expression);
    }

    private ExpressionNode ParsePostfixExpression(ExpressionNode expression)
    {
        while (_currentToken.Type == TokenType.LeftBracket)
        {
            Consume(TokenType.LeftBracket);

            ExpressionNode index = ParseExpression();

            Consume(TokenType.RightBracket);

            expression = new StringIndexExpressionNode(expression, index);
        }

        return expression;
    }

    private DataType ParseType()
    {
        switch (_currentToken.Type)
        {
            case TokenType.Int:
                Consume(TokenType.Int);
                return DataType.Int;

            case TokenType.Float:
                Consume(TokenType.Float);
                return DataType.Float;

            case TokenType.String:
                Consume(TokenType.String);
                return DataType.String;

            case TokenType.Bool:
                Consume(TokenType.Bool);
                return DataType.Bool;

            default:
                throw new Exception("Expected type, but found: " + _currentToken.Text);
        }
    }

    private void Consume(TokenType expectedType)
    {
        if (_currentToken.Type != expectedType)
        {
            throw new Exception(
                "Expected token " + expectedType + ", but found " + _currentToken.Type + " with text '" + _currentToken.Text + "'.");
        }

        MoveNext();
    }

    private void MoveNext()
    {
        _currentToken = _nextToken;
        _nextToken = _lexer.NextToken();
    }
}