using System;
using System.Collections.Generic;

namespace pineapple
{
    class backend
    {
        public struct GlobalVariables
        {
            public Dictionary<string, string> Variables;
        }

        static public GlobalVariables NewGlobalVariables()
        {
            GlobalVariables globalVariables = new GlobalVariables();
            globalVariables.Variables = new Dictionary<string, string>();
            return globalVariables;
        }

        static public void Execute(string code)
        {
            //parse
            var (ast, error) = parser.parse(code);
            if (error != null)
            {
                Console.WriteLine(error);
                throw new System.Exception();
            }

            //resolve
            error = resolveAST(NewGlobalVariables(), ast);
            if (error != null)
                throw new System.Exception();
        }

        static public string resolveAST(GlobalVariables g, parser.SourceCode ast)
        {
            if (ast.Statements.Count == 0)
                return "resolveAST(): no code to execute, please check your input.";
            foreach (var statement in ast.Statements)
            {
                string error = resolveStatement(g, statement);
                if (error != null)
                    return error;
            }
            return null;
        }

        static public string resolveStatement(GlobalVariables g, parser.Statement statement)
        {
            var assignment = new parser.Assignment().GetType();
            var print = new parser.Print().GetType();
            if (statement.GetType() == assignment)
            {
                return resolveAssignment(g, (parser.Assignment)statement);
            }
            else if (statement.GetType() == print)
            {
                return resolvePrint(g, (parser.Print)statement);
            }
            return "resolveStatement(): undefined statement type.";
        }

        static public string resolveAssignment(GlobalVariables g, parser.Assignment assignment)
        {
            string varName = assignment.Variable.Name;
            if (varName == null)
                return "resolveAssignment(): variable name can NOT be empty.";
            g.Variables[varName] = assignment.String;
            return null;
        }

        static public string resolvePrint(GlobalVariables g, parser.Print print)
        {
            string varName = print.Variable.Name;
            if (varName == null)
                return "resolvePrint(): variable name can NOT be empty.";
            if (!g.Variables.TryGetValue(varName, out string str))
                return "resolvePrint(): variable \"$" + varName + "\"not found.";
            Console.WriteLine(str);
            return null;
        }
    }
}