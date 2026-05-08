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
        Consume(TokenType.LeftRoundBracket);
        Consume(TokenType.RightRoundBracket);

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
            TokenType.Int or TokenType.Float or TokenType.String => ParseVariableDeclarations(),

            TokenType.Const => [ParseConstantDefinition()],

            TokenType.Identifier => [ParseAssignmentStatement()],

            TokenType.Input => [ParseInputStatement()],

            TokenType.Output => [ParseOutputStatement()],

            TokenType.LeftSquareBracket => [ParseCompoundStatement()],

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
        Consume(TokenType.LeftRoundBracket);

        List<string> names = [];

        names.Add(_currentToken.Text);
        Consume(TokenType.Identifier);

        while (_currentToken.Type == TokenType.Comma)
        {
            Consume(TokenType.Comma);

            names.Add(_currentToken.Text);
            Consume(TokenType.Identifier);
        }

        Consume(TokenType.RightRoundBracket);

        return new InputStatementNode(names);
    }

    private OutputStatementNode ParseOutputStatement()
    {
        Consume(TokenType.Output);
        Consume(TokenType.LeftRoundBracket);

        List<ExpressionNode> arguments = [];

        if (_currentToken.Type != TokenType.RightRoundBracket)
        {
            arguments = ParseExpressionList();
        }

        Consume(TokenType.RightRoundBracket);

        return new OutputStatementNode(arguments);
    }

    private CompoundStatementNode ParseCompoundStatement()
    {
        Consume(TokenType.LeftSquareBracket);

        List<StatementNode> statements = ParseStatementList(TokenType.RightSquareBracket);

        Consume(TokenType.RightSquareBracket);

        return new CompoundStatementNode(statements);
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
        return ParseAdditiveExpression();
    }

    private ExpressionNode ParseAdditiveExpression()
    {
        ExpressionNode left = ParseMultiplicativeExpression();

        while (_currentToken.Type == TokenType.Plus ||
               _currentToken.Type == TokenType.Minus)
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

        while (_currentToken.Type == TokenType.Star ||
               _currentToken.Type == TokenType.Slash ||
               _currentToken.Type == TokenType.Percent)
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

        return ParsePrimaryExpression();
    }

    private ExpressionNode ParsePrimaryExpression()
    {
        if (_currentToken.Type == TokenType.IntLiteral)
        {
            string text = _currentToken.Text;
            Consume(TokenType.IntLiteral);

            return new IntLiteralNode(int.Parse(text, CultureInfo.InvariantCulture));
        }

        if (_currentToken.Type == TokenType.FloatLiteral)
        {
            string text = _currentToken.Text;
            Consume(TokenType.FloatLiteral);

            return new FloatLiteralNode(double.Parse(text, CultureInfo.InvariantCulture));
        }

        if (_currentToken.Type == TokenType.StringLiteral)
        {
            string text = _currentToken.Text;
            Consume(TokenType.StringLiteral);

            return new StringLiteralNode(text);
        }

        if (_currentToken.Type == TokenType.Identifier)
        {
            string name = _currentToken.Text;
            Consume(TokenType.Identifier);

            return new IdentifierExpressionNode(name);
        }

        if (_currentToken.Type == TokenType.LeftRoundBracket)
        {
            Consume(TokenType.LeftRoundBracket);

            ExpressionNode expression = ParseExpression();

            Consume(TokenType.RightRoundBracket);

            return expression;
        }

        throw new Exception("Expected expression, but found: " + _currentToken.Text);
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