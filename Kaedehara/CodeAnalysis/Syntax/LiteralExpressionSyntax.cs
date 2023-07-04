namespace KAEDEHARA_COMPILER.CodeAnalysis.Syntax
{
    public sealed class LiteralExpressionSyntax : ExpressionSyntax
    {
        public LiteralExpressionSyntax(SyntaxToken literalToken)
           : this(literalToken, literalToken.Value)
        {

        }
        public LiteralExpressionSyntax(SyntaxToken literalToken, object value)
        {
            this.LiteralToken = literalToken;
            Value = value;
        }


        public override SyntaxKind Kind => SyntaxKind.LiteralExpression;

        public SyntaxToken LiteralToken { get; }
        public object Value { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return LiteralToken;
        }
    }







}