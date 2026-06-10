namespace Kaedehara.CodeAnalysis.Syntax
{
    public sealed class ArrayLiteralExpressionSyntax : ExpressionSyntax
    {
        public ArrayLiteralExpressionSyntax(SyntaxToken openBracketToken, SeparatedSyntaxList<ExpressionSyntax> elements, SyntaxToken closeBracketToken)
        {
            OpenBracketToken = openBracketToken;
            Elements = elements;
            CloseBracketToken = closeBracketToken;
        }

        public override SyntaxKind Kind => SyntaxKind.ArrayLiteralExpression;

        public SyntaxToken OpenBracketToken { get; }
        public SeparatedSyntaxList<ExpressionSyntax> Elements { get; }
        public SyntaxToken CloseBracketToken { get; }
    }
}
