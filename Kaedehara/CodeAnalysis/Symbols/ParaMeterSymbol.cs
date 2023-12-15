namespace Kaedehara.CodeAnalysis.Symbols
{
    public sealed class ParaMeterSymbol : VariableSymbol
    {
        public ParaMeterSymbol(string name,TypeSymbol type)
         : base(name,isReadOnly:true,type)
        {
        }

        public override SymbolKind Kind => SymbolKind.Parameter;
    }
}
