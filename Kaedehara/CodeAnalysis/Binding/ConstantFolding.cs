using System;
using Kaedehara.CodeAnalysis.Symbols;

namespace Kaedehara.CodeAnalysis.Binding
{
    internal static class ConstantFolding
    {
        public static BoundExpression Fold(BoundUnaryOperator op, BoundExpression operand)
        {
            if (operand is BoundLiteralExpression literal)
            {
                var val = literal.Value;
                try
                {
                    switch (op.Kind)
                    {
                        case BoundUnaryOperatorKind.identity:
                            if (op.Type == TypeSymbol.Float)
                                return new BoundLiteralExpression(Convert.ToDouble(val));
                            return new BoundLiteralExpression(Convert.ToInt32(val));
                        case BoundUnaryOperatorKind.Negation:
                            if (op.Type == TypeSymbol.Float)
                                return new BoundLiteralExpression(-Convert.ToDouble(val));
                            return new BoundLiteralExpression(-Convert.ToInt32(val));
                        case BoundUnaryOperatorKind.LogicalNegation:
                            return new BoundLiteralExpression(!Convert.ToBoolean(val));
                        case BoundUnaryOperatorKind.OnesComplement:
                            return new BoundLiteralExpression(~Convert.ToInt32(val));
                    }
                }
                catch
                {
                    // Fallback if folding fails (e.g. overflow)
                }
            }

            return new BoundUnaryExpression(op, operand);
        }

