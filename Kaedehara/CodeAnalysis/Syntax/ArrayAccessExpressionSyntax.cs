namespace Kaedehara.CodeAnalysis.Syntax
{
    public sealed class ArrayAccessExpressionSyntax : ExpressionSyntax
    {
        public ArrayAccessExpressionSyntax(ExpressionSyntax array, SyntaxToken openBracketToken, ExpressionSyntax index, SyntaxToken closeBracketToken)
        {
            Array = array;
            OpenBracketToken = openBracketToken;
            Index = index;
            CloseBracketToken = closeBracketToken;
        }

        public override SyntaxKind Kind => SyntaxKind.ArrayAccessExpression;

        public ExpressionSyntax Array { get; }
        public SyntaxToken OpenBracketToken { get; }
        public ExpressionSyntax Index { get; }
        public SyntaxToken CloseBracketToken { get; }
    }
}
