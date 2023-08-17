using Kaedehara.CodeAnalysis.Binding;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Kaedehara.CodeAnalysis
{
    internal sealed class Evaluator
    {
        private readonly Dictionary<VariableSymbol, object> Variables;
        private readonly BoundStatement _root;
        private object _lastValue ;

        public Evaluator(BoundStatement root, Dictionary<VariableSymbol, object> variables)
        {
            _root = root;
            Variables = variables;
        }


        public object Evaluate()
        {
            EvaluateStatement(_root);
            return _lastValue ; 
        }

        private void EvaluateStatement(BoundStatement node){
            switch(node.Kind){
                case BoundNodeKind.BlockStatement:
                     EvaluateBlockStatement((BoundBlockStatement)node);
                    break;
                case BoundNodeKind.ExpressionStatement:
                     EvaluateExpressionStatement((BoundExpressionStatement)node);
                    break;
                case BoundNodeKind.VariableDeclaration:
                     EvaluateVariableDeclaration((BoundVariableDeclaration)node);
                    break;
                default:
                        throw new Exception($"unexpected statement {node.Kind}");
            }
        }

        private void EvaluateVariableDeclaration(BoundVariableDeclaration node)
        {
            var value = EvaluateExpression(node.Initializer);
            Variables[node.Variable] = value ;
            _lastValue = value ;

            
        }

        private void EvaluateExpressionStatement(BoundExpressionStatement node)
        {
            _lastValue = EvaluateExpression(node.Expression);
        }

        private void EvaluateBlockStatement(BoundBlockStatement node)
        {
            foreach (var statement in node.Statements){
                EvaluateStatement(statement);
            }
        }

        private object EvaluateExpression(BoundExpression node)
        {
            if (node is BoundLiteralExpression n)
            {
                return n.Value;
            }
            if (node is BoundVariableExpression v)
            {
                return Variables[v.Variable]; ;
            }
            if (node is BoundAssignmentExpression a)
            {
                var value = EvaluateExpression(a.Expression);
                Variables[a.Variable] = value;
                return value;
            }
            if (node is BoundUnaryExpression u)
            {
                var operand = EvaluateExpression(u.Operand);
                switch (u.Op.Kind)
                {
                    case BoundUnaryOperatorKind.identity:
                        return (int)operand;
                    case BoundUnaryOperatorKind.Negation:
                        return -(int)operand;
                    case BoundUnaryOperatorKind.LogicalNegation:
                        return !(bool)operand;
                    default:
                        throw new Exception($"unexpected unary operator {u.Op.Kind}");
                }

            }
            if (node is BoundBinaryExpression b)
            {
                var left = EvaluateExpression(b.Left);
                var right = EvaluateExpression(b.Right);

                switch (b.Op.Kind)
                {
                    case BoundBinaryOperatorKind.Addition:
                        return (int)left + (int)right;
                    case BoundBinaryOperatorKind.Subtraction:
                        return (int)left - (int)right;
                    case BoundBinaryOperatorKind.Multiplication:
                        return (int)left * (int)right;
                    case BoundBinaryOperatorKind.Division:
                        return (int)left / (int)right;
                    case BoundBinaryOperatorKind.LogicalAnd:
                        return (bool)left && (bool)right;
                    case BoundBinaryOperatorKind.LogicalOr:
                        return (bool)left || (bool)right;
                    case BoundBinaryOperatorKind.Equals:
                        return Equals(left, right);
                    case BoundBinaryOperatorKind.NotEquals:
                        return !Equals(left, right);

                    default:
                        throw new Exception($"unexpected binary operator {b.Op.Kind}");
                }
            }
            throw new Exception($"unexpected node {node.Kind}");
        }
    }







}