        public static BoundExpression Fold(BoundExpression left, BoundBinaryOperator op, BoundExpression right)
        {
            if (left is BoundLiteralExpression leftLiteral && right is BoundLiteralExpression rightLiteral)
            {
                var l = leftLiteral.Value;
                var r = rightLiteral.Value;
                try
                {
                    switch (op.Kind)
                    {
                        case BoundBinaryOperatorKind.Addition:
                            if (op.Type == TypeSymbol.Int)
                                return new BoundLiteralExpression(Convert.ToInt32(l) + Convert.ToInt32(r));
                            else if (op.Type == TypeSymbol.Float)
                                return new BoundLiteralExpression(Convert.ToDouble(l) + Convert.ToDouble(r));
                            else
                                return new BoundLiteralExpression(Convert.ToString(l) + Convert.ToString(r));
                        case BoundBinaryOperatorKind.Subtraction:
                            if (op.LeftType == TypeSymbol.Float)
                                return new BoundLiteralExpression(Convert.ToDouble(l) - Convert.ToDouble(r));
                            return new BoundLiteralExpression(Convert.ToInt32(l) - Convert.ToInt32(r));
                        case BoundBinaryOperatorKind.Multiplication:
                            if (op.LeftType == TypeSymbol.Float)
                                return new BoundLiteralExpression(Convert.ToDouble(l) * Convert.ToDouble(r));
                            return new BoundLiteralExpression(Convert.ToInt32(l) * Convert.ToInt32(r));
                        case BoundBinaryOperatorKind.Division:
                            if (op.LeftType == TypeSymbol.Float)
                                return new BoundLiteralExpression(Convert.ToDouble(l) / Convert.ToDouble(r));
                            return new BoundLiteralExpression(Convert.ToInt32(l) / Convert.ToInt32(r));

                        case BoundBinaryOperatorKind.BitwiseAnd:
                            if (op.Type == TypeSymbol.Int)
                                return new BoundLiteralExpression(Convert.ToInt32(l) & Convert.ToInt32(r));
                            else
                                return new BoundLiteralExpression(Convert.ToBoolean(l) & Convert.ToBoolean(r));
                        case BoundBinaryOperatorKind.BitWiseOr:
                            if (op.Type == TypeSymbol.Int)
                                return new BoundLiteralExpression(Convert.ToInt32(l) | Convert.ToInt32(r));
                            else
                                return new BoundLiteralExpression(Convert.ToBoolean(l) | Convert.ToBoolean(r));
                        case BoundBinaryOperatorKind.BitWiseXOR:
                            if (op.Type == TypeSymbol.Int)
                                return new BoundLiteralExpression(Convert.ToInt32(l) ^ Convert.ToInt32(r));
                            else
                                return new BoundLiteralExpression(Convert.ToBoolean(l) ^ Convert.ToBoolean(r));

                        case BoundBinaryOperatorKind.LogicalAnd:
                            return new BoundLiteralExpression(Convert.ToBoolean(l) && Convert.ToBoolean(r));
                        case BoundBinaryOperatorKind.LogicalOr:
                            return new BoundLiteralExpression(Convert.ToBoolean(l) || Convert.ToBoolean(r));
                        case BoundBinaryOperatorKind.Equals:
                            return new BoundLiteralExpression(Equals(l, r));
                        case BoundBinaryOperatorKind.NotEquals:
                            return new BoundLiteralExpression(!Equals(l, r));

                        case BoundBinaryOperatorKind.LessThan:
                            if (op.LeftType == TypeSymbol.Float)
                                return new BoundLiteralExpression(Convert.ToDouble(l) < Convert.ToDouble(r));
                            else if (op.LeftType == TypeSymbol.Char)
                                return new BoundLiteralExpression(Convert.ToChar(l) < Convert.ToChar(r));
                            return new BoundLiteralExpression(Convert.ToInt32(l) < Convert.ToInt32(r));
                        case BoundBinaryOperatorKind.LessThanOrEqualsTo:
                            if (op.LeftType == TypeSymbol.Float)
                                return new BoundLiteralExpression(Convert.ToDouble(l) <= Convert.ToDouble(r));
                            else if (op.LeftType == TypeSymbol.Char)
                                return new BoundLiteralExpression(Convert.ToChar(l) <= Convert.ToChar(r));
                            return new BoundLiteralExpression(Convert.ToInt32(l) <= Convert.ToInt32(r));
                        case BoundBinaryOperatorKind.GreaterThan:
                            if (op.LeftType == TypeSymbol.Float)
                                return new BoundLiteralExpression(Convert.ToDouble(l) > Convert.ToDouble(r));
                            else if (op.LeftType == TypeSymbol.Char)
                                return new BoundLiteralExpression(Convert.ToChar(l) > Convert.ToChar(r));
                            return new BoundLiteralExpression(Convert.ToInt32(l) > Convert.ToInt32(r));
                        case BoundBinaryOperatorKind.GreaterOrEqualsTo:
                            if (op.LeftType == TypeSymbol.Float)
                                return new BoundLiteralExpression(Convert.ToDouble(l) >= Convert.ToDouble(r));
                            else if (op.LeftType == TypeSymbol.Char)
                                return new BoundLiteralExpression(Convert.ToChar(l) >= Convert.ToChar(r));
                            return new BoundLiteralExpression(Convert.ToInt32(l) >= Convert.ToInt32(r));
                    }
                }
                catch
                {
                    // Fallback
                }
            }

            return new BoundBinaryExpression(left, op, right);
        }

        public static BoundExpression Fold(TypeSymbol targetType, BoundExpression expression)
        {
            if (expression is BoundLiteralExpression literal)
            {
                var val = literal.Value;
                try
                {
                    if (targetType == TypeSymbol.Bool)
                        return new BoundLiteralExpression(Convert.ToBoolean(val));
                    else if (targetType == TypeSymbol.Int)
                    {
                        if (val is double d)
                            return new BoundLiteralExpression((int)d);
                        return new BoundLiteralExpression(Convert.ToInt32(val));
                    }
                    else if (targetType == TypeSymbol.String)
                        return new BoundLiteralExpression(Convert.ToString(val));
                    else if (targetType == TypeSymbol.Float)
                        return new BoundLiteralExpression(Convert.ToDouble(val));
                    else if (targetType == TypeSymbol.Char)
                    {
                        if (val is string s && s.Length > 0)
                            return new BoundLiteralExpression(s[0]);
                        return new BoundLiteralExpression(Convert.ToChar(val));
                    }
                }
                catch
                {
                    // Fallback
                }
            }

            return new BoundConversionExpression(targetType, expression);
        }
    }
}
