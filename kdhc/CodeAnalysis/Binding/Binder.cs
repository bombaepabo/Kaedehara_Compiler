using KAEDEHARA_COMPILER.CodeAnalysis.Syntax;
using kdhc.CodeAnalysis.Binding;

namespace KAEDEHARA_COMPILER.CodeAnalysis.Binding;
internal sealed class Binder
{
    private readonly List<string> _diagnostics = new List<string>();
    public IEnumerable<string> Diagnostics => _diagnostics;
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
            _diagnostics.Add($"Unary operator '{syntax.Operatortoken.Text}' is not defined for type {boundOperand.type}.");
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
            _diagnostics.Add($"Binary operator '{syntax.Operatortoken.Text}' is not defined for type {boundLeft.type} and {boundRight.type}.");
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





