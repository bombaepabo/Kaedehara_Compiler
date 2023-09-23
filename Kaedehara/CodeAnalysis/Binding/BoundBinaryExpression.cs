using Kaedehara.CodeAnalysis.Symbols;

namespace Kaedehara.CodeAnalysis.Binding;
internal sealed partial class BoundBinaryExpression : BoundExpression
{
    public BoundBinaryExpression(BoundExpression left, BoundBinaryOperator op, BoundExpression right)
    {
        Left = left;
        Op = op;
        Right = right;
    }

    public BoundBinaryOperator Op { get; }
    public BoundExpression Right { get; }
    public BoundExpression Left { get; }
    public override BoundNodeKind Kind => BoundNodeKind.BinaryExpression;
    public override TypeSymbol Type => Op.Type;
}





