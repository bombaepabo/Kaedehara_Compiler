using KAEDEHARA_COMPILER.CodeAnalysis;
namespace KAEDEHARA_COMPILER
{
    class Program
    {
        static void Main(string[] args)
        {
            bool ShowTree = false;
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
                // var parser = new Parser(line);
                var syntaxTree = SyntaxTree.Parse(line);
                if (ShowTree)
                {
                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    PrettyPrint(syntaxTree.Root);
                    Console.ForegroundColor = color;
                }

                if (syntaxTree.Diagnostics.Any())
                {
                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    foreach (var diag in syntaxTree.Diagnostics)
                    {
                        Console.WriteLine(diag);
                    }
                    Console.ForegroundColor = color;

                }
                else
                {
                    var e = new Evaluator(syntaxTree.Root);
                    var result = e.Evaluate();
                    Console.WriteLine(result);
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

            indent += isLast ? "    " : "│   ";
            var lastChild = node.GetChildren().LastOrDefault();
            foreach (var child in node.GetChildren())
            {
                PrettyPrint(child, indent, child == lastChild);
            }
        }



    }
}