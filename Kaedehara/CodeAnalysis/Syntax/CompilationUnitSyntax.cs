namespace Kaedehara.CodeAnalysis.Syntax

    public sealed class CompilationUnitSyntax : SyntaxNode
{
    public CompilationUnitSyntax(ExpressionSyntax expression, SyntaxToken endofFileToken)
    {
        ExpressionSyntax = expression;
        EndofFileToken = endofFileToken;
    }
    public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
    public ExpressionSyntax Expression { get; }
    public SyntaxToken EndofFileToken { get; }

}