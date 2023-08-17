namespace Kaedehara.CodeAnalysis.Syntax
{
    public static class SyntaxFacts
    {
        public static IEnumerable<SyntaxKind> GetBinaryOperatorKinds(){
                var kinds = (SyntaxKind[]) Enum.GetValues(typeof(SyntaxKind));
                foreach(var kind in kinds){
                    if(GetBinaryOperatorPrecedence(kind) >0){
                        yield return kind ;
                    }
                }

        }
           public static IEnumerable<SyntaxKind> GetUnaryOperatorKinds(){
                var kinds = (SyntaxKind[]) Enum.GetValues(typeof(SyntaxKind));
                foreach(var kind in kinds){
                    if(GetUnaryOperatorPrecedence(kind) >0){
                        yield return kind ;
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
                    return 3;
                case SyntaxKind.AmpersanToken:
                    return 2;
                case SyntaxKind.PipeToken:
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
                case "let":
                    return SyntaxKind.LetKeyword;
                case "false":
                    return SyntaxKind.FalseKeyword;
                case "var":
                    return SyntaxKind.VarKeyword;
                default:
                    return SyntaxKind.IdentifierToken;
            }
        }
        public static string GetText(SyntaxKind kind){
            switch(kind)
            {
                case SyntaxKind.PlusToken:
                    return "+" ;
                case SyntaxKind.MinusToken:
                    return "-" ; 
                case SyntaxKind.StarToken:
                    return "*" ; 
                case SyntaxKind.SlashToken:
                    return "/" ; 
                case SyntaxKind.BangToken:
                    return "!" ; 
                case SyntaxKind.EqualsToken:
                    return "=" ; 
                case SyntaxKind.AmpersanToken:
                    return "&&" ; 
                case SyntaxKind.PipeToken:
                    return "||" ; 
                case SyntaxKind.EqualEqualToken:
                    return "==" ; 
                case SyntaxKind.NotEqualToken:
                    return "!=" ; 
                case SyntaxKind.OpenParenthesisToken:
                    return "(" ; 
                case SyntaxKind.CloseParenthesisToken:
                    return ")" ; 
                case SyntaxKind.OpenBraceToken:
                    return "{" ; 
                case SyntaxKind.CloseBraceToken:
                    return "}" ; 
                case SyntaxKind.FalseKeyword:
                    return "false" ; 
                case SyntaxKind.TrueKeyword:
                    return "true" ;     
                case SyntaxKind.LetKeyword:
                    return "let" ;
                case SyntaxKind.VarKeyword:
                    return "var" ;
                default:
                    return null ; 
            }
        
        }
    }







}