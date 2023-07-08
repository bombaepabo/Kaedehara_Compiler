namespace KAEDEHARA_COMPILER.CodeAnalysis
{
    public sealed class VariableSymbol {
        internal VariableSymbol(string name,Type type){
            Name = name;
            Type = type;
        }

        public string Name { get; }
        public Type Type { get; }
    }







}