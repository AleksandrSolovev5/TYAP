namespace Lexing;

public class Lexer
{
    private readonly string _source;
    private int _position;

    public Lexer(string source)
    {
        _source = source;
        _position = 0;
    }

    public Token NextToken()
    {
        SkipIgnored();

        if (IsAtEnd())
        {
            return new Token(TokenType.EndOfFile, string.Empty);
        }

        char current = CurrentChar();

        if (char.IsLetter(current))
        {
            return ReadIdentifierOrKeyword();
        }

        if (char.IsDigit(current))
        {
            return ReadNumber();
        }

        if (current == '"')
        {
            return ReadString();
        }

        if (current == '(')
        {
            _position++;
            return new Token(TokenType.LeftRoundBracket, "(");
        }

        if (current == ')')
        {
            _position++;
            return new Token(TokenType.RightRoundBracket, ")");
        }

        if (current == ',')
        {
            _position++;
            return new Token(TokenType.Comma, ",");
        }

        if (current == ';')
        {
            _position++;
            return new Token(TokenType.Semicolon, ";");
        }

        throw new Exception("Unexpected character: " + current);
    }

    private void SkipIgnored()
    {
        bool skippedSomething = true;

        while (skippedSomething)
        {
            skippedSomething = false;

            while (!IsAtEnd() && char.IsWhiteSpace(CurrentChar()))
            {
                _position++;
                skippedSomething = true;
            }

            if (!IsAtEnd() && CurrentChar() == '$')
            {
                if (PeekChar() == '*')
                {
                    SkipBlockComment();
                    skippedSomething = true;
                }
                else
                {
                    SkipLineComment();
                    skippedSomething = true;
                }
            }
        }
    }

    private void SkipLineComment()
    {
        _position++;

        while (!IsAtEnd() && CurrentChar() != '\n')
        {
            _position++;
        }

        if (!IsAtEnd())
        {
            _position++;
        }
    }

    private void SkipBlockComment()
    {
        _position++;
        _position++;

        while (!IsAtEnd())
        {
            if (CurrentChar() == '*' && PeekChar() == '$')
            {
                _position++;
                _position++;
                return;
            }

            _position++;
        }

        throw new Exception("Unterminated block comment");
    }

    private Token ReadIdentifierOrKeyword()
    {
        int start = _position;

        while (!IsAtEnd() && (char.IsLetterOrDigit(CurrentChar()) || CurrentChar() == '_'))
        {
            _position++;
        }

        string text = _source.Substring(start, _position - start);

        if (text == "output")
        {
            return new Token(TokenType.Output, text);
        }

        throw new Exception("Unknown identifier: " + text);
    }

    private Token ReadNumber()
    {
        int start = _position;

        while (!IsAtEnd() && char.IsDigit(CurrentChar()))
        {
            _position++;
        }

        if (!IsAtEnd() && CurrentChar() == '.')
        {
            _position++;

            if (IsAtEnd() || !char.IsDigit(CurrentChar()))
            {
                throw new Exception("Invalid float literal");
            }

            while (!IsAtEnd() && char.IsDigit(CurrentChar()))
            {
                _position++;
            }

            string floatText = _source.Substring(start, _position - start);
            return new Token(TokenType.FloatLiteral, floatText);
        }

        string intText = _source.Substring(start, _position - start);
        return new Token(TokenType.IntLiteral, intText);
    }

    private Token ReadString()
    {
        _position++;

        string text = "";

        while (!IsAtEnd() && CurrentChar() != '"')
        {
            text = text + CurrentChar();
            _position++;
        }

        if (IsAtEnd())
        {
            throw new Exception("Unterminated string literal");
        }

        _position++;
        return new Token(TokenType.StringLiteral, text);
    }

    private char CurrentChar()
    {
        return _source[_position];
    }

    private char PeekChar()
    {
        if (_position + 1 >= _source.Length)
        {
            return '\0';
        }

        return _source[_position + 1];
    }

    private bool IsAtEnd()
    {
        return _position >= _source.Length;
    }
}