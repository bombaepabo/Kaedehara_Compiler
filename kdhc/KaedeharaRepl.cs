using Kaedehara.CodeAnalysis.Syntax;
using Kaedehara.CodeAnalysis;
using Kaedehara.CodeAnalysis.Text;
using Kaedehara.CodeAnalysis.Symbols;

namespace kdhc
{
    internal sealed class KaedeharaRepl : Repl
    {
        private Compilation _previous;
        private bool _showTree;
        private bool _showProgram;
        private readonly Dictionary<VariableSymbol, object> _variables = new Dictionary<VariableSymbol, object>();
        protected override void RenderLine(string line)
        {
            var tokens = SyntaxTree.ParseToken(line);
            foreach (var token in tokens)
            {
                var isKeyword = token.Kind.ToString().EndsWith("Keyword");
                var isIdentifier = token.Kind == SyntaxKind.IdentifierToken;
                var isNumber = token.Kind == SyntaxKind.NumberToken;
                var isString = token.Kind == SyntaxKind.StringToken;

                if (isKeyword)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                }
                else if (isIdentifier)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;

                }
                else if (isNumber)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }
                else if (isString)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                }

                Console.Write(token.Text);
                Console.ResetColor();
            }
        }
        protected override void EvaluateMetaCommand(string input)
        {
            switch (input)
            {
                case "#ShowTree":
                    _showTree = !_showTree;
                    Console.WriteLine(_showTree ? " Showing parse trees." : "Not showing parse trees.");
                    break;
                case "#ShowProgram":
                    _showProgram = !_showProgram;
                    Console.WriteLine(_showProgram ? " Showing bound trees." : "Not showing bound trees.");
                    break;
                case "#clear":
                    Console.Clear();
                    break;
                case "#reset":
                    _previous = null;
                    _variables.Clear();
                    break;
                default:
                    base.EvaluateMetaCommand(input);
                    break;
            }
        }

        protected override bool isCompleteSubmission(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return true;
            }
            var lastTwoLinesAreBlank = text.Split(Environment.NewLine).Reverse().TakeWhile(s => string.IsNullOrEmpty(s)).Take(2).Count() == 2;
            if(lastTwoLinesAreBlank){
                return true ;
            }
            var syntaxTree = SyntaxTree.Parse(text);
            if (syntaxTree.Diagnostics.Any())
            {
                return false;
            }
            if (syntaxTree.Root.Statement.GetLastToken().IsMissing)
            {
                return false;
            }
            return true;
        }

        protected override void EvaluateSubmission(string text)
        {
            var syntaxTree = SyntaxTree.Parse(text);

            var compilation = _previous == null
                                ? new Compilation(syntaxTree)
                                : _previous.ContinuedWith(syntaxTree);
            if (_showTree)
            {
                syntaxTree.Root.WriteTo(Console.Out);
            }
            if (_showProgram)
            {
                compilation.EmitTree(Console.Out);
            }

            var result = compilation.Evaluate(_variables);
            if (!result.Diagnostics.Any())
            {
                if (result.Value != null){
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(result.Value);
                    Console.ResetColor();
                }
                _previous = compilation;
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
        }
    }
}
