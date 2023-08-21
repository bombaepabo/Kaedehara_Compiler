namespace Kaedehara.CodeAnalysis.Syntax
{
    public sealed class IfStatementSyntax : StatementSyntax
    {
        public IfStatementSyntax(SyntaxToken ifKeyword, ExpressionSyntax condition,StatementSyntax thenStatement,ElseClauseSyntax elseCaluse){
            IfKeyword = ifKeyword;
            Condition = condition;
            ThenStatement = thenStatement;
            ElseClause = elseCaluse;
        }

        public SyntaxToken IfKeyword { get; }
        public ExpressionSyntax Condition { get; }
        public StatementSyntax ThenStatement { get; }
        public ElseClauseSyntax ElseClause { get; }
        public override SyntaxKind Kind => SyntaxKind.IfStatement;
    }
} 