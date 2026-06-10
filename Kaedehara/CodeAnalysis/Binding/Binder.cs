using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Kaedehara.CodeAnalysis.Symbols;
using Kaedehara.CodeAnalysis.Syntax;

namespace Kaedehara.CodeAnalysis.Binding
{
    internal sealed class Binder
    {
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();
        private BoundScope _scope;
        private FunctionSymbol _currentFunction;

        public Binder(BoundScope parent)
        {
            _scope = new BoundScope(parent);
        }

        public static BoundGlobalScope BindGlobalScope(BoundGlobalScope previous, CompilationUnitSyntax syntax)
        {
            var parentScope = CreateParentScopes(previous);
            var binder = new Binder(parentScope);
            var expression = binder.BindStatement(syntax.Statement);
            var variables = binder._scope.GetDeclaredVariables();
            var functions = binder._scope.GetDeclaredFunctions();
            var diagnostics = binder.Diagnostics.ToImmutableArray();

            if (previous != null)
            {
                diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);
            }
            return new BoundGlobalScope(previous, diagnostics, functions, variables, expression);
        }

        private static BoundScope CreateParentScopes(BoundGlobalScope previous)
        {
            var stack = new Stack<BoundGlobalScope>();
            while (previous != null)
            {
                stack.Push(previous);
                previous = previous.Previous;
            }
            var parent = CreateRootScope();
            while (stack.Count > 0)
            {
                previous = stack.Pop();
                var scope = new BoundScope(parent);
                foreach (var f in previous.Functions)
                {
                    scope.TryDeclareFunction(f);
                }
                foreach (var v in previous.Variables)
                {
                    scope.TryDeclareVariable(v);
                }
                parent = scope;
            }
            return parent;
        }

        private static BoundScope CreateRootScope()
        {
            var result = new BoundScope(null);
            foreach (var f in BuiltinFunctions.GetAll())
                result.TryDeclareFunction(f);
            return result;
        }

        public DiagnosticBag Diagnostics => _diagnostics;

