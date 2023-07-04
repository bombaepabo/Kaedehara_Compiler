namespace KAEDEHARA_COMPILER.CodeAnalysis.Binding;

internal sealed class BoundUnaryExpression : BoundExpression
{
    public BoundUnaryExpression(BoundUnaryOperator op, BoundExpression operand)
    {
        Op = op;
        Operand = operand;
    }

    public BoundUnaryOperator Op { get; }
    public BoundExpression Operand { get; }
    public override Type type => Op.Type;


    public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
}





