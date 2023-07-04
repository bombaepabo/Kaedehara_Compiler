using KAEDEHARA_COMPILER.CodeAnalysis.Binding;
using KAEDEHARA_COMPILER.CodeAnalysis.Syntax;
namespace KAEDEHARA_COMPILER.CodeAnalysis
{
    public sealed class Compilation {
        public Compilation(SyntaxTree syntax){
            Syntax = syntax;
        }

        public SyntaxTree Syntax { get; }
        public EvaluationResult Evaluate(){
            var binder = new Binder();
            var boundExpression = binder.BindExpression(Syntax.Root);
            var diagnostics = Syntax.Diagnostics.Concat(binder.Diagnostics).ToArray();
            if(diagnostics.Any()){
                return new EvaluationResult(diagnostics,null);
            }
            var evaluator = new Evaluator(boundExpression);
            var value = evaluator.Evaluate();
            return new EvaluationResult(Array.Empty<Diagnostic>(),value);
        }

    }
}