using KAEDEHARA_COMPILER.CodeAnalysis.Syntax;

namespace KAEDEHARA_COMPILER.CodeAnalysis.Binding;

internal sealed class BoundBinaryOperator
{
    private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, Type type)
   : this(syntaxKind, kind, type, type, type)
    {

    }
    private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, Type operandType, Type resultType)
   : this(syntaxKind, kind, operandType, operandType, resultType)
    {

    }
    private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, Type leftType, Type rightType, Type resultType)
    {
        SyntaxKind = syntaxKind;
        Kind = kind;
        LeftType = leftType;
        RightType = rightType;
        Type = resultType;
    }

    public SyntaxKind SyntaxKind { get; }
    public BoundBinaryOperatorKind Kind { get; }
    public Type LeftType { get; }
    public Type RightType { get; }
    public Type OperandType { get; }
    public Type Type { get; }

    private static BoundBinaryOperator[] _operators = {
            new BoundBinaryOperator(SyntaxKind.PlusToken,BoundBinaryOperatorKind.Addition,typeof(int)),
            new BoundBinaryOperator(SyntaxKind.MinusToken,BoundBinaryOperatorKind.Subtraction,typeof(int)),
            new BoundBinaryOperator(SyntaxKind.StarToken,BoundBinaryOperatorKind.Multiplication,typeof(int)),
            new BoundBinaryOperator(SyntaxKind.SlashToken,BoundBinaryOperatorKind.Division,typeof(int)),
            new BoundBinaryOperator(SyntaxKind.AmpersanToken,BoundBinaryOperatorKind.LogicalAnd,typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.PipeToken,BoundBinaryOperatorKind.LogicalOr,typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.EqualToken,BoundBinaryOperatorKind.Equals,typeof(int),typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.NotEqualToken,BoundBinaryOperatorKind.NotEquals,typeof(int),typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.EqualToken,BoundBinaryOperatorKind.Equals,typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.NotEqualToken,BoundBinaryOperatorKind.NotEquals,typeof(bool)),

        };
    public static BoundBinaryOperator Bind(SyntaxKind syntaxKind, Type leftType, Type rightType)
    {
        foreach (var op in _operators)
        {
            if (op.SyntaxKind == syntaxKind && op.LeftType == leftType && op.RightType == rightType)
            {
                return op;
            }

        }
        return null;
    }
}




