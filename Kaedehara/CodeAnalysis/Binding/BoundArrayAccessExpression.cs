using Kaedehara.CodeAnalysis.Symbols;

namespace Kaedehara.CodeAnalysis.Binding
{
    internal sealed class BoundArrayAccessExpression : BoundExpression
    {
        public BoundArrayAccessExpression(BoundExpression array, BoundExpression index)
        {
            Array = array;
            Index = index;
        }

        public override TypeSymbol Type => Array.Type is ArrayTypeSymbol arrayType ? arrayType.ElementType : TypeSymbol.Error;
        public override BoundNodeKind Kind => BoundNodeKind.ArrayAccessExpression;

        public BoundExpression Array { get; }
        public BoundExpression Index { get; }
    }
}
