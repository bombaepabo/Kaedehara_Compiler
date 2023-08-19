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
            Compilation previous = null;

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                if (textBuilder.Length == 0)
                {
                    Console.Write("» ");
                }
                else
                {
                    Console.Write("· ");
                }
                Console.ResetColor();

                var input = Console.ReadLine();
                var isBlank = string.IsNullOrWhiteSpace(input);
                if (textBuilder.Length == 0)
                {
                    if (isBlank)
                    {
                        break;
                    }
                    if (input == "#ShowTree")
                    {
                        ShowTree = !ShowTree;
                        Console.WriteLine(ShowTree ? " Showing parse trees." : "Not showing parse trees");
                        continue;
                    }
                    else if (input == "#clear")
                    {
                        Console.Clear();
                        continue;
                    }
                    else if (input == "#reset")
                    {
                        previous = null;
                        variables.Clear();
                        continue;
                    }
                }
                textBuilder.AppendLine(input);
                var text = textBuilder.ToString();
                var syntaxTree = SyntaxTree.Parse(text);
                if (!isBlank && syntaxTree.Diagnostics.Any())
                {
                    continue;
                }
                var compilation = previous == null 
                                    ? new Compilation(syntaxTree)
                                    : previous.ContinuedWith(syntaxTree);
                var result = compilation.Evaluate(variables);
                if (ShowTree)
                {
                    syntaxTree.Root.WriteTo(Console.Out);
                }

                if (!result.Diagnostics.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine(result.Value);
                    Console.ResetColor();
                    previous = compilation;
                }
                else
                {
                    foreach (var diag in result.Diagnostics)
                    {
                        var lineIndex = syntaxTree.Text.GetLineIndex(diag.Span.Start);
                        var line = syntaxTree.Text.Lines[lineIndex];
                        var lineNumber = lineIndex + 1;
                        var character = diag.Span.Start - line.Start + 1;

                        Console.WriteLine();

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