using System.Globalization;

using Ast;
using Lexing;

namespace Parsing;

public class Parser
{
    private readonly Lexer _lexer;
    private Token _currentToken;

    public Parser(Lexer lexer)
    {
        _lexer = lexer;
        _currentToken = _lexer.NextToken();
    }

    public ProgramNode ParseProgram()
    {
        List<StatementNode> statements = new();

        statements.Add(ParseStatement());

        while (_currentToken.Type == TokenType.Semicolon)
        {
            Consume(TokenType.Semicolon);

            if (_currentToken.Type == TokenType.EndOfFile)
            {
                break;
            }

            statements.Add(ParseStatement());
        }

        if (_currentToken.Type != TokenType.EndOfFile)
        {
            throw new Exception("Unexpected token after program end: " + _currentToken.Text);
        }

        return new ProgramNode(statements);
    }

    private StatementNode ParseStatement() // пока только output
    {
        if (_currentToken.Type == TokenType.Output)
        {
            return ParseOutputStatement();
        }

        throw new Exception("Expected statement, but found: " + _currentToken.Text);
    }

    private OutputStatementNode ParseOutputStatement()
    {
        Consume(TokenType.Output);
        Consume(TokenType.LeftRoundBracket);

        List<ExpressionNode> arguments = new();

        if (_currentToken.Type != TokenType.RightRoundBracket)
        {
            arguments = ParseExpressionList();
        }

        Consume(TokenType.RightRoundBracket);

        return new OutputStatementNode(arguments);
    }

    private List<ExpressionNode> ParseExpressionList()
    {
        List<ExpressionNode> expressions = new();

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
        return ParseLiteral();
    }

    private ExpressionNode ParseLiteral()
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

        throw new Exception("Expected literal, but found: " + _currentToken.Text);
    }

    private void Consume(TokenType expectedType)
    {
        if (_currentToken.Type != expectedType)
        {
            throw new Exception(
                "Expected token " + expectedType + ", but found " + _currentToken.Type + " with text '" + _currentToken.Text + "'.");
        }

        _currentToken = _lexer.NextToken();
    }
}