using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.WebSockets;
using Kaedehara.CodeAnalysis.Text;

namespace Kaedehara.CodeAnalysis.Syntax
{
    internal sealed class Parser
    {
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();
        private readonly SourceText _text;
        private readonly ImmutableArray<SyntaxToken> _tokens;
        private int _position;

        public Parser(SourceText text)
        {
            var tokens = new List<SyntaxToken>();

            var lexer = new Lexer(text);
            SyntaxToken token;
            do
            {
                token = lexer.Lex();
                if (token.Kind != SyntaxKind.WhitespaceToken &&
                    token.Kind != SyntaxKind.BadToken)
                {
                    tokens.Add(token);
                }
            } while (token.Kind != SyntaxKind.EndOfFileToken);

            _text = text;
            _tokens = tokens.ToImmutableArray();
            _diagnostics.AddRange(lexer.Diagonostics);
        }
        public DiagnosticBag Diagnostics => _diagnostics;
        private SyntaxToken Peek(int offset)
        {
            var index = _position + offset;
            if (index >= _tokens.Length)
            {
                return _tokens[_tokens.Length - 1];

            }
            return _tokens[index];
        }
        private SyntaxToken Current => Peek(0);
        private SyntaxToken NextToken()
        {
            var current = Current;
            _position++;
            return current;
        }
        private SyntaxToken MatchToken(SyntaxKind kind)
        {
            if (Current.Kind == kind)
            {
                return NextToken();
            }
            _diagnostics.ReportUnexpectedToken(Current.Span, Current.Kind, kind);
            return new SyntaxToken(kind, Current.Position, null, null);
        }
        public CompilationUnitSyntax ParseCompilationUnit()
        {
            var statement = ParseStatement();
            var endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);
            return new CompilationUnitSyntax(statement, endOfFileToken);
        }
        private StatementSyntax ParseStatement()
        {
            switch (Current.Kind)
            {
                case SyntaxKind.OpenBraceToken:
                    return ParseBlockStatement();
                case SyntaxKind.LetKeyword:
                case SyntaxKind.VarKeyword:
                    return ParseVariableDeclaration();
                case SyntaxKind.IfKeyword:
                    return ParseIfStatement();
                case SyntaxKind.WhileKeyword:
                    return ParseWhileStatement();
                case SyntaxKind.ForKeyword:
                    return ParseForStatement();
                case SyntaxKind.FnKeyword:
                    return ParseFunctionDeclaration();
                case SyntaxKind.ReturnKeyword:
                    return ParseReturnStatement();
                default:
                    return ParseExpressionStatement();
            }
        }

        private FunctionDeclarationSyntax ParseFunctionDeclaration()
        {
            var fnKeyword = MatchToken(SyntaxKind.FnKeyword);
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var openParenthesisToken = MatchToken(SyntaxKind.OpenParenthesisToken);
            var parameters = ParseParameterList();
            var closeParenthesisToken = MatchToken(SyntaxKind.CloseParenthesisToken);
            var type = ParseOptionalTypeClause();
            var body = ParseBlockStatement();
            return new FunctionDeclarationSyntax(fnKeyword, identifier, openParenthesisToken, parameters, closeParenthesisToken, type, body);
        }

        private SeparatedSyntaxList<ParameterSyntax> ParseParameterList()
        {
            var nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();
            while (Current.Kind != SyntaxKind.CloseParenthesisToken &&
                  Current.Kind != SyntaxKind.EndOfFileToken)
            {
                var parameter = ParseParameter();
                nodesAndSeparators.Add(parameter);
                if (Current.Kind != SyntaxKind.CloseParenthesisToken)
                {
                    var comma = MatchToken(SyntaxKind.CommaToken);
                    nodesAndSeparators.Add(comma);
                }
            }
            return new SeparatedSyntaxList<ParameterSyntax>(nodesAndSeparators.ToImmutable());
        }

        private ParameterSyntax ParseParameter()
        {
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var type = ParseTypeClause();
            return new ParameterSyntax(identifier, type);
        }

