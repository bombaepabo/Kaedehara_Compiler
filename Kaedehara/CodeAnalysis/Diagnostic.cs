using System.Collections;
using KAEDEHARA_COMPILER.CodeAnalysis.Syntax;

namespace KAEDEHARA_COMPILER.CodeAnalysis
{
    public sealed class Diagnostic {
        public Diagnostic(TextSpan span,string message){
            Span = span;
            Message = message;
        }

        public TextSpan Span { get; }
        public string Message { get; }
        public override string ToString() => Message;
       
    }
    internal sealed class DiagnosticBag : IEnumerable<Diagnostic>
    {
        private readonly List<Diagnostic> _diagnostics = new List<Diagnostic>();

        public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();

        public void AddRange(DiagnosticBag diagonostics)
        {
            _diagnostics.AddRange(diagonostics._diagnostics);
        }
        public void ReportBadCharacter(int position, char current)
        {
            var span = new TextSpan(position,1);
            var message = $"ERROR: bad charactor input: '{current}'" ; 
            Report(span,message);
        }

        public void ReportInvalidNumber(TextSpan span, string text, Type type)
        {
            var message = $"The number {text} isn't valid {type}";
            Report(span,message);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    

        private void Report(TextSpan span , string message ){
            var diagonostic = new Diagnostic(span,message);
            _diagnostics.Add(diagonostic);

        }

        internal void ReportUnexpectedToken(TextSpan span, SyntaxKind actualKind, SyntaxKind expectedKind)
        {
            var message = $"Unexpected token <{actualKind}>, expected <{expectedKind}>";
            Report(span,message);
        }

        internal void ReportUndefinedUnaryOperator(TextSpan span, string? operatorText, Type operandType)
        {
            var message = $"Unary operator '{operatorText}' is not defined for type {operandType}.";
            Report(span,message);
        }

        internal void ReportUndefinedBinaryOperator(TextSpan span, string? operatorText, Type boundLeft, Type boundRight)
        {
            var message = $"Binary operator '{operatorText}' is not defined for type {boundLeft} and {boundRight}." ;
            Report(span,message);
        }

        internal void ReportUndefinedName(TextSpan span, string name)
        {
              var message = $"Variables'{name}' doesn't exist." ;
            Report(span,message);
        }
    }








}