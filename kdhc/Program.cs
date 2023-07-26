using Kaedehara.CodeAnalysis.Syntax;
using Kaedehara.CodeAnalysis.Binding;
using Kaedehara.CodeAnalysis;
using System.Text;
using Kaedehara.CodeAnalysis.Text;

namespace kdhc
{
    internal static class Program
    {
        private static void Main()
        {
            var ShowTree = false;
            var variables = new Dictionary<VariableSymbol, object>();
            var textBuilder = new StringBuilder();

            while (true)
            {
                if (textBuilder.Length == 0)
                {
                    Console.Write("> ");
                }
                else
                {
                    Console.Write("| ");
                }

                var input = Console.ReadLine();
                var isBlank = string.IsNullOrWhiteSpace(input);
                if (textBuilder.Length == 0)
                {
                    if (isBlank)
                    {
                        break;
                    }

                    if (string.IsNullOrEmpty(input))
                    {
                        return;
                    }
                    if (input == "#ShowTree")
                    {
                        ShowTree = !ShowTree;
                        Console.WriteLine(ShowTree ? " Showing parse trees." : "Not showing parse trees");
                        continue;
                    }
                    else if (input == "#cls")
                    {
                        Console.Clear();
                        continue;
                    }
                }
                else
                {

                }
                textBuilder.AppendLine(input);
                var text = textBuilder.ToString();
                var syntaxTree = SyntaxTree.Parse(text);
                if (!isBlank && syntaxTree.Diagnostics.Any())
                {
                    continue;
                }
                var compilation = new Compilation(syntaxTree);
                var result = compilation.Evaluate(variables);
                var diagnostics = result.Diagnostics;
                if (ShowTree)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    syntaxTree.Root.WriteTo(Console.Out);
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
                        var lineIndex = syntaxTree.Text.GetLineIndex(diag.Span.Start);
                        var line = syntaxTree.Text.Lines[lineIndex];
                        var lineNumber = lineIndex + 1;
                        var character = diag.Span.Start - line.Start + 1;


                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write($"({lineNumber},{character}): ");
                        Console.WriteLine(diag);
                        Console.ResetColor();
                        var prefixSpan = TextSpan.FromBounds(line.Start, diag.Span.Start);
                        var suffixSpan = TextSpan.FromBounds(diag.Span.End, line.End);
                        var prefix = syntaxTree.Text.ToString(prefixSpan);
                        var error = syntaxTree.Text.ToString(diag.Span);
                        var suffix = syntaxTree.Text.ToString(suffixSpan);
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
                textBuilder.Clear();
            }
        }



    }
}