        private TypeClauseSyntax ParseTypeClause()
        {
            var colonToken = MatchToken(SyntaxKind.ColonToken);
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            SyntaxToken openBracketToken = null;
            SyntaxToken closeBracketToken = null;
            if (Current.Kind == SyntaxKind.OpenBracketToken)
            {
                openBracketToken = MatchToken(SyntaxKind.OpenBracketToken);
                closeBracketToken = MatchToken(SyntaxKind.CloseBracketToken);
            }
            return new TypeClauseSyntax(colonToken, identifier, openBracketToken, closeBracketToken);
        }

        private TypeClauseSyntax ParseOptionalTypeClause()
        {
            if (Current.Kind != SyntaxKind.ArrowToken)
            {
                return null;
            }
            var arrowToken = NextToken();
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            SyntaxToken openBracketToken = null;
            SyntaxToken closeBracketToken = null;
            if (Current.Kind == SyntaxKind.OpenBracketToken)
            {
                openBracketToken = MatchToken(SyntaxKind.OpenBracketToken);
                closeBracketToken = MatchToken(SyntaxKind.CloseBracketToken);
            }
            return new TypeClauseSyntax(arrowToken, identifier, openBracketToken, closeBracketToken);
        }

        private StatementSyntax ParseReturnStatement()
        {
            var keyword = MatchToken(SyntaxKind.ReturnKeyword);
            var hasExpression = Current.Kind != SyntaxKind.CloseBraceToken &&
                                Current.Kind != SyntaxKind.EndOfFileToken &&
                                Current.Kind != SyntaxKind.LetKeyword &&
                                Current.Kind != SyntaxKind.VarKeyword &&
                                Current.Kind != SyntaxKind.IfKeyword &&
                                Current.Kind != SyntaxKind.WhileKeyword &&
                                Current.Kind != SyntaxKind.ForKeyword &&
                                Current.Kind != SyntaxKind.FnKeyword &&
                                Current.Kind != SyntaxKind.ReturnKeyword;
            var expression = hasExpression ? ParseExpression() : null;
            return new ReturnStatementSyntax(keyword, expression);
        }

        private StatementSyntax ParseForStatement()
        {
            var keyword = MatchToken(SyntaxKind.ForKeyword);
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var equalsToken = MatchToken(SyntaxKind.EqualsToken);
            var lowerbound = ParseExpression();
            var toKeyword = MatchToken(SyntaxKind.ToKeyword);
            var upperbound = ParseExpression();
            var body = ParseStatement();
            return new ForStatementSyntax(keyword, identifier, equalsToken, lowerbound, toKeyword, upperbound, body);
        }
        private StatementSyntax ParseWhileStatement()
        {
            var keyword = MatchToken(SyntaxKind.WhileKeyword);
            var condition = ParseExpression();
            var body = ParseStatement();
            return new WhileStatementSyntax(keyword, condition, body);
        }

        private StatementSyntax ParseIfStatement()
        {
            var keyword = MatchToken(SyntaxKind.IfKeyword);
            var condition = ParseExpression();
            var statement = ParseStatement();
            var elseClause = ParseElseClause();
            return new IfStatementSyntax(keyword, condition, statement, elseClause);
        }

        private ElseClauseSyntax ParseElseClause()
        {
            if (Current.Kind != SyntaxKind.ElseKeyword)
            {
                return null;
            }
            var keyword = NextToken();
            var statement = ParseStatement();
            return new ElseClauseSyntax(keyword, statement);
        }

