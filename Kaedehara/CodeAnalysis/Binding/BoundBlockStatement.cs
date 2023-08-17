using System.Collections.Immutable;

namespace Kaedehara.CodeAnalysis.Binding;

internal sealed class BoundBlockStatement : BoundStatement
{
    public BoundBlockStatement(ImmutableArray<BoundStatement> statements){
        statements = statements ;
    }
    public override BoundNodeKind Kind =>  BoundNodeKind.BlockStatement ; 
    public ImmutableArray<BoundStatement> Statements {get;}
}





