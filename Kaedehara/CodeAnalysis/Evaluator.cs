using Kaedehara.CodeAnalysis.Binding;
using Kaedehara.CodeAnalysis.Symbols;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Numerics;
using System.Reflection.Emit;
using System.Xml;

namespace Kaedehara.CodeAnalysis
{

    internal sealed class Evaluator
    {
        private readonly BoundBlockStatement _root;
        private readonly Dictionary<VariableSymbol, object> _variables;
        private readonly HashSet<VariableSymbol> _globals;
        private readonly Stack<Dictionary<VariableSymbol, object>> _locals = new Stack<Dictionary<VariableSymbol, object>>();
        private readonly Stack<FunctionSymbol> _currentFunctions = new Stack<FunctionSymbol>();
        private object _lastValue;
        private Random _random;

        public Evaluator(BoundBlockStatement root, Dictionary<VariableSymbol, object> variables, HashSet<VariableSymbol> globals)
        {
            _root = root;
            _variables = variables;
            _globals = globals;
        }


        public object Evaluate()
        {
            return EvaluateBlock(_root);
        }

        private object EvaluateBlock(BoundBlockStatement block)
        {
            var labelToIndex = new Dictionary<BoundLabel, int>();
            for (var i = 0; i < block.Statements.Length; i++)
            {
                if (block.Statements[i] is BoundLabelStatement l)
                {
                    labelToIndex.Add(l.Label, i + 1);
                }
            }
            var index = 0;
            while (index < block.Statements.Length)
            {
                var s = block.Statements[index];
                switch (s.Kind)
                {

                    case BoundNodeKind.VariableDeclaration:
                        EvaluateVariableDeclaration((BoundVariableDeclaration)s);
                        index++;
                        break;
                    case BoundNodeKind.ExpressionStatement:
                        EvaluateExpressionStatement((BoundExpressionStatement)s);
                        index++;
                        break;
                    case BoundNodeKind.GoToStatement:
                        var gs = (BoundGoToStatement)s;
                        index = labelToIndex[gs.Label];
                        break;
                    case BoundNodeKind.ConditionalGoToStatement:
                        var cgs = (BoundConditionalGoToStatement)s;
                        var condition = (bool)EvaluateExpression(cgs.Condition);
                        if (condition == cgs.JumpIfTrue)
                        {
                            index = labelToIndex[cgs.Label];
                        }
                        else
                        {
                            index++;
                        }
                        break;
                    case BoundNodeKind.LabelStatement:
                        index++;
                        break;
                    case BoundNodeKind.FunctionDeclaration:
                        index++;
                        break;
                    case BoundNodeKind.ReturnStatement:
                        var rs = (BoundReturnStatement)s;
                        var val = rs.Expression == null ? null : EvaluateExpression(rs.Expression);
                        return val;
                    default:
                        throw new Exception($"unexpected node {s.Kind}");
                }
            }
            return _lastValue;
        }

        private object EvaluateExpression(BoundExpression node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.LiteralExpression:
                    return EvaluateLiteralExpression((BoundLiteralExpression)node);
                case BoundNodeKind.VariableExpression:
                    return EvaluateVariableExpression((BoundVariableExpression)node);
                case BoundNodeKind.AssignmentExpression:
                    return EvaluateAssignmentExpression((BoundAssignmentExpression)node);
                case BoundNodeKind.UnaryExpression:
                    return EvaluateUnaryExpression((BoundUnaryExpression)node);
                case BoundNodeKind.BinaryExpression:
                    return EvaluateBinaryExpression((BoundBinaryExpression)node);
                case BoundNodeKind.CallExpression:
                    return EvaluateCallExpression((BoundCallExpression)node);
                case BoundNodeKind.ConversionExpression:
                    return EvaluateConversionExpression((BoundConversionExpression)node);
                case BoundNodeKind.ArrayLiteralExpression:
                    return EvaluateArrayLiteralExpression((BoundArrayLiteralExpression)node);
                case BoundNodeKind.ArrayAccessExpression:
                    return EvaluateArrayAccessExpression((BoundArrayAccessExpression)node);
                case BoundNodeKind.ArrayAssignmentExpression:
                    return EvaluateArrayAssignmentExpression((BoundArrayAssignmentExpression)node);
                default:
                    throw new Exception($"Unexpected node {node.Kind}");
            }
        }

