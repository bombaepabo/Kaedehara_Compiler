using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Emit;
using Kaedehara.CodeAnalysis.Binding;
using Kaedehara.CodeAnalysis.Symbols;

namespace Kaedehara.CodeAnalysis.Emit
{
    public static class GlobalState
    {
        public static readonly List<VariableSymbol> Symbols = new List<VariableSymbol>();
        private static Random _random;

        public static int GetSymbolIndex(VariableSymbol symbol)
        {
            lock (Symbols)
            {
                int index = Symbols.IndexOf(symbol);
                if (index < 0)
                {
                    index = Symbols.Count;
                    Symbols.Add(symbol);
                }
                return index;
            }
        }

        public static object GetVariable(Dictionary<VariableSymbol, object> variables, int symbolIndex)
        {
            VariableSymbol symbol;
            lock (Symbols)
            {
                symbol = Symbols[symbolIndex];
            }
            lock (variables)
            {
                return variables.TryGetValue(symbol, out var val) ? val : null;
            }
        }

        public static void SetVariable(Dictionary<VariableSymbol, object> variables, int symbolIndex, object value)
        {
            VariableSymbol symbol;
            lock (Symbols)
            {
                symbol = Symbols[symbolIndex];
            }
            lock (variables)
            {
                variables[symbol] = value;
            }
        }

        public static int GetRandom(int max)
        {
            if (_random == null)
            {
                _random = new Random();
            }
            return _random.Next(max);
        }
    }

    internal sealed class Emitter
    {
        public static readonly Dictionary<string, DynamicMethod> CompiledFunctions = new Dictionary<string, DynamicMethod>();

        private readonly ILGenerator _il;
        private readonly Dictionary<VariableSymbol, LocalBuilder> _locals = new Dictionary<VariableSymbol, LocalBuilder>();
        private readonly Dictionary<BoundLabel, Label> _labels = new Dictionary<BoundLabel, Label>();
        private readonly FunctionSymbol _currentFunction;

        private Emitter(ILGenerator il, FunctionSymbol currentFunction)
        {
            _il = il;
            _currentFunction = currentFunction;
        }

        public static object EmitAndExecute(BoundBlockStatement program, Dictionary<VariableSymbol, object> variables)
        {
            DeclareFunctions(program);
            CompileFunctions(program);

            var mainMethod = new DynamicMethod(
                "MainScript",
                typeof(object),
                new[] { typeof(Dictionary<VariableSymbol, object>) },
                typeof(Emitter).Module
            );

            var il = mainMethod.GetILGenerator();
            var emitter = new Emitter(il, null);
            emitter.EmitProgram(program);

            var mainDelegate = (Func<Dictionary<VariableSymbol, object>, object>)mainMethod.CreateDelegate(typeof(Func<Dictionary<VariableSymbol, object>, object>));
            return mainDelegate(variables);
        }

        private static void DeclareFunctions(BoundBlockStatement program)
        {
            foreach (var statement in program.Statements)
            {
                if (statement is BoundFunctionDeclaration decl)
                {
                    var method = CreateFunctionDynamicMethod(decl.Function);
                    CompiledFunctions[decl.Function.Name] = method;
                }
            }
        }

        private static void CompileFunctions(BoundBlockStatement program)
        {
            foreach (var statement in program.Statements)
            {
                if (statement is BoundFunctionDeclaration decl)
                {
                    var method = CompiledFunctions[decl.Function.Name];
                    var il = method.GetILGenerator();
                    var emitter = new Emitter(il, decl.Function);
                    emitter.EmitFunctionBody(decl);
                }
            }
        }

        private static DynamicMethod CreateFunctionDynamicMethod(FunctionSymbol function)
        {
            var parameterTypes = new List<Type> { typeof(Dictionary<VariableSymbol, object>) };
            foreach (var parameter in function.Parameter)
            {
                parameterTypes.Add(GetSystemType(parameter.Type));
            }

            var method = new DynamicMethod(
                function.Name,
                GetSystemType(function.Type),
                parameterTypes.ToArray(),
                typeof(Emitter).Module
            );
            return method;
        }

        private static Type GetSystemType(TypeSymbol type)
        {
            if (type == TypeSymbol.Int) return typeof(int);
            if (type == TypeSymbol.Float) return typeof(double);
            if (type == TypeSymbol.Bool) return typeof(bool);
            if (type == TypeSymbol.Char) return typeof(char);
            if (type == TypeSymbol.String) return typeof(string);
            if (type == TypeSymbol.Void) return typeof(void);
            if (type is ArrayTypeSymbol) return typeof(object[]);
            return typeof(object);
        }

