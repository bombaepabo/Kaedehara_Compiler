namespace Kaedehara.CodeAnalysis.Syntax
{
    sealed class BinaryExpressionSyntax : ExpressionSyntax
    {
        public BinaryExpressionSyntax(ExpressionSyntax left, SyntaxToken operatortoken, ExpressionSyntax right)
        {
            Left = left;
            Operatortoken = operatortoken;
            Right = right;
        }
        public override SyntaxKind Kind => SyntaxKind.BinaryExpression;
        public ExpressionSyntax Left { get; }
        public SyntaxToken Operatortoken { get; }
        public ExpressionSyntax Right { get; }

    }









}