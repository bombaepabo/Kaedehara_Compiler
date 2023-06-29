namespace KAEDEHARA_COMPILER.CodeAnalysis
{
    class Evaluator {
            private readonly ExpressionSyntax _root;

            public Evaluator(ExpressionSyntax root){
                this._root = root;
            }
    public int Evaluate()
    {
        return EvaluateExpression(_root);
    }

    private int EvaluateExpression(ExpressionSyntax node)
    {
                if(node is NumberExpressionSyntax n){
                    return  (int) n.NumberToken.Value;
                }
                if(node is BinaryExpressionSyntax b)
                {
                    var left  = EvaluateExpression(b.Left);
                    var right = EvaluateExpression(b.Right);

                    if(b.Operatortoken.Kind == SyntaxKind.PlusToken){
                        return left + right;
                    }
                    else if(b.Operatortoken.Kind == SyntaxKind.MinusToken){
                        return left - right;                    
                    }
                    else if(b.Operatortoken.Kind == SyntaxKind.StarToken){
                        return left * right;
                    }
                    else if(b.Operatortoken.Kind == SyntaxKind.SlashToken){
                        return left / right;                    
                    }
                    else {
                        throw new Exception($"unexpected binary operator {b.Operatortoken.Kind}");
                    }
                }
                if(node is ParenthesizedExpressionSyntax p){
                    return EvaluateExpression(p.Expression);
                }
                throw new Exception($"unexpected node {node.Kind}");
    }
}







}