        private void EmitProgram(BoundBlockStatement program)
        {
            var lastValueLoc = _il.DeclareLocal(typeof(object));
            _il.Emit(OpCodes.Ldnull);
            _il.Emit(OpCodes.Stloc, lastValueLoc);

            DeclareLabels(program);

            foreach (var statement in program.Statements)
            {
                if (statement is BoundFunctionDeclaration)
                    continue;

                if (statement is BoundExpressionStatement exprStmt)
                {
                    EmitExpression(exprStmt.Expression);
                    if (exprStmt.Expression.Type != TypeSymbol.Void)
                    {
                        BoxIfValueType(exprStmt.Expression.Type);
                        _il.Emit(OpCodes.Stloc, lastValueLoc);
                    }
                }
                else
                {
                    EmitStatement(statement);
                }
            }

            _il.Emit(OpCodes.Ldloc, lastValueLoc);
            _il.Emit(OpCodes.Ret);
        }

        private void EmitFunctionBody(BoundFunctionDeclaration decl)
        {
            foreach (var local in decl.Function.Locals)
            {
                _locals[local] = _il.DeclareLocal(GetSystemType(local.Type));
            }

            DeclareLabels(decl.Body);

            foreach (var statement in decl.Body.Statements)
            {
                EmitStatement(statement);
            }

            if (decl.Function.Type == TypeSymbol.Void)
            {
                _il.Emit(OpCodes.Ret);
            }
            else
            {
                EmitDefault(GetSystemType(decl.Function.Type));
                _il.Emit(OpCodes.Ret);
            }
        }

        private void DeclareLabels(BoundBlockStatement block)
        {
            foreach (var statement in block.Statements)
            {
                if (statement is BoundLabelStatement labelStmt)
                {
                    _labels[labelStmt.Label] = _il.DefineLabel();
                }
            }
        }

        private bool IsLocal(VariableSymbol variable)
        {
            if (_currentFunction == null) return false;

            foreach (var parameter in _currentFunction.Parameter)
            {
                if (parameter == variable) return true;
            }

            return _currentFunction.Locals.Contains(variable);
        }

        private int GetParameterIndex(VariableSymbol variable)
        {
            if (_currentFunction == null) return -1;
            for (int i = 0; i < _currentFunction.Parameter.Length; i++)
            {
                if (_currentFunction.Parameter[i] == variable)
                {
                    return i + 1;
                }
            }
            return -1;
        }

        private void LoadVariable(VariableSymbol variable)
        {
            if (IsLocal(variable))
            {
                var paramIndex = GetParameterIndex(variable);
                if (paramIndex >= 0)
                {
                    _il.Emit(OpCodes.Ldarg, paramIndex);
                }
                else
                {
                    var local = _locals[variable];
                    _il.Emit(OpCodes.Ldloc, local);
                }
            }
            else
            {
                var index = GlobalState.GetSymbolIndex(variable);
                _il.Emit(OpCodes.Ldarg_0);
                _il.Emit(OpCodes.Ldc_I4, index);
                _il.Emit(OpCodes.Call, typeof(GlobalState).GetMethod(nameof(GlobalState.GetVariable))!);
                UnboxOrCast(variable.Type);
            }
        }

        private void StoreVariable(VariableSymbol variable)
        {
            if (IsLocal(variable))
            {
                var paramIndex = GetParameterIndex(variable);
                if (paramIndex >= 0)
                {
                    _il.Emit(OpCodes.Starg, paramIndex);
                }
                else
                {
                    var local = _locals[variable];
                    _il.Emit(OpCodes.Stloc, local);
                }
            }
            else
            {
                var index = GlobalState.GetSymbolIndex(variable);
                BoxIfValueType(variable.Type);

                var temp = _il.DeclareLocal(typeof(object));
                _il.Emit(OpCodes.Stloc, temp);
                _il.Emit(OpCodes.Ldarg_0);
                _il.Emit(OpCodes.Ldc_I4, index);
                _il.Emit(OpCodes.Ldloc, temp);
                _il.Emit(OpCodes.Call, typeof(GlobalState).GetMethod(nameof(GlobalState.SetVariable))!);
            }
        }

        private void BoxIfValueType(TypeSymbol type)
        {
            if (type == TypeSymbol.Int || type == TypeSymbol.Bool || type == TypeSymbol.Char || type == TypeSymbol.Float)
            {
                _il.Emit(OpCodes.Box, GetSystemType(type));
            }
        }

        private void UnboxOrCast(TypeSymbol type)
        {
            var systemType = GetSystemType(type);
            if (systemType.IsValueType)
            {
                _il.Emit(OpCodes.Unbox_Any, systemType);
            }
            else
            {
                _il.Emit(OpCodes.Castclass, systemType);
            }
        }