        private object EvaluateArrayLiteralExpression(BoundArrayLiteralExpression node)
        {
            var elements = new object[node.Elements.Length];
            for (int i = 0; i < node.Elements.Length; i++)
            {
                elements[i] = EvaluateExpression(node.Elements[i]);
            }
            return elements;
        }

        private object EvaluateArrayAccessExpression(BoundArrayAccessExpression node)
        {
            var array = (object[])EvaluateExpression(node.Array);
            var index = (int)EvaluateExpression(node.Index);
            return array[index];
        }

        private object EvaluateArrayAssignmentExpression(BoundArrayAssignmentExpression node)
        {
            var array = (object[])EvaluateExpression(node.Array);
            var index = (int)EvaluateExpression(node.Index);
            var value = EvaluateExpression(node.Expression);
            array[index] = value;
            return value;
        }

        private object EvaluateConversionExpression(BoundConversionExpression node)
        {
            var value = EvaluateExpression(node.Expression);
            if(node.Type == TypeSymbol.Bool){
                return Convert.ToBoolean(value);
            }
            else if(node.Type == TypeSymbol.Int){
                if (value is double d) return (int)d;
                return Convert.ToInt32(value);
            }
            else if(node.Type == TypeSymbol.String){
                return Convert.ToString(value);
            }
            else if(node.Type == TypeSymbol.Float){
                return Convert.ToDouble(value);
            }
            else if(node.Type == TypeSymbol.Char){
                if (value is string s && s.Length > 0) return s[0];
                return Convert.ToChar(value);
            }
            else{
                throw new Exception($"Unexpected node {node.Type}");

            }
        }

        private object EvaluateCallExpression(BoundCallExpression node)
        {
            if (node.Function == BuiltinFunctions.Input)
            {
                return Console.ReadLine();
            }
            else if (node.Function == BuiltinFunctions.Print)
            {
                var message = Convert.ToString(EvaluateExpression(node.Arguments[0]));
                Console.WriteLine(message);
                return null;
            }
            else if (node.Function == BuiltinFunctions.Rnd)
            {
                var max = (int)EvaluateExpression(node.Arguments[0]);
                if (_random == null)
                {
                    _random = new Random();
                }
                return _random.Next(max);
            }
            else
            {
                var parameterValues = new object[node.Arguments.Length];
                for (int i = 0; i < node.Arguments.Length; i++)
                {
                    parameterValues[i] = EvaluateExpression(node.Arguments[i]);
                }

                var frame = new Dictionary<VariableSymbol, object>();
                for (int i = 0; i < node.Arguments.Length; i++)
                {
                    frame[node.Function.Parameter[i]] = parameterValues[i];
                }

                _locals.Push(frame);
                _currentFunctions.Push(node.Function);

                var result = EvaluateBlock(node.Function.Body);

                _currentFunctions.Pop();
                _locals.Pop();

                return result;
            }
        }

        private object EvaluateUnaryExpression(BoundUnaryExpression u)
        {
            var operand = EvaluateExpression(u.Operand);
            switch (u.Op.Kind)
            {
                case BoundUnaryOperatorKind.identity:
                    if (u.Type == TypeSymbol.Float)
                        return (double)operand;
                    return (int)operand;
                case BoundUnaryOperatorKind.Negation:
                    if (u.Type == TypeSymbol.Float)
                        return -(double)operand;
                    return -(int)operand;
                case BoundUnaryOperatorKind.LogicalNegation:
                    return !(bool)operand;
                case BoundUnaryOperatorKind.OnesComplement:
                    return ~(int)operand;
                default:
                    throw new Exception($"Unexpected unary operator {u.Op}");
            }
        }

        private object EvaluateBinaryExpression(BoundBinaryExpression b)
        {
            var left = EvaluateExpression(b.Left);
            var right = EvaluateExpression(b.Right);

