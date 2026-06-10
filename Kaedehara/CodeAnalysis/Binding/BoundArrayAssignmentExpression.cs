using Kaedehara.CodeAnalysis.Symbols;

namespace Kaedehara.CodeAnalysis.Binding
{
    internal sealed class BoundArrayAssignmentExpression : BoundExpression
    {
        public BoundArrayAssignmentExpression(BoundExpression array, BoundExpression index, BoundExpression expression)
        {
            Array = array;
            Index = index;
            Expression = expression;
        }

        public override TypeSymbol Type => Expression.Type;
        public override BoundNodeKind Kind => BoundNodeKind.ArrayAssignmentExpression;

        public BoundExpression Array { get; }
        public BoundExpression Index { get; }
        public BoundExpression Expression { get; }
    }
}
