using KAEDEHARA_COMPILER.CodeAnalysis.Binding;
namespace KAEDEHARA_COMPILER.CodeAnalysis
{
    internal sealed class Evaluator
    {
        private readonly BoundExpression _root;

        public Evaluator(BoundExpression root)
        {
            this._root = root;
        }
        public object Evaluate()
        {
            return EvaluateExpression(_root);
        }

        private object EvaluateExpression(BoundExpression node)
        {
            if (node is BoundLiteralExpression n)
            {
                return n.Value;
            }
            if (node is BoundUnaryExpression u){
                var operand =  EvaluateExpression(u.Operand);
                switch (u.Op.Kind)
                {
                    case BoundUnaryOperatorKind.identity:
                        return (int)operand;
                    case BoundUnaryOperatorKind.Negation:
                        return -(int)operand;
                     case BoundUnaryOperatorKind.LogicalNegation:
                        return !(bool)operand;
                    default:
                        throw new Exception($"unexpected unary operator {u.Op.Kind}");
                }

            }
            if (node is BoundBinaryExpression b)
            {
                var left =  EvaluateExpression(b.Left);
                var right = EvaluateExpression(b.Right);

                switch (b.Op.Kind)
                {
                    case BoundBinaryOperatorKind.Addition:
                        return (int) left + (int)right;
                    case BoundBinaryOperatorKind.Subtraction:
                        return (int) left - (int)right;
                    case BoundBinaryOperatorKind.Multiplication:
                        return (int) left * (int)right;
                    case BoundBinaryOperatorKind.Division:
                        return (int) left / (int)right;
                    case BoundBinaryOperatorKind.LogicalAnd:
                        return (bool) left && (bool)right;
                    case BoundBinaryOperatorKind.LogicalOr:
                        return (bool) left || (bool)right;
                    case BoundBinaryOperatorKind.Equals:
                        return Equals(left,right);
                     case BoundBinaryOperatorKind.NotEquals:
                        return !Equals(left,right);

                    default:
                        throw new Exception($"unexpected binary operator {b.Op.Kind}");
                }
            }
            throw new Exception($"unexpected node {node.Kind}");
        }
    }







}