        private BoundStatement BindStatement(StatementSyntax syntax)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.BlockStatement:
                    return BindBlockStatement((BlockStatementSyntax)syntax);
                case SyntaxKind.VariableDeclaration:
                    return BindVariableDeclaration((VariableDeclarationSyntax)syntax);
                case SyntaxKind.IfStatement:
                    return BindIfStatement((IfStatementSyntax)syntax);
                case SyntaxKind.WhileStatement:
                    return BindWhileStatement((WhileStatementSyntax)syntax);
                case SyntaxKind.ForStatement:
                    return BindForStatement((ForStatementSyntax)syntax);
                case SyntaxKind.FunctionDeclaration:
                    return BindFunctionDeclaration((FunctionDeclarationSyntax)syntax);
                case SyntaxKind.ReturnStatement:
                    return BindReturnStatement((ReturnStatementSyntax)syntax);
                case SyntaxKind.ExpressionStatement:
                    return BindExpressionStatement((ExpressionStatementSyntax)syntax);
                default:
                    throw new Exception($"unexpected syntax {syntax.Kind}");
            }
        }

        private BoundStatement BindForStatement(ForStatementSyntax syntax)
        {
            var lowerBound = BindExpression(syntax.Lowerbound, TypeSymbol.Int);
            var upperBound = BindExpression(syntax.Upperbound, TypeSymbol.Int);

            _scope = new BoundScope(_scope);

            var variable = BindVariable(syntax.Identifier, isReadOnly: true, TypeSymbol.Int);

            var body = BindStatement(syntax.Body);

            _scope = _scope.Parent;

            return new BoundForStatement(variable, lowerBound, upperBound, body);
        }

        private VariableSymbol BindVariable(SyntaxToken identifier, bool isReadOnly, TypeSymbol type)
        {
            var name = identifier.Text ?? "?";
            var declare = !identifier.IsMissing;
            var variable = new VariableSymbol(name, isReadOnly, type);
            if (declare && !_scope.TryDeclareVariable(variable))
                _diagnostics.ReportVariableAlreadyDeclared(identifier.Span, name);
            return variable;
        }

        private BoundStatement BindWhileStatement(WhileStatementSyntax syntax)
        {
            var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            var body = BindStatement(syntax.Body);
            return new BoundWhileStatement(condition, body);
        }

        private BoundStatement BindIfStatement(IfStatementSyntax syntax)
        {
            var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            var thenStatement = BindStatement(syntax.ThenStatement);
            var elseStatement = syntax.ElseClause == null ? null : BindStatement(syntax.ElseClause.ElseStatement);
            return new BoundIfStatement(condition, thenStatement, elseStatement);
        }

        private BoundExpression BindExpression(ExpressionSyntax syntax, TypeSymbol targetType)
        {
            var result = BindExpression(syntax);
            if (targetType != TypeSymbol.Error && result.Type != TypeSymbol.Error && result.Type != targetType)
            {
                _diagnostics.ReportCannotConvert(syntax.Span, result.Type, targetType);
            }
            return result;
        }

        private BoundStatement BindBlockStatement(BlockStatementSyntax syntax)
        {
            var statements = ImmutableArray.CreateBuilder<BoundStatement>();
            _scope = new BoundScope(_scope);
            foreach (var statementSyntax in syntax.Statements)
            {
                var statement = BindStatement(statementSyntax);
                statements.Add(statement);
            }
            _scope = _scope.Parent;
            return new BoundBlockStatement(statements.ToImmutable());
        }

        private BoundStatement BindVariableDeclaration(VariableDeclarationSyntax syntax)
        {
            var isReadOnly = syntax.Keyword.Kind == SyntaxKind.LetKeyword;
            var initializer = BindExpression(syntax.Initializer);
            var type = BindTypeClause(syntax.TypeClause);
            
            var variableType = type ?? initializer.Type;
            if (type != null && initializer.Type != TypeSymbol.Error && type != TypeSymbol.Error)
            {
                if (initializer.Type != type)
                {
                    var conversion = Conversion.Classify(initializer.Type, type);
                    if (!conversion.Exists)
                    {
                        _diagnostics.ReportCannotConvert(syntax.Initializer.Span, initializer.Type, type);
                    }
                    else if (!conversion.IsIdentity)
                    {
                        initializer = new BoundConversionExpression(type, initializer);
                    }
                }
            }

            var variable = BindVariable(syntax.Identifer, isReadOnly, variableType);
            return new BoundVariableDeclaration(variable, initializer);
        }

        private BoundStatement BindExpressionStatement(ExpressionStatementSyntax syntax)
        {
            var expression = BindExpression(syntax.Expression, canBeVoid: true);
            return new BoundExpressionStatement(expression);
        }

        private BoundExpression BindExpression(ExpressionSyntax syntax, bool canBeVoid = false)
        {
            var result = BindExpressionInternal(syntax);
            if (!canBeVoid && result.Type == TypeSymbol.Void)
            {
                _diagnostics.ReportExpressionMustHaveValue(syntax.Span);
                return new BoundErrorExpression();
            }
            return result;
        }

        private BoundExpression BindExpressionInternal(ExpressionSyntax syntax)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.ParenthesizedExpression:
                    return BindParenthesizedExpression((ParenthesizedExpressionSyntax)syntax);
                case SyntaxKind.LiteralExpression:
                    return BindLiteralExpression((LiteralExpressionSyntax)syntax);
                case SyntaxKind.NameExpression:
                    return BindNameExpression((NameExpressionSyntax)syntax);
                case SyntaxKind.AssignmentExpression:
                    return BindAssignmentExpression((AssignmentExpressionSyntax)syntax);
                case SyntaxKind.UnaryExpression:
                    return BindUnaryExpression((UnaryExpressionSyntax)syntax);
                case SyntaxKind.BinaryExpression:
                    return BindBinaryExpression((BinaryExpressionSyntax)syntax);
                case SyntaxKind.CallExpression:
                    return BindCallExpression((CallExpressionSyntax)syntax);
                case SyntaxKind.ArrayLiteralExpression:
                    return BindArrayLiteralExpression((ArrayLiteralExpressionSyntax)syntax);
                case SyntaxKind.ArrayAccessExpression:
                    return BindArrayAccessExpression((ArrayAccessExpressionSyntax)syntax);
                default:
                    throw new Exception($"unexpected syntax {syntax.Kind}");
            }
        }

        private BoundExpression BindCallExpression(CallExpressionSyntax syntax)
        {
            if (syntax.Arguments.Count == 1 && LookupType(syntax.Identifer.Text) is TypeSymbol type)
            {
                return BindConversion(type, syntax.Arguments[0]);
            }
            var boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();
            foreach (var argument in syntax.Arguments)
            {
                var boundArgument = BindExpression(argument);
                boundArguments.Add(boundArgument);
            }
            if (!_scope.TryLookupFunction(syntax.Identifer.Text, out var function))
            {
                _diagnostics.ReportUndefinedFunction(syntax.Identifer.Span, syntax.Identifer.Text);
                return new BoundErrorExpression();
            }
            if (syntax.Arguments.Count != function.Parameter.Length)
            {
                _diagnostics.ReportWrongArgumentCount(syntax.Span, function.Name, function.Parameter.Length, syntax.Arguments.Count);
                return new BoundErrorExpression();
            }
            for (var i = 0; i < syntax.Arguments.Count; i++)
            {
                var argument = boundArguments[i];
                var parameter = function.Parameter[i];
                if (argument.Type != parameter.Type)
                {
                    var conversion = Conversion.Classify(argument.Type, parameter.Type);
                    if (conversion.IsImplicit)
                    {
                        boundArguments[i] = new BoundConversionExpression(parameter.Type, argument);
                    }
                    else
                    {
                        _diagnostics.ReportWrongArgumentType(syntax.Span, parameter.Name, parameter.Type, argument.Type);
                        return new BoundErrorExpression();
                    }
                }
            }
            return new BoundCallExpression(function, boundArguments.ToImmutable());
        }

        private BoundExpression BindConversion(TypeSymbol type, ExpressionSyntax syntax)
        {
            var expression = BindExpression(syntax);
            var conversion = Conversion.Classify(expression.Type, type);
            if (!conversion.Exists)
            {
                _diagnostics.ReportCannotConvert(syntax.Span, expression.Type, type);
                return new BoundErrorExpression();
            }
            return ConstantFolding.Fold(type, expression);
        }

        private TypeSymbol LookupType(string name)
        {
            switch (name)
            {
                case "bool":
                    return TypeSymbol.Bool;
                case "int":
                    return TypeSymbol.Int;
                case "string":
                    return TypeSymbol.String;
                case "float":
                    return TypeSymbol.Float;
                case "char":
                    return TypeSymbol.Char;
                default:
                    return null;
            }
        }

        private TypeSymbol BindTypeClause(TypeClauseSyntax syntax)
        {
            if (syntax == null)
                return null;

            var type = LookupType(syntax.Identifier.Text);
            if (type == null)
            {
                _diagnostics.ReportUndefinedType(syntax.Identifier.Span, syntax.Identifier.Text);
                return TypeSymbol.Error;
            }

            if (syntax.OpenBracketToken != null)
            {
                type = new ArrayTypeSymbol(type);
            }

            return type;
        }

        private BoundExpression BindNameExpression(NameExpressionSyntax syntax)
        {
            var name = syntax.IdentifierToken.Text;
            if (syntax.IdentifierToken.IsMissing)
            {
                return new BoundErrorExpression();
            }
            if (!_scope.TryLookupVariable(name, out var variable))
            {
                _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return new BoundErrorExpression();
            }
            return new BoundVariableExpression(variable);
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax syntax)
        {
            var left = BindExpression(syntax.Left);
            var boundExpression = BindExpression(syntax.Expression);

            if (left is BoundVariableExpression variableExpression)
            {
                var variable = variableExpression.Variable;
                if (variable.IsReadOnly)
                {
                    _diagnostics.ReportCannotAssign(syntax.EqualsToken.Span, variable.Name);
                }
                if (boundExpression.Type != variable.Type)
                {
                    var conversion = Conversion.Classify(boundExpression.Type, variable.Type);
                    if (!conversion.Exists)
                    {
                        _diagnostics.ReportCannotConvert(syntax.Expression.Span, boundExpression.Type, variable.Type);
                        return boundExpression;
                    }
                    if (!conversion.IsIdentity)
                    {
                        boundExpression = new BoundConversionExpression(variable.Type, boundExpression);
                    }
                }
                return new BoundAssignmentExpression(variable, boundExpression);
            }
            else if (left is BoundArrayAccessExpression arrayAccessExpression)
            {
                var elementType = arrayAccessExpression.Type;
                if (elementType == TypeSymbol.Error)
                {
                    return boundExpression;
                }

                if (boundExpression.Type != elementType)
                {
                    var conversion = Conversion.Classify(boundExpression.Type, elementType);
                    if (!conversion.Exists)
                    {
                        _diagnostics.ReportCannotConvert(syntax.Expression.Span, boundExpression.Type, elementType);
                        return boundExpression;
                    }
                    if (!conversion.IsIdentity)
                    {
                        boundExpression = new BoundConversionExpression(elementType, boundExpression);
                    }
                }
                return new BoundArrayAssignmentExpression(arrayAccessExpression.Array, arrayAccessExpression.Index, boundExpression);
            }
            else
            {
                if (left.Type != TypeSymbol.Error)
                {
                    _diagnostics.ReportCannotAssign(syntax.EqualsToken.Span);
                }
                return boundExpression;
            }
        }

        private BoundExpression BindParenthesizedExpression(ParenthesizedExpressionSyntax syntax)
        {
            return BindExpression(syntax.Expression);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
        {
            var boundOperand = BindExpression(syntax.Operand);
            if (boundOperand.Type == TypeSymbol.Error)
            {
                return new BoundErrorExpression();
            }
            var boundOperator = BoundUnaryOperator.Bind(syntax.Operatortoken.Kind, boundOperand.Type);
            if (boundOperator == null)
            {
                _diagnostics.ReportUndefinedUnaryOperator(syntax.Operatortoken.Span, syntax.Operatortoken.Text, boundOperand.Type);
                return new BoundErrorExpression();
            }
            return ConstantFolding.Fold(boundOperator, boundOperand);
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
        {
            var boundLeft = BindExpression(syntax.Left);
            var boundRight = BindExpression(syntax.Right);
            if (boundLeft.Type == TypeSymbol.Error || boundRight.Type == TypeSymbol.Error)
            {
                return new BoundErrorExpression();
            }

            var leftType = boundLeft.Type;
            var rightType = boundRight.Type;
            if (leftType != rightType)
            {
                var leftToRight = Conversion.Classify(leftType, rightType);
                if (leftToRight.IsImplicit)
                {
                    boundLeft = new BoundConversionExpression(rightType, boundLeft);
                }
                else
                {
                    var rightToLeft = Conversion.Classify(rightType, leftType);
                    if (rightToLeft.IsImplicit)
                    {
                        boundRight = new BoundConversionExpression(leftType, boundRight);
                    }
                }
            }

            var boundOperator = BoundBinaryOperator.Bind(syntax.Operatortoken.Kind, boundLeft.Type, boundRight.Type);
            if (boundOperator == null)
            {
                _diagnostics.ReportUndefinedBinaryOperator(syntax.Operatortoken.Span, syntax.Operatortoken.Text, leftType, rightType);
                return new BoundErrorExpression();
            }
            return ConstantFolding.Fold(boundLeft, boundOperator, boundRight);
        }

        private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
        {
            var value = syntax.Value ?? 0;
            return new BoundLiteralExpression(value);
        }

        private BoundStatement BindFunctionDeclaration(FunctionDeclarationSyntax syntax)
        {
            var parameters = ImmutableArray.CreateBuilder<ParaMeterSymbol>();
            var seenParameterNames = new HashSet<string>();

            foreach (var parameterSyntax in syntax.Parameters)
            {
                var parameterName = parameterSyntax.Identifier.Text;
                var parameterType = BindTypeClause(parameterSyntax.Type);

                if (!seenParameterNames.Add(parameterName))
                {
                    _diagnostics.ReportVariableAlreadyDeclared(parameterSyntax.Identifier.Span, parameterName);
                }
                else
                {
                    var parameter = new ParaMeterSymbol(parameterName, parameterType);
                    parameters.Add(parameter);
                }
            }

            var returnType = TypeSymbol.Void;
            if (syntax.Type != null)
            {
                returnType = BindTypeClause(syntax.Type);
            }

            var function = new FunctionSymbol(syntax.Identifier.Text, parameters.ToImmutable(), returnType);
            if (!_scope.TryDeclareFunction(function))
            {
                _diagnostics.ReportFunctionAlreadyDeclared(syntax.Identifier.Span, function.Name);
            }

            var previousFunction = _currentFunction;
            _currentFunction = function;

            var functionScope = new BoundScope(_scope);
            foreach (var parameter in function.Parameter)
            {
                functionScope.TryDeclareVariable(parameter);
            }

            var previousScope = _scope;
            _scope = functionScope;

            var body = BindBlockStatement(syntax.Body);

            _scope = previousScope;
            _currentFunction = previousFunction;

            if (function.Type != TypeSymbol.Void && function.Type != TypeSymbol.Error)
            {
                if (!AllPathsReturn(body))
                {
                    _diagnostics.ReportAllPathsMustReturn(syntax.Identifier.Span);
                }
            }

            function.Body = (BoundBlockStatement)body;

            var localsBuilder = ImmutableArray.CreateBuilder<VariableSymbol>();
            foreach (var parameter in function.Parameter)
            {
                localsBuilder.Add(parameter);
            }
            CollectLocals(body, localsBuilder);
            function.Locals = new HashSet<VariableSymbol>(localsBuilder);

            return new BoundFunctionDeclaration(function, (BoundBlockStatement)body);
        }

        private BoundStatement BindReturnStatement(ReturnStatementSyntax syntax)
        {
            if (_currentFunction == null)
            {
                _diagnostics.ReportInvalidReturnStatement(syntax.ReturnKeyword.Span);
            }

            BoundExpression expression = null;
            if (syntax.Expression != null)
            {
                expression = BindExpression(syntax.Expression);
            }

            if (_currentFunction != null)
            {
                var expectedType = _currentFunction.Type;
                var actualType = expression == null ? TypeSymbol.Void : expression.Type;

                if (expectedType != TypeSymbol.Error && actualType != TypeSymbol.Error)
                {
                    if (expectedType == TypeSymbol.Void && actualType != TypeSymbol.Void)
                    {
                        _diagnostics.ReportInvalidReturnType(syntax.Expression.Span, expectedType, actualType);
                    }
                    else if (expectedType != TypeSymbol.Void && actualType == TypeSymbol.Void)
                    {
                        _diagnostics.ReportInvalidReturnType(syntax.ReturnKeyword.Span, expectedType, actualType);
                    }
                    else if (expectedType != actualType)
                    {
                        var conversion = Conversion.Classify(actualType, expectedType);
                        if (!conversion.Exists)
                        {
                            _diagnostics.ReportInvalidReturnType(syntax.Expression.Span, expectedType, actualType);
                        }
                        else if (conversion.IsImplicit)
                        {
                            expression = new BoundConversionExpression(expectedType, expression);
                        }
                        else
                        {
                            _diagnostics.ReportInvalidReturnType(syntax.Expression.Span, expectedType, actualType);
                        }
                    }
                }
            }

            return new BoundReturnStatement(expression);
        }

        private static bool AllPathsReturn(BoundStatement statement)
        {
            if (statement is BoundReturnStatement)
                return true;

            if (statement is BoundBlockStatement block)
            {
                foreach (var s in block.Statements)
                {
                    if (AllPathsReturn(s))
                        return true;
                }
                return false;
            }

            if (statement is BoundIfStatement @if)
            {
                if (@if.ElseStatement == null)
                    return false;
                return AllPathsReturn(@if.ThenStatement) && AllPathsReturn(@if.ElseStatement);
            }

            return false;
        }

        private BoundExpression BindArrayLiteralExpression(ArrayLiteralExpressionSyntax syntax)
        {
            var boundElements = ImmutableArray.CreateBuilder<BoundExpression>();
            foreach (var elementSyntax in syntax.Elements)
            {
                boundElements.Add(BindExpression(elementSyntax));
            }

            TypeSymbol elementType = TypeSymbol.Int;
            if (boundElements.Count > 0)
            {
                elementType = boundElements[0].Type;
                for (int i = 1; i < boundElements.Count; i++)
                {
                    var currentType = boundElements[i].Type;
                    if (currentType != elementType)
                    {
                        var convToElement = Conversion.Classify(currentType, elementType);
                        if (convToElement.Exists && convToElement.IsImplicit)
                        {
                            // Can convert implicitly
                        }
                        else
                        {
                            var convToCurrent = Conversion.Classify(elementType, currentType);
                            if (convToCurrent.Exists && convToCurrent.IsImplicit)
                            {
                                elementType = currentType;
                            }
                            else
                            {
                                _diagnostics.ReportCannotConvert(syntax.Elements[i].Span, currentType, elementType);
                            }
                        }
                    }
                }
            }

            var finalElements = ImmutableArray.CreateBuilder<BoundExpression>();
            for (int i = 0; i < boundElements.Count; i++)
            {
                var element = boundElements[i];
                if (element.Type != elementType && element.Type != TypeSymbol.Error && elementType != TypeSymbol.Error)
                    finalElements.Add(new BoundConversionExpression(elementType, element));
                else
                    finalElements.Add(element);
            }

            var arrayType = new ArrayTypeSymbol(elementType);
            return new BoundArrayLiteralExpression(arrayType, finalElements.ToImmutable());
        }

        private BoundExpression BindArrayAccessExpression(ArrayAccessExpressionSyntax syntax)
        {
            var array = BindExpression(syntax.Array);
            var index = BindExpression(syntax.Index);

            if (array.Type == TypeSymbol.Error)
                return new BoundErrorExpression();

            if (!(array.Type is ArrayTypeSymbol arrayType))
            {
                _diagnostics.ReportCannotIndex(syntax.Array.Span, array.Type);
                return new BoundErrorExpression();
            }

            if (index.Type != TypeSymbol.Int)
            {
                var conversion = Conversion.Classify(index.Type, TypeSymbol.Int);
                if (conversion.Exists && !conversion.IsIdentity)
                {
                    index = new BoundConversionExpression(TypeSymbol.Int, index);
                }
                else if (!conversion.Exists)
                {
                    _diagnostics.ReportIndexMustBeInt(syntax.Index.Span, index.Type);
                }
            }

            return new BoundArrayAccessExpression(array, index);
        }

        private static void CollectLocals(BoundStatement statement, ImmutableArray<VariableSymbol>.Builder builder)
        {
            if (statement == null)
                return;

            if (statement is BoundVariableDeclaration decl)
            {
                builder.Add(decl.Variable);
            }
            else if (statement is BoundBlockStatement block)
            {
                foreach (var s in block.Statements)
                    CollectLocals(s, builder);
            }
            else if (statement is BoundIfStatement @if)
            {
                CollectLocals(@if.ThenStatement, builder);
                CollectLocals(@if.ElseStatement, builder);
            }
            else if (statement is BoundWhileStatement whileStmt)
            {
                CollectLocals(whileStmt.Body, builder);
            }
            else if (statement is BoundForStatement forStmt)
            {
                builder.Add(forStmt.Variable);
                CollectLocals(forStmt.Body, builder);
            }
        }
    }
}