        private BlockStatementSyntax ParseBlockStatement()
        {
            var statements = ImmutableArray.CreateBuilder<StatementSyntax>();
            var openBraceToken = MatchToken(SyntaxKind.OpenBraceToken);
            while (Current.Kind != SyntaxKind.EndOfFileToken &&
                  Current.Kind != SyntaxKind.CloseBraceToken)
            {
                var startToken = Current;
                var statement = ParseStatement();
                statements.Add(statement);

                if (Current == startToken)
                {
                    NextToken();
                }
            }

            var closeBraceToken = MatchToken(SyntaxKind.CloseBraceToken);
            return new BlockStatementSyntax(openBraceToken, statements.ToImmutable(), closeBraceToken);

        }
        private StatementSyntax ParseVariableDeclaration()
        {
            var expected = Current.Kind == SyntaxKind.LetKeyword ? SyntaxKind.LetKeyword : SyntaxKind.VarKeyword;
            var keyword = MatchToken(expected);
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            TypeClauseSyntax typeClause = null;
            if (Current.Kind == SyntaxKind.ColonToken)
            {
                typeClause = ParseTypeClause();
            }
            var equals = MatchToken(SyntaxKind.EqualsToken);
            var initializer = ParseExpression();
            return new VariableDeclarationSyntax(keyword, identifier, typeClause, equals, initializer);
        }

        private ExpressionStatementSyntax ParseExpressionStatement()
        {
            var expression = ParseExpression();
            return new ExpressionStatementSyntax(expression);
        }


        private ExpressionSyntax ParseExpression()
        {
            return ParseAssignmentExpression();

        }
        private ExpressionSyntax ParseAssignmentExpression()
        {
            var left = ParseBinaryExpression();

            if (Current.Kind == SyntaxKind.EqualsToken)
            {
                var equalsToken = MatchToken(SyntaxKind.EqualsToken);
                var right = ParseAssignmentExpression();
                return new AssignmentExpressionSyntax(left, equalsToken, right);
            }

            return left;
        }
        private ExpressionSyntax ParseBinaryExpression(int parentPrecedence = 0)
        {
            ExpressionSyntax left;
            var unaryOperatorPrecedence = Current.Kind.GetUnaryOperatorPrecedence();
            if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence)
            {
                var operatorToken = NextToken();
                var operand = ParseBinaryExpression(unaryOperatorPrecedence);
                left = new UnaryExpressionSyntax(operatorToken, operand);
            }
            else
            {
                left = ParsePostfixExpression();
            }

