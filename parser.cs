using System.Collections.Generic;

namespace pineapple
{
    class parser
    {
        public struct Variable
        {
            public int LineNum;
            public string Name;
        }

        public struct Assignment : Statement
        {
            public int LineNum;
            public Variable Variable;
            public string String;
            public System.Type ValueType()
            {
                return this.GetType();
            }
        }

        public struct Print : Statement
        {
            public int LineNum;
            public Variable Variable;

            public System.Type ValueType()
            {
                return this.GetType();
            }
        }

        public interface Statement
        {
            System.Type ValueType();
        };

        public struct SourceCode
        {
            public int LineNum;
            public List<Statement> Statements;
        }

        //Name ::= [_A-Za-z][_0-9A-Za-z]*
        static public (string, string) parseName(ref lexer.Lexer Lexer)
        {
            var (_, name) = Lexer.NextTokenIs(lexer.TOKEN_NAME);
            return (name, null);
        }

        //String ::= '"' '"' Ignored | '"' StringCharacter '"' Ignored
        static public (string, string) parseString(ref lexer.Lexer Lexer)
        {
            string str = "";
            switch (Lexer.LookAhead())
            {
                case lexer.TOKEN_DUOQUOTE:
                    Lexer.NextTokenIs(lexer.TOKEN_DUOQUOTE);
                    Lexer.LookAheadAndSkip(lexer.TOKEN_IGNORED);
                    return (str, null);
                case lexer.TOKEN_QUOTE:
                    Lexer.NextTokenIs(lexer.TOKEN_QUOTE);
                    str = Lexer.scanBeforeToken(lexer.tokenNameMap[lexer.TOKEN_QUOTE]);
                    Lexer.NextTokenIs(lexer.TOKEN_QUOTE);
                    Lexer.LookAheadAndSkip(lexer.TOKEN_IGNORED);
                    return (str, null);
                default:
                    return (null, "parseString(): not a string.");
            }
        }

        //Variable ::= "$" Name Ignored
        static public (Variable, string) parseVariable(ref lexer.Lexer Lexer)
        {
            Variable variable = new Variable();
            string error;
            variable.LineNum = Lexer.GetLineNum();
            Lexer.NextTokenIs(lexer.TOKEN_VAR_PREFIX);
            (variable.Name, error) = parseName(ref Lexer);
            if (error != null)
                return (variable, error);
            Lexer.LookAheadAndSkip(lexer.TOKEN_IGNORED);
            return (variable, null);
        }

        //Assignment  ::= Variable Ignored "=" Ignored String Ignored
        static public (Assignment, string) parseAssignment(ref lexer.Lexer Lexer)
        {
            Assignment assignment = new Assignment();
            string error;
            assignment.LineNum = Lexer.GetLineNum();
            (assignment.Variable, error) = parseVariable(ref Lexer);
            if (error != null)
                return (assignment, error);
            Lexer.LookAheadAndSkip(lexer.TOKEN_IGNORED);
            Lexer.NextTokenIs(lexer.TOKEN_EQUAL);
            Lexer.LookAheadAndSkip(lexer.TOKEN_IGNORED);
            (assignment.String, error) = parseString(ref Lexer);
            if (error != null)
                return (assignment, error);
            Lexer.LookAheadAndSkip(lexer.TOKEN_IGNORED);
            return (assignment, null);
        }

        //Print ::= "print" "(" Ignored Variable Ignored ")" Ignored
        static public (Print, string) parsePrint(ref lexer.Lexer Lexer)
        {
            Print print = new Print();
            string error;
            print.LineNum = Lexer.GetLineNum();
            Lexer.NextTokenIs(lexer.TOKEN_PRINT);
            Lexer.NextTokenIs(lexer.TOKEN_LEFT_PAREN);
            Lexer.LookAheadAndSkip(lexer.TOKEN_IGNORED);
            (print.Variable, error) = parseVariable(ref Lexer);
            if (error != null)
                return (print, error);
            Lexer.LookAheadAndSkip(lexer.TOKEN_IGNORED);
            Lexer.NextTokenIs(lexer.TOKEN_RIGHT_PAREN);
            Lexer.LookAheadAndSkip(lexer.TOKEN_IGNORED);
            return (print, null);
        }

        //Statement ::= Print | Assignment
        static public (List<Statement>, string) parseStatements(ref lexer.Lexer Lexer)
        {
            List<Statement> statements = new List<Statement>();
            while (!isSourceCodeEnd(Lexer.LookAhead()))
            {
                var (statement, error) = parseStatement(ref Lexer);
                if (error != null)
                    return (null, error);
                statements.Add(statement);
            }
            return (statements, null);
        }

        static public (Statement, string) parseStatement(ref lexer.Lexer Lexer)
        {
            Lexer.LookAheadAndSkip(lexer.TOKEN_IGNORED);
            switch (Lexer.LookAhead())
            {
                case lexer.TOKEN_PRINT:
                    return parsePrint(ref Lexer);
                case lexer.TOKEN_VAR_PREFIX:
                    return parseAssignment(ref Lexer);
                default:
                    return (null, "parseStatement(): unknown Statement.");
            }
        }

        static private bool isSourceCodeEnd(int token)
        {
            if (token == lexer.TOKEN_EOF)
                return true;
            return false;
        }

        //SourceCode ::= Statement+ 
        static public (SourceCode, string) parseSourceCode(ref lexer.Lexer Lexer)
        {
            SourceCode sourceCode = new SourceCode();
            string error;
            sourceCode.LineNum = Lexer.GetLineNum();
            (sourceCode.Statements, error) = parseStatements(ref Lexer);
            if (error != null)
                return (sourceCode, error);
            return (sourceCode, null);
        }

        static public (SourceCode, string) parse(string code)
        {
            lexer.Lexer Lexer = lexer.NewLexer(code);
            var (sourceCode, error) = parseSourceCode(ref Lexer);
            if (error != null)
                return (sourceCode, error);
            Lexer.NextTokenIs(lexer.TOKEN_EOF);
            return (sourceCode, null);
        }
    }
}