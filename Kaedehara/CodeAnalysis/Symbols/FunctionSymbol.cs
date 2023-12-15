using System.Collections.Immutable;

namespace Kaedehara.CodeAnalysis.Symbols
{
    public sealed class FunctionSymbol:Symbol{
        public FunctionSymbol(string name,ImmutableArray<ParaMeterSymbol>parameter,TypeSymbol type)
        :base(name)
        {
            Parameter = parameter;
            Type = type;
        }

        public override SymbolKind Kind => SymbolKind.Function;

        public ImmutableArray<ParaMeterSymbol> Parameter { get; }
        public TypeSymbol Type { get; }
    }
}
