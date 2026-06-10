namespace Kaedehara.CodeAnalysis.Syntax
{
    public sealed class AssignmentExpressionSyntax : ExpressionSyntax
    {
        public AssignmentExpressionSyntax(ExpressionSyntax left, SyntaxToken equalsToken, ExpressionSyntax expression)
        {
            Left = left;
            EqualsToken = equalsToken;
            Expression = expression;
        }

        public ExpressionSyntax Left { get; }
        public SyntaxToken EqualsToken { get; }
        public ExpressionSyntax Expression { get; }

        public override SyntaxKind Kind => SyntaxKind.AssignmentExpression;
    }
}