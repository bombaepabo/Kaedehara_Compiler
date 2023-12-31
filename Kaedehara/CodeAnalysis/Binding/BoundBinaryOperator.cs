using Kaedehara.CodeAnalysis.Symbols;
using Kaedehara.CodeAnalysis.Syntax;

namespace Kaedehara.CodeAnalysis.Binding;

internal sealed class BoundBinaryOperator
{
    private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeSymbol type)
   : this(syntaxKind, kind, type, type, type)
    {

    }
    private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeSymbol operandType, TypeSymbol resultType)
   : this(syntaxKind, kind, operandType, operandType, resultType)
    {

    }
    private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeSymbol leftType, TypeSymbol rightType, TypeSymbol resultType)
    {
        SyntaxKind = syntaxKind;
        Kind = kind;
        LeftType = leftType;
        RightType = rightType;
        Type = resultType;
    }

    public SyntaxKind SyntaxKind { get; }
    public BoundBinaryOperatorKind Kind { get; }
    public TypeSymbol LeftType { get; }
    public TypeSymbol RightType { get; }
    public TypeSymbol OperandType { get; }
    public TypeSymbol Type { get; }

    private static BoundBinaryOperator[] _operators = {
            new BoundBinaryOperator(SyntaxKind.PlusToken,BoundBinaryOperatorKind.Addition,TypeSymbol.Int),
            new BoundBinaryOperator(SyntaxKind.MinusToken,BoundBinaryOperatorKind.Subtraction,TypeSymbol.Int),
            new BoundBinaryOperator(SyntaxKind.StarToken,BoundBinaryOperatorKind.Multiplication,TypeSymbol.Int),
            new BoundBinaryOperator(SyntaxKind.SlashToken,BoundBinaryOperatorKind.Division,TypeSymbol.Int),
            new BoundBinaryOperator(SyntaxKind.EqualEqualToken,BoundBinaryOperatorKind.Equals,TypeSymbol.Int,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.NotEqualToken,BoundBinaryOperatorKind.NotEquals,TypeSymbol.Int,TypeSymbol.Bool),

            new BoundBinaryOperator(SyntaxKind.AmpersanToken,BoundBinaryOperatorKind.BitwiseAnd,TypeSymbol.Int),
            new BoundBinaryOperator(SyntaxKind.PipeToken,BoundBinaryOperatorKind.BitWiseOr,TypeSymbol.Int),
            new BoundBinaryOperator(SyntaxKind.HatToken,BoundBinaryOperatorKind.BitWiseXOR,TypeSymbol.Int),

            new BoundBinaryOperator(SyntaxKind.LessToken,BoundBinaryOperatorKind.LessThan,TypeSymbol.Int,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.LessOrEqualsToken,BoundBinaryOperatorKind.LessThanOrEqualsTo,TypeSymbol.Int,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.GreatToken,BoundBinaryOperatorKind.GreaterThan,TypeSymbol.Int,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.GreaterOrEqualsToken,BoundBinaryOperatorKind.GreaterOrEqualsTo,TypeSymbol.Int,TypeSymbol.Bool),


            new BoundBinaryOperator(SyntaxKind.AmpersanToken,BoundBinaryOperatorKind.BitwiseAnd,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.AmpersanAmpersanToken,BoundBinaryOperatorKind.LogicalAnd,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.PipeToken,BoundBinaryOperatorKind.BitWiseOr,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.PipePipeToken,BoundBinaryOperatorKind.LogicalOr,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.HatToken,BoundBinaryOperatorKind.BitWiseXOR,TypeSymbol.Bool),

            new BoundBinaryOperator(SyntaxKind.EqualEqualToken,BoundBinaryOperatorKind.Equals,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.NotEqualToken,BoundBinaryOperatorKind.NotEquals,TypeSymbol.Bool),
            
            new BoundBinaryOperator(SyntaxKind.PlusToken,BoundBinaryOperatorKind.Addition,TypeSymbol.String),



        };
    public static BoundBinaryOperator Bind(SyntaxKind syntaxKind, TypeSymbol leftType, TypeSymbol rightType)
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





