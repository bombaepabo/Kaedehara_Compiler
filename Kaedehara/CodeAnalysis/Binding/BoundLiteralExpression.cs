using Kaedehara.CodeAnalysis.Binding;

namespace Kaedehara.CodeAnalysis.Binding
{
    internal sealed class BoundLiteralExpression : BoundExpression
    {

        public BoundLiteralExpression(object value)
        {
            Value = value;
        }

        public object Value { get; }

        public override Type type => Value.GetType();

        public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
    }
}