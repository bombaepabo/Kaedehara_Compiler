namespace Kaedehara.CodeAnalysis.Symbols
{

    public class TypeSymbol : Symbol
    {
        public static readonly TypeSymbol Error = new TypeSymbol("?");
        public static readonly TypeSymbol Int = new TypeSymbol("int");
        public static readonly TypeSymbol Bool = new TypeSymbol("bool");
        public static readonly TypeSymbol String = new TypeSymbol("string");
        public static readonly TypeSymbol Void = new TypeSymbol("void");
        public static readonly TypeSymbol Float = new TypeSymbol("float");
        public static readonly TypeSymbol Char = new TypeSymbol("char");

        protected TypeSymbol(string name)
        : base(name)
        {
        }

        public override SymbolKind Kind => SymbolKind.Type;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null || obj.GetType() != this.GetType()) return false;
            return Name == ((TypeSymbol)obj).Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static bool operator ==(TypeSymbol left, TypeSymbol right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(TypeSymbol left, TypeSymbol right)
        {
            return !(left == right);
        }
    }
}
