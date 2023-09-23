using Kaedehara.CodeAnalysis.Text;

namespace Kaedehara.CodeAnalysis.Syntax
{
    public sealed class SyntaxToken : SyntaxNode
    {
        public SyntaxToken(SyntaxKind kind, int position, string text, Object value)
        {
            Kind = kind;
            Position = position;
            Text = text;
            Value = value;
        }
        public override SyntaxKind Kind { get; }
        public int Position { get; }
        public string Text { get; }
        public Object Value { get; }
        public override TextSpan Span => new TextSpan(Position, Text?.Length ?? 0);
        public bool IsMissing => Text == null ;

    }







}