            while (true)
            {
                var precedence = Current.Kind.GetBinaryOperatorPrecedence();
                if (precedence == 0 || precedence <= parentPrecedence)
                    break;
                var operatorToken = NextToken();
                var right = ParseBinaryExpression(precedence);
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }
            return left;
        }


        private ExpressionSyntax ParsePrimaryExpression()
        {
            switch (Current.Kind)
            {
                case SyntaxKind.OpenParenthesisToken:
                    return ParseParenthesizedExpression();
                case SyntaxKind.OpenBracketToken:
                    return ParseArrayLiteralExpression();
                case SyntaxKind.FalseKeyword:
                case SyntaxKind.TrueKeyword:
                    return ParseBooleanLiteral();
                case SyntaxKind.NumberToken:
                    return ParseNumberLitheral();
                case SyntaxKind.FloatToken:
                    return ParseFloatLiteral();
                case SyntaxKind.CharToken:
                    return ParseCharLiteral();
                case SyntaxKind.StringToken:
                    return ParseStringLitheral();

                case SyntaxKind.IdentifierToken:
                default:
                    return ParseNameOrCallExpression();
            }
        }

        private ExpressionSyntax ParseArrayLiteralExpression()
        {
            var openBracketToken = MatchToken(SyntaxKind.OpenBracketToken);
            var elements = ParseArrayElements();
            var closeBracketToken = MatchToken(SyntaxKind.CloseBracketToken);
            return new ArrayLiteralExpressionSyntax(openBracketToken, elements, closeBracketToken);
        }

        private SeparatedSyntaxList<ExpressionSyntax> ParseArrayElements()
        {
            var nodesAndSeperators = ImmutableArray.CreateBuilder<SyntaxNode>();
            while (Current.Kind != SyntaxKind.CloseBracketToken &&
                   Current.Kind != SyntaxKind.EndOfFileToken)
            {
                var expression = ParseExpression();
                nodesAndSeperators.Add(expression);
                if (Current.Kind != SyntaxKind.CloseBracketToken)
                {
                    var comma = MatchToken(SyntaxKind.CommaToken);
                    nodesAndSeperators.Add(comma);
                }
            }
            return new SeparatedSyntaxList<ExpressionSyntax>(nodesAndSeperators.ToImmutable());
        }

        private ExpressionSyntax ParsePostfixExpression()
        {
            var expr = ParsePrimaryExpression();
            while (true)
            {
                if (Current.Kind == SyntaxKind.OpenBracketToken)
                {
                    var openBracketToken = MatchToken(SyntaxKind.OpenBracketToken);
                    var index = ParseExpression();
                    var closeBracketToken = MatchToken(SyntaxKind.CloseBracketToken);
                    expr = new ArrayAccessExpressionSyntax(expr, openBracketToken, index, closeBracketToken);
                }
                else
                {
                    break;
                }
            }
            return expr;
        }

        private ExpressionSyntax ParseNameOrCallExpression()
        {
            if (Peek(0).Kind == SyntaxKind.IdentifierToken && Peek(1).Kind == SyntaxKind.OpenParenthesisToken)
                return ParseCallExpression();
            return ParseNameExpression();
        }

        private ExpressionSyntax ParseCallExpression()
        {
            var identifer = MatchToken(SyntaxKind.IdentifierToken);
            var OpenParenthesisToken = MatchToken(SyntaxKind.OpenParenthesisToken);
            var arguments = ParseArguments();
            var CloseParenthesisToken = MatchToken(SyntaxKind.CloseParenthesisToken);
            return new CallExpressionSyntax(identifer, OpenParenthesisToken, arguments, CloseParenthesisToken);
        }

        private SeparatedSyntaxList<ExpressionSyntax> ParseArguments()
        {
            var nodesAndSeperators = ImmutableArray.CreateBuilder<SyntaxNode>();
            while (Current.Kind != SyntaxKind.CloseParenthesisToken &&
                  Current.Kind != SyntaxKind.EndOfFileToken)
            {
                var expression = ParseExpression();
                nodesAndSeperators.Add(expression);
                if (Current.Kind != SyntaxKind.CloseParenthesisToken)
                {
                    var comma = MatchToken(SyntaxKind.CommaToken);
                    nodesAndSeperators.Add(comma);
                }
            }
            return new SeparatedSyntaxList<ExpressionSyntax>(nodesAndSeperators.ToImmutable());
        }

        private ExpressionSyntax ParseParenthesizedExpression()
        {
            var left = MatchToken(SyntaxKind.OpenParenthesisToken);
            var expression = ParseExpression();
            var right = MatchToken(SyntaxKind.CloseParenthesisToken);
            return new ParenthesizedExpressionSyntax(left, expression, right);
        }
        private ExpressionSyntax ParseBooleanLiteral()
        {
            var isTrue = Current.Kind == SyntaxKind.TrueKeyword;
            var keywordToken = isTrue ? MatchToken(SyntaxKind.TrueKeyword) : MatchToken(SyntaxKind.FalseKeyword);
            return new LiteralExpressionSyntax(keywordToken, isTrue);
        }
        private ExpressionSyntax ParseNumberLitheral()
        {
            var numberToken = MatchToken(SyntaxKind.NumberToken);
            return new LiteralExpressionSyntax(numberToken);
        }
        private ExpressionSyntax ParseFloatLiteral()
        {
            var floatToken = MatchToken(SyntaxKind.FloatToken);
            return new LiteralExpressionSyntax(floatToken);
        }
        private ExpressionSyntax ParseCharLiteral()
        {
            var charToken = MatchToken(SyntaxKind.CharToken);
            return new LiteralExpressionSyntax(charToken);
        }
        private ExpressionSyntax ParseStringLitheral()
        {
            var stringToken = MatchToken(SyntaxKind.StringToken);
            return new LiteralExpressionSyntax(stringToken);
        }
        private ExpressionSyntax ParseNameExpression()
        {
            var identifierToken = MatchToken(SyntaxKind.IdentifierToken);

            return new NameExpressionSyntax(identifierToken);
        }

    }
}