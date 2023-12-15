using System.Collections;
using System.Collections.Immutable;
namespace Kaedehara.CodeAnalysis.Syntax
{
    public abstract class SeparatedSyntaxList
    {
    public abstract ImmutableArray<SyntaxNode> GetWithSeperators();

    }
    public sealed class SeparatedSyntaxList<T> : SeparatedSyntaxList,IEnumerable<T>
        where T:SyntaxNode
        {
            private readonly ImmutableArray<SyntaxNode> _nodeAndSeperators;
            public SeparatedSyntaxList(ImmutableArray<SyntaxNode> nodeAndSeperators)
            {
                _nodeAndSeperators = nodeAndSeperators;
            }
            public int Count => (_nodeAndSeperators.Length + 1) / 2;
            public T this[int index] =>(T) _nodeAndSeperators[index * 2];
            public SyntaxToken GetSeparator(int index){
                if (index == Count - 1)
                    return null;
                
                return (SyntaxToken) _nodeAndSeperators[index *2 + 1];
            }

        public override ImmutableArray<SyntaxNode> GetWithSeperators() =>_nodeAndSeperators;

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0 ; i <Count;i++){
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
           return GetEnumerator();
        }
    }
}