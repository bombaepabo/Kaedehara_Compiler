namespace Kaedehara.CodeAnalysis.Syntax
{
    public sealed class CompilationUnitSyntax : SyntaxNode
    {
        public CompilationUnitSyntax(StatementSyntax statement, SyntaxToken endofFileToken)
        {
            Statement = statement;
            EndofFileToken = endofFileToken;
        }
        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
        public StatementSyntax Statement { get; }
        public SyntaxToken EndofFileToken { get; }

    }
}