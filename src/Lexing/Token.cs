namespace Lexing;

public class Token
{
    public Token(TokenType type, string text)
    {
        Type = type;
        Text = text;
    }

    public TokenType Type { get; }

    public string Text { get; }

    public override string ToString()
    {
        return Type + ": " + Text;
    }
}