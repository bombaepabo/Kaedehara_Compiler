using Kaedehara.CodeAnalysis.Symbols;

namespace Kaedehara.CodeAnalysis.Binding;

internal abstract class BoundExpression : BoundNode
{
    public abstract TypeSymbol Type { get; }
}





