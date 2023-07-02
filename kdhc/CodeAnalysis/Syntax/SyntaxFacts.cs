namespace KAEDEHARA_COMPILER.CodeAnalysis.Syntax
{
    internal static class SyntaxFacts 
    {
         public static int GetBinaryOperatorPrecedence(this SyntaxKind kind){
            switch (kind)
            {
               
                case SyntaxKind.StarToken:
                case SyntaxKind.SlashToken:
                    return 5 ;
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                    return 4 ;
                case SyntaxKind.EqualToken:
                case SyntaxKind.NotEqualToken:
                    return 3 ;
                case SyntaxKind.AmpersanToken:
                    return 2;
                case SyntaxKind.PipeToken:
                    return 1;
                default:
                    return 0 ;
            }
        }
         public static int GetUanaryOperatorPrecedence(this SyntaxKind kind){
            switch (kind)
            {
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                case SyntaxKind.BangToken:
                    return 6 ;
                
                default:
                    return 0 ;
            }
        }

        public static SyntaxKind GetKeywordKind(string text)
        {
            switch(text){
                case "true":
                    return SyntaxKind.TrueKeyword;
                case "false":
                    return SyntaxKind.FalseKeyword;
                default :
                    return SyntaxKind.IdentifierToken;
            }
        }
    }







}