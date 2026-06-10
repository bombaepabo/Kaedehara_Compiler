namespace Kaedehara.CodeAnalysis.Symbols
{
    public sealed class ArrayTypeSymbol : TypeSymbol
    {
        public ArrayTypeSymbol(TypeSymbol elementType) : base(elementType.Name + "[]")
        {
            ElementType = elementType;
        }

        public TypeSymbol ElementType { get; }

        public override bool Equals(object obj)
        {
            return obj is ArrayTypeSymbol other && ElementType.Equals(other.ElementType);
        }

        public override int GetHashCode()
        {
            return ElementType.GetHashCode() ^ 31;
        }
    }
}
