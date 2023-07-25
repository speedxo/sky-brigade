namespace VrekLang;

public enum TokenType
{
    Integer,
    Plus,
    Minus,
    Multiply,
    Divide,
    Definition,
    Variable,
    Assign,
    String,
    Identifier,
    EOF,
    EndOfStatement
}

public class Token 
{
    public TokenType Type { get; private set; }
    public string Lexeme { get; private set; }

    public Token(TokenType type, string lexeme)
    {
        this.Type = type;
        this.Lexeme = lexeme;
    }
    public override string ToString()
    {
        return $"({Type}, {Lexeme})";
    }
}

public class Lexer
{
    private readonly string sourceCode;
    private int position;

    public Lexer(string sourceCode)
    {
        this.sourceCode = sourceCode;
        this.position = 0;
    }

    private char CurrentChar()
    {
        return position < sourceCode.Length ? sourceCode[position] : '\0';
    }

    private void Advance()
    {
        position++;
    }
    private char Peek(int n=0)
    {
        int peekPosition = position + n;
        return peekPosition < sourceCode.Length ? sourceCode[peekPosition] : '\0';
    }
    private bool CheckNextPattern(string pattern)
    {
        for (int i = 0; i < pattern.Length; i++)
        {
            if (Peek(i) != pattern[i])
            {
                return false;
            }
        }
        return true;
    }

    public Token GetNextToken()
    {
        while (position < sourceCode.Length)
        {
            char currentChar = CurrentChar();

            if (char.IsDigit(currentChar))
            {
                return new Token(TokenType.Integer, GetInteger());
            }
            else if (currentChar == '"')
            {
                return new Token(TokenType.String, GetString());
            }
            else if (currentChar == 'v')
            {
                if (CheckNextPattern("var"))
                {
                    Advance();
                    Advance();
                    Advance();
                    return new Token(TokenType.Definition, "var");
                }
            }
            else if (currentChar == '=')
            {
                Advance();
                return new Token(TokenType.Assign, "=");
            }
            else if (char.IsLetter(currentChar))
            {
                return new Token(TokenType.Identifier, GetVariable());
            }

            while (char.IsWhiteSpace(CurrentChar())) { Advance(); }


            switch (currentChar)
            {
                case '+':
                    Advance();
                    return new Token(TokenType.Plus, currentChar.ToString());
                case '-':
                    Advance();
                    return new Token(TokenType.Minus, currentChar.ToString());
                case '*':
                    Advance();
                    return new Token(TokenType.Multiply, currentChar.ToString());
                case '/':
                    Advance();
                    return new Token(TokenType.Divide, currentChar.ToString());
                case ';':
                    Advance();
                    return new Token(TokenType.EndOfStatement, currentChar.ToString());
            }
        }

        return new Token(TokenType.EOF, "");
    }
    private string GetVariable()
    {
        string result = "";
        while (position < sourceCode.Length && !char.IsWhiteSpace(CurrentChar()))
        {
            result += CurrentChar();
            Advance();
        }
        return result;
    }
    private string GetInteger()
    {
        string result = "";
        while (position < sourceCode.Length && char.IsDigit(CurrentChar()))
        {
            result += CurrentChar();
            Advance();
        }
        return result;
    }

    public string GetString()
    {
        string result = "";
        Advance(); // Consume the leading "

        while (position < sourceCode.Length && CurrentChar() != '"')
        {
            result += CurrentChar();
            Advance();
        }
        
        Advance(); // Consume the tailing "

        return result;
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        string code = "var bingbong = 10;var test = \"cum\";\0";
        Lexer lexer = new Lexer(code);
        Token token;
        do { token = lexer.GetNextToken(); Console.WriteLine(token.ToString()); }
        while (token.Type != TokenType.EOF);
        
    }
}