            switch (b.Op.Kind)
            {
                case BoundBinaryOperatorKind.Addition:
                    if (b.Type == TypeSymbol.Int)
                        return (int)left + (int)right;
                    else if (b.Type == TypeSymbol.Float)
                        return (double)left + (double)right;
                    else
                        return (string)left + (string)right;
                case BoundBinaryOperatorKind.Subtraction:
                    if (b.Type == TypeSymbol.Float)
                        return (double)left - (double)right;
                    return (int)left - (int)right;
                case BoundBinaryOperatorKind.Multiplication:
                    if (b.Type == TypeSymbol.Float)
                        return (double)left * (double)right;
                    return (int)left * (int)right;
                case BoundBinaryOperatorKind.Division:
                    if (b.Type == TypeSymbol.Float)
                        return (double)left / (double)right;
                    return (int)left / (int)right;

                case BoundBinaryOperatorKind.BitwiseAnd:
                    if (b.Type == TypeSymbol.Int)
                        return (int)left & (int)right;
                    else
                        return (bool)left & (bool)right;
                case BoundBinaryOperatorKind.BitWiseOr:
                    if (b.Type == TypeSymbol.Int)
                        return (int)left | (int)right;
                    else
                        return (bool)left | (bool)right;
                case BoundBinaryOperatorKind.BitWiseXOR:
                    if (b.Type == TypeSymbol.Int)
                        return (int)left ^ (int)right;
                    else
                        return (bool)left ^ (bool)right;

                case BoundBinaryOperatorKind.LogicalAnd:
                    return (bool)left && (bool)right;
                case BoundBinaryOperatorKind.LogicalOr:
                    return (bool)left || (bool)right;
                case BoundBinaryOperatorKind.Equals:
                    return Equals(left, right);
                case BoundBinaryOperatorKind.NotEquals:
                    return !Equals(left, right);
                case BoundBinaryOperatorKind.LessThan:
                    if (b.Left.Type == TypeSymbol.Float)
                        return (double)left < (double)right;
                    else if (b.Left.Type == TypeSymbol.Char)
                        return (char)left < (char)right;
                    return (int)left < (int)right;
                case BoundBinaryOperatorKind.LessThanOrEqualsTo:
                    if (b.Left.Type == TypeSymbol.Float)
                        return (double)left <= (double)right;
                    else if (b.Left.Type == TypeSymbol.Char)
                        return (char)left <= (char)right;
                    return (int)left <= (int)right;
                case BoundBinaryOperatorKind.GreaterThan:
                    if (b.Left.Type == TypeSymbol.Float)
                        return (double)left > (double)right;
                    else if (b.Left.Type == TypeSymbol.Char)
                        return (char)left > (char)right;
                    return (int)left > (int)right;
                case BoundBinaryOperatorKind.GreaterOrEqualsTo:
                    if (b.Left.Type == TypeSymbol.Float)
                        return (double)left >= (double)right;
                    else if (b.Left.Type == TypeSymbol.Char)
                        return (char)left >= (char)right;
                    return (int)left >= (int)right;
                default:
                    throw new Exception($"unexpected binary operator {b.Op.Kind}");
            }
        }
        private void EvaluateVariableDeclaration(BoundVariableDeclaration node)
        {
            var value = EvaluateExpression(node.Initializer);
            if (_currentFunctions.Count > 0 && _currentFunctions.Peek().Locals.Contains(node.Variable))
            {
                _locals.Peek()[node.Variable] = value;
            }
            else
            {
                _variables[node.Variable] = value;
            }
            _lastValue = value;
        }


        private void EvaluateExpressionStatement(BoundExpressionStatement node)
        {
            _lastValue = EvaluateExpression(node.Expression);
        }
        private static object EvaluateLiteralExpression(BoundLiteralExpression n)
        {
            return n.Value;
        }
        private object EvaluateVariableExpression(BoundVariableExpression v)
        {
            if (_currentFunctions.Count > 0 && _currentFunctions.Peek().Locals.Contains(v.Variable))
            {
                return _locals.Peek()[v.Variable];
            }
            return _variables[v.Variable];
        }
        private object EvaluateAssignmentExpression(BoundAssignmentExpression a)
        {
            var value = EvaluateExpression(a.Expression);
            if (_currentFunctions.Count > 0 && _currentFunctions.Peek().Locals.Contains(a.Variable))
            {
                _locals.Peek()[a.Variable] = value;
            }
            else
            {
                _variables[a.Variable] = value;
            }
            return value;
        }
    }
}