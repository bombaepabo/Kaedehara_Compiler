using System.Collections;
using Kaedehara.CodeAnalysis.Symbols;
using Kaedehara.CodeAnalysis.Syntax;
using Kaedehara.CodeAnalysis.Text;

namespace Kaedehara.CodeAnalysis
{
    internal sealed class DiagnosticBag : IEnumerable<Diagnostic>
    {
        private readonly List<Diagnostic> _diagnostics = new List<Diagnostic>();

        public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void AddRange(DiagnosticBag diagonostics)
        {
            _diagnostics.AddRange(diagonostics._diagnostics);
        }
        public void ReportBadCharacter(int position, char character)
        {
            var span = new TextSpan(position, 1);
            var message = $"ERROR: bad charactor input: '{character}'";
            Report(span, message);
        }

        public void ReportInvalidNumber(TextSpan span, string text, TypeSymbol type)
        {
            var message = $"The number {text} isn't valid {type}";
            Report(span, message);
        }



        private void Report(TextSpan span, string message)
        {
            var diagonostic = new Diagnostic(span, message);
            _diagnostics.Add(diagonostic);

        }

        public void ReportUnexpectedToken(TextSpan span, SyntaxKind actualKind, SyntaxKind expectedKind)
        {
            var message = $"Unexpected token <{actualKind}>, expected <{expectedKind}>";
            Report(span, message);
        }

        public void ReportUndefinedUnaryOperator(TextSpan span, string operatorText, TypeSymbol operandType)
        {
            var message = $"Unary operator '{operatorText}' is not defined for type '{operandType}'.";
            Report(span, message);
        }

        public void ReportUndefinedBinaryOperator(TextSpan span, string operatorText, TypeSymbol boundLeft, TypeSymbol boundRight)
        {
            var message = $"Binary operator '{operatorText}' is not defined for types '{boundLeft}' and '{boundRight}'.";
            Report(span, message);
        }

        public void ReportUndefinedName(TextSpan span, string name)
        {
            var message = $"Variables '{name}' doesn't exist.";
            Report(span, message);
        }

        internal void ReportCannotConvert(TextSpan span, TypeSymbol fromType, TypeSymbol toType)
        {
             var message = $"Cannot convert type '{fromType}' to '{toType}'.";
            Report(span, message);
        }

        internal void ReportVariableAlreadyDeclared(TextSpan span, string name)
        {
            var message = $"Variable '{name}' is already declared.";
            Report(span, message);
        }

        internal void ReportCannotAssign(TextSpan span, string name)
        {
            var message = $"Variable '{name}' is read-only and cannot be assigned to.";
            Report(span, message);
        }

        public void ReportUnterminatedString(TextSpan span)
        {
            var message = "Unterminated string literal.";
            Report(span, message);
        }
    }








}