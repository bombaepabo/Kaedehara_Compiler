using System.Collections.Immutable;
using System.Collections.Generic;

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
        internal Binding.BoundBlockStatement Body { get; set; }
        internal HashSet<VariableSymbol> Locals { get; set; }
    }
}
