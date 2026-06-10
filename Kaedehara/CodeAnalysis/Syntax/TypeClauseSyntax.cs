namespace Kaedehara.CodeAnalysis.Syntax
{
    public sealed class TypeClauseSyntax : SyntaxNode
    {
        public TypeClauseSyntax(SyntaxToken colonToken, SyntaxToken identifier, SyntaxToken openBracketToken = null, SyntaxToken closeBracketToken = null)
        {
            ColonToken = colonToken;
            Identifier = identifier;
            OpenBracketToken = openBracketToken;
            CloseBracketToken = closeBracketToken;
        }

        public override SyntaxKind Kind => SyntaxKind.TypeClause;
        public SyntaxToken ColonToken { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken OpenBracketToken { get; }
        public SyntaxToken CloseBracketToken { get; }
    }
}
