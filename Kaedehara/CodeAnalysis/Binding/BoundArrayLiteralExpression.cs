using System.Collections.Immutable;
using Kaedehara.CodeAnalysis.Symbols;

namespace Kaedehara.CodeAnalysis.Binding
{
    internal sealed class BoundArrayLiteralExpression : BoundExpression
    {
        public BoundArrayLiteralExpression(TypeSymbol type, ImmutableArray<BoundExpression> elements)
        {
            Type = type;
            Elements = elements;
        }

        public override TypeSymbol Type { get; }
        public override BoundNodeKind Kind => BoundNodeKind.ArrayLiteralExpression;

        public ImmutableArray<BoundExpression> Elements { get; }
    }
}
