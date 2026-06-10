using Kaedehara.CodeAnalysis.Symbols;

namespace Kaedehara.CodeAnalysis.Binding
{
    internal sealed class BoundFunctionDeclaration : BoundStatement
    {
        public BoundFunctionDeclaration(FunctionSymbol function, BoundBlockStatement body)
        {
            Function = function;
            Body = body;
        }

        public override BoundNodeKind Kind => BoundNodeKind.FunctionDeclaration;
        public FunctionSymbol Function { get; }
        public BoundBlockStatement Body { get; }
    }
}
