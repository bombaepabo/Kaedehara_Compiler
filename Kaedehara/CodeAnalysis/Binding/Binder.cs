using Kaedehara.CodeAnalysis.Syntax;
using Kaedehara.CodeAnalysis.Syntax;

namespace Kaedehara.CodeAnalysis.Binding;
internal sealed class Binder
{
    private readonly DiagnosticBag _diagnostics = new DiagnosticBag();

    public Binder(Dictionary<VariableSymbol, object> variables)
    {
        Variables = variables;
    }

    public DiagnosticBag Diagnostics => _diagnostics;

    private readonly Dictionary<VariableSymbol, object> Variables;

    public BoundExpression BindExpression(ExpressionSyntax syntax)
    {
        switch (syntax.Kind)
        {
            case SyntaxKind.ParenthesizedExpression:
                return BindParenthesizedExpression(((ParenthesizedExpressionSyntax)syntax));
            case SyntaxKind.LiteralExpression:
                return BindLiteralExpression((LiteralExpressionSyntax)syntax);
            case SyntaxKind.NameExpression:
                return BindNameExpression(((NameExpressionSyntax)syntax));
            case SyntaxKind.AssignmentExpression:
                return BindAssignmentExpression(((AssignmentExpressionSyntax)syntax));
            case SyntaxKind.UnaryExpression:
                return BindUnaryExpression((UnaryExpressionSyntax)syntax);
            case SyntaxKind.BinaryExpression:
                return BindBinaryExpression((BinaryExpressionSyntax)syntax);
            default:
                throw new Exception($"unexpected syntax {syntax.Kind}");
        }
    }


    private BoundExpression BindNameExpression(NameExpressionSyntax syntax)
    {
        var name = syntax.IdentifierToken.Text;
        var variable = Variables.Keys.FirstOrDefault(v => v.Name == name);
        if (variable == null)
        {
            _diagnostics.ReportUndefinedName(syntax.IdentifierToken.span, name);
            return new BoundLiteralExpression(0);
        }
        return new BoundVariableExpression(variable);
    }
    private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax syntax)
    {
        var name = syntax.IdentifierToken.Text;
        var boundExpression = BindExpression(syntax.Expression);
        var existingVariable = Variables.Keys.FirstOrDefault(v => v.Name == name);
        if (existingVariable != null)
        {
            Variables.Remove(existingVariable);
        }
        var variable = new VariableSymbol(name, boundExpression.type);
        Variables[variable] = null;
        return new BoundAssignmentExpression(variable, boundExpression);
    }

    private BoundExpression BindParenthesizedExpression(ParenthesizedExpressionSyntax syntax)
    {
        return BindExpression(syntax.Expression);
    }

    private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
    {
        var boundOperand = BindExpression(syntax.Operand);
        var boundOperator = BoundUnaryOperator.Bind(syntax.Operatortoken.Kind, boundOperand.type);
        if (boundOperator == null)
        {
            _diagnostics.ReportUndefinedUnaryOperator(syntax.Operatortoken.span, syntax.Operatortoken.Text, boundOperand.type);
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
            _diagnostics.ReportUndefinedBinaryOperator(syntax.Operatortoken.span, syntax.Operatortoken.Text, boundLeft.type, boundRight.type);
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





