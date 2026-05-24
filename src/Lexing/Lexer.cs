using System.Text;

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

        if (IsEnglishLetter(current))
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

        switch (current)
        {
            case '(':
                return ReadSingleCharacterToken(TokenType.LeftParenthesis);

            case ')':
                return ReadSingleCharacterToken(TokenType.RightParenthesis);

            case '[':
                return ReadSingleCharacterToken(TokenType.LeftBracket);

            case ']':
                return ReadSingleCharacterToken(TokenType.RightBracket);

            case ',':
                return ReadSingleCharacterToken(TokenType.Comma);

            case ';':
                return ReadSingleCharacterToken(TokenType.Semicolon);

            case '+':
                return ReadSingleCharacterToken(TokenType.Plus);

            case '-':
                return ReadSingleCharacterToken(TokenType.Minus);

            case '*':
                return ReadSingleCharacterToken(TokenType.Star);

            case '/':
                return ReadSingleCharacterToken(TokenType.Slash);

            case '%':
                return ReadSingleCharacterToken(TokenType.Percent);

            case '=':
                return ReadSingleCharacterToken(TokenType.Equal);

            default:
                throw new Exception("Unexpected character: " + current);
        }
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

        while (!IsAtEnd() && IsIdentifierPart(CurrentChar()))
        {
            _position++;
        }

        string text = _source.Substring(start, _position - start);

        return text switch
        {
            "input" => new Token(TokenType.Input, text),
            "output" => new Token(TokenType.Output, text),
            "int" => new Token(TokenType.Int, text),
            "float" => new Token(TokenType.Float, text),
            "string" => new Token(TokenType.String, text),
            "const" => new Token(TokenType.Const, text),
            "len" => new Token(TokenType.Len, text),
            _ => new Token(TokenType.Identifier, text),
        };
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

            if (!IsAtEnd() && (char.IsLetter(CurrentChar()) || CurrentChar() == '_'))
            {
                throw new Exception("Invalid number literal");
            }

            string floatText = _source.Substring(start, _position - start);
            return new Token(TokenType.FloatLiteral, floatText);
        }

        if (!IsAtEnd() && (char.IsLetter(CurrentChar()) || CurrentChar() == '_'))
        {
            throw new Exception("Invalid number literal");
        }

        string intText = _source.Substring(start, _position - start);
        return new Token(TokenType.IntLiteral, intText);
    }

    private Token ReadString()
    {
        _position++;

        StringBuilder builder = new();

        while (!IsAtEnd() && CurrentChar() != '"')
        {
            if (CurrentChar() == '\\')
            {
                builder.Append(ReadEscapeSequence());
            }
            else
            {
                builder.Append(CurrentChar());
                _position++;
            }
        }

        if (IsAtEnd())
        {
            throw new Exception("Unterminated string literal");
        }

        _position++;

        return new Token(TokenType.StringLiteral, builder.ToString());
    }

    private char ReadEscapeSequence()
    {
        _position++;

        if (IsAtEnd())
        {
            throw new Exception("Unterminated escape sequence");
        }

        char current = CurrentChar();
        _position++;

        return current switch
        {
            '"' => '"',
            '\\' => '\\',
            'n' => '\n',
            't' => '\t',
            _ => throw new Exception("Unknown escape sequence: \\" + current),
        };
    }

    private Token ReadSingleCharacterToken(TokenType type)
    {
        string text = _source.Substring(_position, 1);
        _position++;
        return new Token(type, text);
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

    private static bool IsEnglishLetter(char character)
    {
        return character is >= 'a' and <= 'z' or >= 'A' and <= 'Z';
    }

    private static bool IsIdentifierPart(char character)
    {
        return IsEnglishLetter(character) || char.IsDigit(character) || character == '_';
    }
}