        private void EmitStatement(BoundStatement statement)
        {
            switch (statement.Kind)
            {
                case BoundNodeKind.VariableDeclaration:
                    EmitVariableDeclaration((BoundVariableDeclaration)statement);
                    break;
                case BoundNodeKind.ExpressionStatement:
                    EmitExpressionStatement((BoundExpressionStatement)statement);
                    break;
                case BoundNodeKind.LabelStatement:
                    var labelStmt = (BoundLabelStatement)statement;
                    _il.MarkLabel(_labels[labelStmt.Label]);
                    break;
                case BoundNodeKind.GoToStatement:
                    var gotoStmt = (BoundGoToStatement)statement;
                    _il.Emit(OpCodes.Br, _labels[gotoStmt.Label]);
                    break;
                case BoundNodeKind.ConditionalGoToStatement:
                    EmitConditionalGoToStatement((BoundConditionalGoToStatement)statement);
                    break;
                case BoundNodeKind.ReturnStatement:
                    EmitReturnStatement((BoundReturnStatement)statement);
                    break;
                default:
                    throw new Exception($"Unexpected statement: {statement.Kind}");
            }
        }

        private void EmitVariableDeclaration(BoundVariableDeclaration node)
        {
            EmitExpression(node.Initializer);
            StoreVariable(node.Variable);
        }

        private void EmitExpressionStatement(BoundExpressionStatement node)
        {
            EmitExpression(node.Expression);
            if (node.Expression.Type != TypeSymbol.Void)
            {
                _il.Emit(OpCodes.Pop);
            }
        }

        private void EmitConditionalGoToStatement(BoundConditionalGoToStatement node)
        {
            EmitExpression(node.Condition);
            if (node.JumpIfTrue)
            {
                _il.Emit(OpCodes.Brtrue, _labels[node.Label]);
            }
            else
            {
                _il.Emit(OpCodes.Brfalse, _labels[node.Label]);
            }
        }

        private void EmitReturnStatement(BoundReturnStatement node)
        {
            if (node.Expression != null)
            {
                EmitExpression(node.Expression);
            }
            _il.Emit(OpCodes.Ret);
        }

        private void EmitExpression(BoundExpression node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.LiteralExpression:
                    EmitLiteralExpression((BoundLiteralExpression)node);
                    break;
                case BoundNodeKind.VariableExpression:
                    var varExpr = (BoundVariableExpression)node;
                    LoadVariable(varExpr.Variable);
                    break;
                case BoundNodeKind.AssignmentExpression:
                    EmitAssignmentExpression((BoundAssignmentExpression)node);
                    break;
                case BoundNodeKind.UnaryExpression:
                    EmitUnaryExpression((BoundUnaryExpression)node);
                    break;
                case BoundNodeKind.BinaryExpression:
                    EmitBinaryExpression((BoundBinaryExpression)node);
                    break;
                case BoundNodeKind.CallExpression:
                    EmitCallExpression((BoundCallExpression)node);
                    break;
                case BoundNodeKind.ConversionExpression:
                    EmitConversionExpression((BoundConversionExpression)node);
                    break;
                case BoundNodeKind.ArrayLiteralExpression:
                    EmitArrayLiteralExpression((BoundArrayLiteralExpression)node);
                    break;
                case BoundNodeKind.ArrayAccessExpression:
                    EmitArrayAccessExpression((BoundArrayAccessExpression)node);
                    break;
                case BoundNodeKind.ArrayAssignmentExpression:
                    EmitArrayAssignmentExpression((BoundArrayAssignmentExpression)node);
                    break;
                default:
                    throw new Exception($"Unexpected expression: {node.Kind}");
            }
        }

