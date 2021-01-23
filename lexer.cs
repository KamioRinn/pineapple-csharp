using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace pineapple
{
    class lexer
    {
        //token const
        public const int TOKEN_EOF = 0; // end-of-file
        public const int TOKEN_VAR_PREFIX = 1; // $
        public const int TOKEN_LEFT_PAREN = 2; // (
        public const int TOKEN_RIGHT_PAREN = 3; // )
        public const int TOKEN_EQUAL = 4; // =
        public const int TOKEN_QUOTE = 5; // "
        public const int TOKEN_DUOQUOTE = 6; // ""
        public const int TOKEN_NAME = 7; // Name ::= [_A-Za-z][_0-9A-Za-z]*
        public const int TOKEN_PRINT = 8; // print

        public const int TOKEN_IGNORED = 9; // Ignored

        static public Dictionary<int, string> tokenNameMap = new Dictionary<int, string>()
        {
            {TOKEN_EOF,"EOF"},
            {TOKEN_VAR_PREFIX,"$"},
            {TOKEN_LEFT_PAREN,"("},
            {TOKEN_RIGHT_PAREN,")"},
            {TOKEN_EQUAL,"="},
            {TOKEN_QUOTE,"\""},
            {TOKEN_DUOQUOTE,"\"\""},
            {TOKEN_NAME,"Name"},
            {TOKEN_PRINT,"print"},
            {TOKEN_IGNORED,"Ignored"}
        };

        static Dictionary<string, int> keywords = new Dictionary<string, int>()
        {
            {"print",TOKEN_PRINT}
        };

        public struct Lexer
        {
            public string sourceCode;
            public int lineNum;
            public string nextToken;
            public int nextTokenType;
            public int nextTokenLineNum;

            public (int, string) NextTokenIs(int tokenType)
            {
                var (nowLineNum, nowTokenType, nowToken) = GetNextToken();
                //syntax error
                if (tokenType != nowTokenType)
                {
                    Console.WriteLine("NextTokenIs(): syntax error near \"" + tokenNameMap[nowTokenType] + "\", expected token: {" + tokenNameMap[tokenType] + "} but got {" + tokenNameMap[nowTokenType] + "}.");
                    throw new System.Exception();
                }
                return (nowLineNum, nowToken);
            }

            public int LookAhead()
            {
                //lexer.nextToken* already setted
                if (nextTokenLineNum > 0)
                    return nextTokenType;
                //set it
                int nowLineNum = lineNum;
                var (ln, tokenType, token) = GetNextToken();
                lineNum = nowLineNum;
                nextTokenLineNum = ln;
                nextTokenType = tokenType;
                nextToken = token;
                return tokenType;
            }

            public void LookAheadAndSkip(int expectedType)
            {
                //get next token
                int nowLineNum = lineNum;
                var (ln, tokenType, token) = GetNextToken();
                //not is expected type, reverse cursor
                if (tokenType != expectedType)
                {
                    lineNum = nowLineNum;
                    nextTokenLineNum = ln;
                    nextTokenType = tokenType;
                    nextToken = token;
                }
            }

            private (int, int, string) GetNextToken()
            {
                if (nextTokenLineNum > 0)
                {
                    int ln = nextTokenLineNum;
                    lineNum = nextTokenLineNum;
                    nextTokenLineNum = 0;
                    return (ln, nextTokenType, nextToken);
                }
                return MatchToken();
            }

            public int GetLineNum()
            {
                return lineNum;
            }

            public (int, int, string) MatchToken()
            {
                //check ignored
                if (isIgnored())
                    return (lineNum, TOKEN_IGNORED, "Ignored");
                //finish
                if (sourceCode.Length == 0)
                    return (lineNum, TOKEN_EOF, tokenNameMap[TOKEN_EOF]);
                //check token
                switch (sourceCode[0])
                {
                    case '$':
                        skipSourceCode(1);
                        return (lineNum, TOKEN_VAR_PREFIX, "$");
                    case '(':
                        skipSourceCode(1);
                        return (lineNum, TOKEN_LEFT_PAREN, "(");
                    case ')':
                        skipSourceCode(1);
                        return (lineNum, TOKEN_RIGHT_PAREN, ")");
                    case '=':
                        skipSourceCode(1);
                        return (lineNum, TOKEN_EQUAL, "=");
                    case '"':
                        if (nextSourceCodeIs("\"\""))
                        {
                            skipSourceCode(2);
                            return (lineNum, TOKEN_DUOQUOTE, "\"\"");
                        }
                        skipSourceCode(1);
                        return (lineNum, TOKEN_QUOTE, "\"");
                }

                // check multiple character token
                if (sourceCode[0] == '_' || char.IsLetter(sourceCode[0]))
                {
                    string token = scanName();
                    if (keywords.TryGetValue(token, out int tokenType))
                        return (lineNum, tokenType, token);
                    return (lineNum, TOKEN_NAME, token);
                }

                //unexpected symbol
                Console.WriteLine("MatchToken(): unexpected symbol near \"" + sourceCode[0] + "\".");
                throw new System.Exception();
            }

            public void skipSourceCode(int n)
            {
                sourceCode = sourceCode.Substring(n);
            }

            public bool nextSourceCodeIs(string s)
            {
                if (s.Length < sourceCode.Length && s.CompareTo(sourceCode.Remove(s.Length)) == 0)
                    return true;
                else if (s == sourceCode)
                    return true;
                return false;
            }

            public string scanName()
            {
                return scan(new Regex(@"^[_\d\w]+"));
            }

            public string scan(Regex regexp)
            {
                string token = regexp.Match(sourceCode).Groups[0].Value;
                if (token != "")
                {
                    skipSourceCode(token.Length);
                    return token;
                }
                Console.WriteLine("unreachable!");
                throw new System.Exception();
            }

            //return content before token
            public string scanBeforeToken(string token)
            {
                string[] s = sourceCode.Split(token);
                if (s.Length < 2)
                {
                    Console.WriteLine("unreachable!");
                    throw new System.Exception();
                }
                skipSourceCode(s[0].Length);
                return s[0];
            }

            public bool isIgnored()
            {
                bool isIgnored = false;
                bool isWhiteSpace(char c)
                {
                    switch (c)
                    {
                        case '\t':
                        case '\n':
                        case '\v':
                        case '\f':
                        case '\r':
                        case ' ':
                            return true;
                    }
                    return false;
                }
                //matching
                while (sourceCode.Length > 0)
                {
                    //isNewLine
                    if (nextSourceCodeIs("\r\n") || nextSourceCodeIs("\n\r"))
                    {
                        skipSourceCode(2);
                        lineNum++;
                        isIgnored = true;
                    }
                    else if (sourceCode[0] == '\r' || sourceCode[0] == '\n')
                    {
                        skipSourceCode(1);
                        lineNum++;
                        isIgnored = true;
                    }
                    //isWhiteSpace
                    else if (isWhiteSpace(sourceCode[0]))
                    {
                        skipSourceCode(1);
                        isIgnored = true;
                    }
                    else
                    {
                        break;
                    }
                }
                return isIgnored;
            }
        }

        static public Lexer NewLexer(string sourceCode)
        {
            return new Lexer() { sourceCode = sourceCode, lineNum = 1, nextToken = "", nextTokenType = 0, nextTokenLineNum = 0 };
        }








    }

}