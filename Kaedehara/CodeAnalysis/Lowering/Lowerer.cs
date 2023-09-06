using System.Collections.Immutable;
using Kaedehara.CodeAnalysis.Binding;
using Kaedehara.CodeAnalysis.Syntax;

namespace Kaedehara.CodeAnalysis.Lowering
{
    internal sealed class Lowerer : BoundTreeRewriter{
      private int _labelCount ;
     private Lowerer(){

     }
     private LabelSymbol GenerateLabel(){
      var name = $"Label{++_labelCount}";
      return new LabelSymbol(name);
     }
     public static BoundBlockStatement Lower(BoundStatement statement){
        var lowerer = new Lowerer();
        var result = lowerer.RewriteStatement(statement);
        return Flatten(result);
     }
     private static BoundBlockStatement Flatten(BoundStatement statement){

      var builder = ImmutableArray.CreateBuilder<BoundStatement>();
      var stack = new Stack<BoundStatement>();
      stack.Push(statement);

            while (stack.Count > 0)
            {
                var current = stack.Pop();

                if (current is BoundBlockStatement block)
                {
                    foreach (var s in block.Statements.Reverse())
                        stack.Push(s);
                }
                else
                {
                    builder.Add(current);
                }
            }

            return new BoundBlockStatement(builder.ToImmutable());
     }
        protected override BoundStatement RewriteIfStatement(BoundIfStatement node)
        {
            if(node.ElseStatement == null){
               var endLabel = GenerateLabel();
               var goToFalse = new BoundConditionalGoToStatement(endLabel,node.Condition,false);
               var endLabelStatement = new BoundLabelStatement(endLabel);
               var result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(goToFalse,node.ThenStatement,endLabelStatement));
               return RewriteStatement(result);
            }
            else{
               var elseLabel = GenerateLabel();
               var endLabel = GenerateLabel();
               var goToFalse = new BoundConditionalGoToStatement(elseLabel,node.Condition,false);
               var goToEndStatement = new BoundGoToStatement(endLabel);
               var elseLabelStatement = new BoundLabelStatement(elseLabel);
               var endLabelStatement = new BoundLabelStatement(endLabel);
               var result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(goToFalse,
                                                                                          node.ThenStatement,
                                                                                          goToEndStatement,
                                                                                          elseLabelStatement,
                                                                                          node.ElseStatement,
                                                                                          endLabelStatement));
               return RewriteStatement(result);


            }
        }
        protected override BoundStatement RewriteWhileStatement(BoundWhileStatement node)
        {
            var continueLabel = GenerateLabel();
            var checkLabel = GenerateLabel();
            var endLabel = GenerateLabel();

            var gotoCheck = new BoundGoToStatement(checkLabel);
            var continueLabelStatement = new BoundLabelStatement(continueLabel);
            var checkLabelStatement = new BoundLabelStatement(checkLabel);
            var gotoTrue = new BoundConditionalGoToStatement(continueLabel, node.Condition);
            var endLabelStatement = new BoundLabelStatement(endLabel);

            var result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                gotoCheck,
                continueLabelStatement,
                node.Body,
                checkLabelStatement,
                gotoTrue,
                endLabelStatement
            ));

            return RewriteStatement(result);

        }
        protected override BoundStatement RewriteForStatement(BoundForStatement node)
        {
         var VariableDeclaration = new BoundVariableDeclaration(node.Variable,node.LowerBound);
         var VariableExpression = new BoundVariableExpression(node.Variable);
         var upperBoundSymbol = new VariableSymbol("upperBound",true,typeof(int));
         var upperBoundDeclaration = new BoundVariableDeclaration(upperBoundSymbol,node.UpperBound);
         var condition =    new BoundBinaryExpression( 
                            VariableExpression,
                            BoundBinaryOperator.Bind(SyntaxKind.LessOrEqualsToken,typeof(int),typeof(int)),
                           new BoundVariableExpression(upperBoundSymbol));
        var increment = new BoundExpressionStatement(
                            new BoundAssignmentExpression(
                            node.Variable,
                            new BoundBinaryExpression(
                            VariableExpression,
                            BoundBinaryOperator.Bind(SyntaxKind.PlusToken,typeof(int),typeof(int)),
                            new BoundLiteralExpression(1)
            )
         )
         );
         var whileBlock = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(node.Body,increment));
         var whileStatement = new BoundWhileStatement(condition,whileBlock);
         var result = new BoundBlockStatement(
            ImmutableArray.Create<BoundStatement>(VariableDeclaration,upperBoundDeclaration,whileStatement));
         return RewriteStatement(result) ;

        }

    
   
}
}
