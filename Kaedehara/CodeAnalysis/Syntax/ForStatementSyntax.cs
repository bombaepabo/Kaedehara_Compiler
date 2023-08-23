namespace Kaedehara.CodeAnalysis.Syntax
{
    public sealed class ForStatementSyntax : StatementSyntax
    {
        public ForStatementSyntax(SyntaxToken keyword,SyntaxToken identifier,SyntaxToken equalsToken,ExpressionSyntax lowerbound,SyntaxToken toKeyword,ExpressionSyntax upperbound,StatementSyntax body){
            Keyword = keyword;
            Identifier = identifier;
            Lowerbound = lowerbound;
            ToKeyword = toKeyword;
            EqualsToken = equalsToken;
            Upperbound = upperbound;
            Body = body;
        }

        public override SyntaxKind Kind => SyntaxKind.ForStatement;
        public SyntaxToken Keyword { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken EqualsToken { get; }
        public ExpressionSyntax Lowerbound { get; }
        public SyntaxToken ToKeyword { get; }
        public ExpressionSyntax Upperbound { get; }
        public StatementSyntax Body { get; }

    }

} 