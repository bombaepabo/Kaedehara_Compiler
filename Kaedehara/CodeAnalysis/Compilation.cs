using System.Collections.Immutable;
using Kaedehara.CodeAnalysis.Binding;
using Kaedehara.CodeAnalysis.Syntax;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

namespace Kaedehara.CodeAnalysis
{
    public sealed class Compilation
    {
        public Compilation(SyntaxTree syntax)
        : this(null, syntax)
        {
            Syntax = syntax;
        }
        private Compilation(Compilation previous, SyntaxTree syntax)
        {
            Previous = previous;
            Syntax = syntax;
        }


        private BoundGlobalScope _globalScope;

        public Compilation Previous { get; }
        public SyntaxTree Syntax { get; }
        internal BoundGlobalScope GlobalScope
        {
            get
            {
                if (_globalScope == null)
                {
                    var globalScope = Binder.BindGlobalScope(Previous?.GlobalScope, Syntax.Root);
                    Interlocked.CompareExchange(ref _globalScope, globalScope, null);
                }
                return _globalScope;
            }
        }
        public Compilation ContinuedWith(SyntaxTree syntax)
        {
            return new Compilation(this, syntax);
        }
        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            var diagnostics = Syntax.Diagnostics.Concat(GlobalScope.Diagnostics).ToImmutableArray();
            if (diagnostics.Any())
            {
                return new EvaluationResult(diagnostics, null);
            }
            var evaluator = new Evaluator(GlobalScope.Statement, variables);
            var value = evaluator.Evaluate();
            return new EvaluationResult(ImmutableArray<Diagnostic>.Empty, value);
        }

    }
}