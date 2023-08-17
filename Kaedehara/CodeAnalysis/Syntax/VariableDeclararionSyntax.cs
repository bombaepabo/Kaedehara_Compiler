namespace Kaedehara.CodeAnalysis.Syntax
{
    public sealed class VariableDeclararionSyntax : StatementSyntax
    {
        public VariableDeclararionSyntax(SyntaxToken keyword,SyntaxToken identifer,SyntaxToken equalsToken,ExpressionSyntax initializer){
            Keyword = keyword;
            Identifer = identifer;
            EqualsToken = equalsToken;
            Initializer = initializer;
        }
        public override SyntaxKind Kind => SyntaxKind.VariableDeclaration;

        public SyntaxToken Keyword { get; }
        public SyntaxToken Identifer { get; }
        public SyntaxToken EqualsToken { get; }
        public ExpressionSyntax Initializer { get; }
    }
} 