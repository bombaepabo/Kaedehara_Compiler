using KAEDEHARA_COMPILER.CodeAnalysis.Syntax;

namespace KAEDEHARA_COMPILER.CodeAnalysis.Binding;
internal sealed class Binder
{
    private readonly DiagnosticBag _diagnostics = new DiagnosticBag();
    public DiagnosticBag Diagnostics  => _diagnostics;
    public BoundExpression BindExpression(ExpressionSyntax syntax)
    {
        switch (syntax.Kind)
        {
            case SyntaxKind.LiteralExpression:
                return BindLiteralExpression((LiteralExpressionSyntax)syntax);
            case SyntaxKind.BinaryExpression:
                return BindBinaryExpression((BinaryExpressionSyntax)syntax);
            case SyntaxKind.UnaryExpression:
                return BindUnaryExpression((UnaryExpressionSyntax)syntax);
            case SyntaxKind.ParenthesizedExpression:
                return BindExpression(((ParenthesizedExpressionSyntax)syntax).Expression);
            default:
                throw new Exception($"unexpected syntax {syntax.Kind}");
        }
    }

    private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
    {
        var boundOperand = BindExpression(syntax.Operand);
        var boundOperator = BoundUnaryOperator.Bind(syntax.Operatortoken.Kind, boundOperand.type);
        if (boundOperator == null)
        {
            _diagnostics.ReportUndefinedUnaryOperator(syntax.Operatortoken.span,syntax.Operatortoken.Text,boundOperand.type);
            return boundOperand;
        }
        return new BoundUnaryExpression(boundOperator, boundOperand);
    }


    private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
    {
        var boundLeft = BindExpression(syntax.Left);
        var boundRight = BindExpression(syntax.Right);
        var boundOperator = BoundBinaryOperator.Bind(syntax.Operatortoken.Kind, boundLeft.type, boundRight.type);
        if (boundOperator == null)
        {
            _diagnostics.ReportUndefinedBinaryOperator(syntax.Operatortoken.span,syntax.Operatortoken.Text,boundLeft.type,boundRight.type);
            return boundLeft;
        }
        return new BoundBinaryExpression(boundLeft, boundOperator, boundRight);
    }
    private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
    {
        var value = syntax.Value ?? 0;
        return new BoundLiteralExpression(value);
    }
}





