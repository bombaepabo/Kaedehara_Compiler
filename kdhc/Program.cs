using KAEDEHARA_COMPILER.CodeAnalysis.Syntax;
using KAEDEHARA_COMPILER.CodeAnalysis.Binding;
using KAEDEHARA_COMPILER.CodeAnalysis;
namespace KAEDEHARA_COMPILER
{
    internal static class Program
    {
        private static void Main()
        {
            var ShowTree = false;
            var variables = new Dictionary<VariableSymbol, object>();
            while (true)
            {
                Console.Write(">");
                var line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                {
                    return;
                }
                if (line == "#ShowTree")
                {
                    ShowTree = !ShowTree;
                    Console.WriteLine(ShowTree ? " Showing parse trees." : "Not showing parse trees");
                    continue;
                }
                else if (line == "#cls")
                {
                    Console.Clear();
                    continue;
                }
                var syntaxTree = SyntaxTree.Parse(line);
                var compilation = new Compilation(syntaxTree);
                var result = compilation.Evaluate(variables);
                var diagnostics = result.Diagnostics;
                if (ShowTree)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    PrettyPrint(syntaxTree.Root);
                    Console.ResetColor();
                }

                if (!diagnostics.Any())
                {
                    Console.WriteLine(result.Value);

                }
                else
                {
                    foreach (var diag in diagnostics)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(diag);
                        Console.ResetColor();
                        var prefix = line.Substring(0, diag.Span.Start);
                        var error = line.Substring(diag.Span.Start, diag.Span.Length);
                        var suffix = line.Substring(diag.Span.End);
                         Console.Write("    ");
                        Console.Write(prefix);
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write(error);
                        Console.ResetColor();
                        Console.Write(suffix);
                        Console.WriteLine();
                    }
                    Console.WriteLine();

                }
            }
        }
        static void PrettyPrint(SyntaxNode node, string indent = "", bool isLast = true)
        {
            var marker = isLast ? "└──" : "├──";

            Console.Write(indent);
            Console.Write(marker);

            Console.Write(node.Kind);
            if (node is SyntaxToken t && t.Value != null)
            {
                Console.Write(" ");
                Console.Write(t.Value);

            }
            Console.WriteLine();

            indent += isLast ? "    " : "│  ";
            var lastChild = node.GetChildren().LastOrDefault();
            foreach (var child in node.GetChildren())
            {
                PrettyPrint(child, indent, child == lastChild);
            }
        }



    }
}