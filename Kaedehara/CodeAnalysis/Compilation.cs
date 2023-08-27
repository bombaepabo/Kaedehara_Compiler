using System.Collections.Immutable;
using Kaedehara.CodeAnalysis.Binding;
using Kaedehara.CodeAnalysis.Syntax;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Kaedehara.CodeAnalysis.Lowering;

namespace Kaedehara.CodeAnalysis
{
    public sealed class Compilation
    {
        private BoundGlobalScope _globalScope;
        public Compilation(SyntaxTree syntax)
        : this(null, syntax)
        {
        }
        private Compilation(Compilation previous, SyntaxTree syntax)
        {
            Previous = previous;
            Syntax = syntax;
        }



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
            var statement = GetStatement();
            var evaluator = new Evaluator(statement, variables);
            var value = evaluator.Evaluate();
            return new EvaluationResult(ImmutableArray<Diagnostic>.Empty, value);
        }

        public void EmitTree(TextWriter writer)
        {
            var statement = GetStatement();
            statement.WriteTo(writer);
        }

        private BoundBlockStatement GetStatement()
        {
            var result =  GlobalScope.Statement;
            return Lowerer.Lower(result);
        }
    }
}