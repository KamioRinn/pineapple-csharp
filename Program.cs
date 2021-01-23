using System;
using System.IO;

namespace pineapple
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string code = File.ReadAllText(args[0]);
                backend.Execute(code);
            }
            catch{}
        }
    }
}