        private void EmitLiteralExpression(BoundLiteralExpression node)
        {
            var value = node.Value;
            if (value is int i)
            {
                _il.Emit(OpCodes.Ldc_I4, i);
            }
            else if (value is double d)
            {
                _il.Emit(OpCodes.Ldc_R8, d);
            }
            else if (value is bool b)
            {
                _il.Emit(b ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            }
            else if (value is char c)
            {
                _il.Emit(OpCodes.Ldc_I4, (int)c);
            }
            else if (value is string s)
            {
                _il.Emit(OpCodes.Ldstr, s);
            }
            else if (value == null)
            {
                _il.Emit(OpCodes.Ldnull);
            }
            else
            {
                throw new Exception($"Unexpected literal value type: {value.GetType()}");
            }
        }

        private void EmitAssignmentExpression(BoundAssignmentExpression node)
        {
            EmitExpression(node.Expression);
            _il.Emit(OpCodes.Dup);
            StoreVariable(node.Variable);
        }

        private void EmitUnaryExpression(BoundUnaryExpression node)
        {
            EmitExpression(node.Operand);
            switch (node.Op.Kind)
            {
                case BoundUnaryOperatorKind.identity:
                    break;
                case BoundUnaryOperatorKind.Negation:
                    _il.Emit(OpCodes.Neg);
                    break;
                case BoundUnaryOperatorKind.LogicalNegation:
                    _il.Emit(OpCodes.Ldc_I4_0);
                    _il.Emit(OpCodes.Ceq);
                    break;
                case BoundUnaryOperatorKind.OnesComplement:
                    _il.Emit(OpCodes.Not);
                    break;
                default:
                    throw new Exception($"Unexpected unary operator kind: {node.Op.Kind}");
            }
        }

        private void EmitBinaryExpression(BoundBinaryExpression node)
        {
            EmitExpression(node.Left);
            EmitExpression(node.Right);

            switch (node.Op.Kind)
            {
                case BoundBinaryOperatorKind.Addition:
                    if (node.Type == TypeSymbol.String)
                    {
                        var concatMethod = typeof(string).GetMethod(nameof(string.Concat), new[] { typeof(string), typeof(string) })!;
                        _il.Emit(OpCodes.Call, concatMethod);
                    }
                    else
                    {
                        _il.Emit(OpCodes.Add);
                    }
                    break;
                case BoundBinaryOperatorKind.Subtraction:
                    _il.Emit(OpCodes.Sub);
                    break;
                case BoundBinaryOperatorKind.Multiplication:
                    _il.Emit(OpCodes.Mul);
                    break;
                case BoundBinaryOperatorKind.Division:
                    _il.Emit(OpCodes.Div);
                    break;
                case BoundBinaryOperatorKind.BitwiseAnd:
                case BoundBinaryOperatorKind.LogicalAnd:
                    _il.Emit(OpCodes.And);
                    break;
                case BoundBinaryOperatorKind.BitWiseOr:
                case BoundBinaryOperatorKind.LogicalOr:
                    _il.Emit(OpCodes.Or);
                    break;
                case BoundBinaryOperatorKind.BitWiseXOR:
                    _il.Emit(OpCodes.Xor);
                    break;
                case BoundBinaryOperatorKind.Equals:
                    if (node.Left.Type == TypeSymbol.String || node.Left.Type is ArrayTypeSymbol)
                    {
                        var equalsMethod = typeof(object).GetMethod(nameof(object.Equals), new[] { typeof(object), typeof(object) })!;
                        _il.Emit(OpCodes.Call, equalsMethod);
                    }
                    else
                    {
                        _il.Emit(OpCodes.Ceq);
                    }
                    break;
                case BoundBinaryOperatorKind.NotEquals:
                    if (node.Left.Type == TypeSymbol.String || node.Left.Type is ArrayTypeSymbol)
                    {
                        var equalsMethod = typeof(object).GetMethod(nameof(object.Equals), new[] { typeof(object), typeof(object) })!;
                        _il.Emit(OpCodes.Call, equalsMethod);
                        _il.Emit(OpCodes.Ldc_I4_0);
                        _il.Emit(OpCodes.Ceq);
                    }
                    else
                    {
                        _il.Emit(OpCodes.Ceq);
                        _il.Emit(OpCodes.Ldc_I4_0);
                        _il.Emit(OpCodes.Ceq);
                    }
                    break;
                case BoundBinaryOperatorKind.LessThan:
                    _il.Emit(OpCodes.Clt);
                    break;
                case BoundBinaryOperatorKind.LessThanOrEqualsTo:
                    _il.Emit(OpCodes.Cgt);
                    _il.Emit(OpCodes.Ldc_I4_0);
                    _il.Emit(OpCodes.Ceq);
                    break;
                case BoundBinaryOperatorKind.GreaterThan:
                    _il.Emit(OpCodes.Cgt);
                    break;
                case BoundBinaryOperatorKind.GreaterOrEqualsTo:
                    _il.Emit(OpCodes.Clt);
                    _il.Emit(OpCodes.Ldc_I4_0);
                    _il.Emit(OpCodes.Ceq);
                    break;
                default:
                    throw new Exception($"Unexpected binary operator kind: {node.Op.Kind}");
            }
        }

        private void EmitCallExpression(BoundCallExpression node)
        {
            if (node.Function == BuiltinFunctions.Input)
            {
                var readLineMethod = typeof(Console).GetMethod(nameof(Console.ReadLine))!;
                _il.Emit(OpCodes.Call, readLineMethod);
            }
            else if (node.Function == BuiltinFunctions.Print)
            {
                EmitExpression(node.Arguments[0]);
                BoxIfValueType(node.Arguments[0].Type);
                var writeLineMethod = typeof(Console).GetMethod(nameof(Console.WriteLine), new[] { typeof(object) })!;
                _il.Emit(OpCodes.Call, writeLineMethod);
            }
            else if (node.Function == BuiltinFunctions.Rnd)
            {
                EmitExpression(node.Arguments[0]);
                var getRandomMethod = typeof(GlobalState).GetMethod(nameof(GlobalState.GetRandom))!;
                _il.Emit(OpCodes.Call, getRandomMethod);
            }
            else
            {
                _il.Emit(OpCodes.Ldarg_0);

                foreach (var arg in node.Arguments)
                {
                    EmitExpression(arg);
                }

                var method = CompiledFunctions[node.Function.Name];
                _il.Emit(OpCodes.Call, method);
            }
        }

        private void EmitConversionExpression(BoundConversionExpression node)
        {
            EmitExpression(node.Expression);

            var toType = node.Type;
            if (toType == TypeSymbol.Int)
            {
                BoxIfValueType(node.Expression.Type);
                var toIntMethod = typeof(Convert).GetMethod(nameof(Convert.ToInt32), new[] { typeof(object) })!;
                _il.Emit(OpCodes.Call, toIntMethod);
            }
            else if (toType == TypeSymbol.Float)
            {
                BoxIfValueType(node.Expression.Type);
                var toDoubleMethod = typeof(Convert).GetMethod(nameof(Convert.ToDouble), new[] { typeof(object) })!;
                _il.Emit(OpCodes.Call, toDoubleMethod);
            }
            else if (toType == TypeSymbol.Bool)
            {
                BoxIfValueType(node.Expression.Type);
                var toBoolMethod = typeof(Convert).GetMethod(nameof(Convert.ToBoolean), new[] { typeof(object) })!;
                _il.Emit(OpCodes.Call, toBoolMethod);
            }
            else if (toType == TypeSymbol.Char)
            {
                BoxIfValueType(node.Expression.Type);
                var toCharMethod = typeof(Convert).GetMethod(nameof(Convert.ToChar), new[] { typeof(object) })!;
                _il.Emit(OpCodes.Call, toCharMethod);
            }
            else if (toType == TypeSymbol.String)
            {
                BoxIfValueType(node.Expression.Type);
                var toStringMethod = typeof(Convert).GetMethod(nameof(Convert.ToString), new[] { typeof(object) })!;
                _il.Emit(OpCodes.Call, toStringMethod);
            }
        }

        private void EmitArrayLiteralExpression(BoundArrayLiteralExpression node)
        {
            _il.Emit(OpCodes.Ldc_I4, node.Elements.Length);
            _il.Emit(OpCodes.Newarr, typeof(object));

            for (int i = 0; i < node.Elements.Length; i++)
            {
                _il.Emit(OpCodes.Dup);
                _il.Emit(OpCodes.Ldc_I4, i);
                EmitExpression(node.Elements[i]);
                BoxIfValueType(node.Elements[i].Type);
                _il.Emit(OpCodes.Stelem_Ref);
            }
        }

        private void EmitArrayAccessExpression(BoundArrayAccessExpression node)
        {
            EmitExpression(node.Array);
            EmitExpression(node.Index);
            _il.Emit(OpCodes.Ldelem_Ref);
            UnboxOrCast(node.Type);
        }

        private void EmitArrayAssignmentExpression(BoundArrayAssignmentExpression node)
        {
            EmitExpression(node.Expression);
            var temp = _il.DeclareLocal(GetSystemType(node.Expression.Type));
            _il.Emit(OpCodes.Stloc, temp);

            EmitExpression(node.Array);
            EmitExpression(node.Index);

            _il.Emit(OpCodes.Ldloc, temp);
            BoxIfValueType(node.Expression.Type);

            _il.Emit(OpCodes.Stelem_Ref);

            _il.Emit(OpCodes.Ldloc, temp);
        }

        private void EmitDefault(Type type)
        {
            if (type == typeof(void))
                return;

            if (type.IsValueType)
            {
                if (type == typeof(double))
                {
                    _il.Emit(OpCodes.Ldc_R8, 0.0);
                }
                else
                {
                    _il.Emit(OpCodes.Ldc_I4_0);
                }
            }
            else
            {
                _il.Emit(OpCodes.Ldnull);
            }
        }
    }
}
