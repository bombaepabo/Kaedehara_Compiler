namespace Kaedehara.CodeAnalysis.Syntax
{
    public static class SyntaxFacts
    {
        public static IEnumerable<SyntaxKind> GetBinaryOperatorKinds()
        {
            var kinds = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
            foreach (var kind in kinds)
            {
                if (GetBinaryOperatorPrecedence(kind) > 0)
                {
                    yield return kind;
                }
            }

        }
        public static IEnumerable<SyntaxKind> GetUnaryOperatorKinds()
        {
            var kinds = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
            foreach (var kind in kinds)
            {
                if (GetUnaryOperatorPrecedence(kind) > 0)
                {
                    yield return kind;
                }
            }

        }
        public static int GetBinaryOperatorPrecedence(this SyntaxKind kind)
        {
            switch (kind)
            {

                case SyntaxKind.StarToken:
                case SyntaxKind.SlashToken:
                    return 5;
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                    return 4;
                case SyntaxKind.EqualEqualToken:
                case SyntaxKind.NotEqualToken:
                case SyntaxKind.LessToken:
                case SyntaxKind.LessOrEqualsToken:
                case SyntaxKind.GreatToken:
                case SyntaxKind.GreaterOrEqualsToken:
                    return 3;
                case SyntaxKind.AmpersanToken:
                case SyntaxKind.AmpersanAmpersanToken:
                    return 2;
                case SyntaxKind.PipePipeToken:
                case SyntaxKind.PipeToken:
                case SyntaxKind.HatToken:
                    return 1;
                default:
                    return 0;
            }
        }
        public static int GetUnaryOperatorPrecedence(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                case SyntaxKind.BangToken:
                case SyntaxKind.TildeToken:
                    return 6;

                default:
                    return 0;
            }
        }

        public static SyntaxKind GetKeywordKind(string text)
        {
            switch (text)
            {
                case "true":
                    return SyntaxKind.TrueKeyword;
                case "if":
                    return SyntaxKind.IfKeyword;
                case "else":
                    return SyntaxKind.ElseKeyword;
                case "let":
                    return SyntaxKind.LetKeyword;
                case "false":
                    return SyntaxKind.FalseKeyword;
                case "for":
                    return SyntaxKind.ForKeyword;
                case "var":
                    return SyntaxKind.VarKeyword;
                case "while":
                    return SyntaxKind.WhileKeyword;
                case "to":
                    return SyntaxKind.ToKeyword;
                default:
                    return SyntaxKind.IdentifierToken;
            }
        }
        public static string GetText(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.PlusToken:
                    return "+";
                case SyntaxKind.MinusToken:
                    return "-";
                case SyntaxKind.StarToken:
                    return "*";
                case SyntaxKind.SlashToken:
                    return "/";
                case SyntaxKind.BangToken:
                    return "!";
                case SyntaxKind.EqualsToken:
                    return "=";
                 case SyntaxKind.TildeToken:
                    return "~";
                case SyntaxKind.LessToken:
                    return "<";
                case SyntaxKind.LessOrEqualsToken:
                    return "<=";
                case SyntaxKind.GreatToken:
                    return ">";
                case SyntaxKind.GreaterOrEqualsToken:
                    return ">=";
                case SyntaxKind.AmpersanToken:
                    return "&";
                case SyntaxKind.AmpersanAmpersanToken:
                    return "&&";
                case SyntaxKind.PipeToken:
                    return "|";
                case SyntaxKind.PipePipeToken:
                    return "||";
                 case SyntaxKind.HatToken:
                    return "^";
                case SyntaxKind.EqualEqualToken:
                    return "==";
                case SyntaxKind.NotEqualToken:
                    return "!=";
                case SyntaxKind.OpenParenthesisToken:
                    return "(";
                case SyntaxKind.CloseParenthesisToken:
                    return ")";
                case SyntaxKind.OpenBraceToken:
                    return "{";
                case SyntaxKind.CloseBraceToken:
                    return "}";
                case SyntaxKind.FalseKeyword:
                    return "false";
                case SyntaxKind.TrueKeyword:
                    return "true";
                case SyntaxKind.LetKeyword:
                    return "let";
                case SyntaxKind.VarKeyword:
                    return "var";
                case SyntaxKind.ElseKeyword:
                    return "else";
                case SyntaxKind.IfKeyword:
                    return "if";
                case SyntaxKind.WhileKeyword:
                    return "while";
                case SyntaxKind.ForKeyword:
                    return "for";
                case SyntaxKind.ToKeyword:
                    return "to";
                default:
                    return null;
            }

        }
    }







}