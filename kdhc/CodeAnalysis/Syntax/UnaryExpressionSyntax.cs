namespace KAEDEHARA_COMPILER.CodeAnalysis.Syntax
{
    sealed class UnaryExpressionSyntax : ExpressionSyntax
        {
            public UnaryExpressionSyntax(SyntaxToken operatortoken,ExpressionSyntax operand){
                Operatortoken = operatortoken;
                Operand = operand;
        }
            public override SyntaxKind Kind => SyntaxKind.UnaryExpression ;
            public SyntaxToken Operatortoken { get; }
        public ExpressionSyntax Operand { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
            {
                yield return Operatortoken ; 
                yield return Operand;
            }
            
        }









}