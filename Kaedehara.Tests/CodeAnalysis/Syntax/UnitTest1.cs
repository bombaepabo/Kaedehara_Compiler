using Kaedehara.CodeAnalysis.Syntax;
namespace Kaedehara.Tests.CodeAnalysis.Syntax;
public class UnitTest1
{
    [Theory]
    [MemberData(nameof(GetTokensData))]
    public void Lexer_Lexes_Token(SyntaxKind kind, string text)
    {
        var tokens = SyntaxTree.ParseToken(text);
        var token = Assert.Single(tokens);
        Assert.Equal(kind, token.Kind);
        Assert.Equal(text, token.Text);
    }
    [Theory]
    [MemberData(nameof(GetTokenPairsData))]
    public void Lexer_Lexes_TokenPairs(SyntaxKind t1Kind, string t1Text,SyntaxKind t2Kind, string t2Text)
    {
        var text = t1Text + t2Text;
        var tokens = SyntaxTree.ParseToken(text).ToArray();
        Assert.Equal(2,tokens.Length);
        Assert.Equal(tokens[0].Kind, t1Kind);
        Assert.Equal(tokens[0].Text, t1Text);

        Assert.Equal(tokens[1].Kind, t2Kind);
        Assert.Equal(tokens[1].Text, t2Text);
    }
    public static IEnumerable<object[]> GetTokensData()
    {
        foreach (var t in GetTokens().Concat(GetSeperators()))
        {
            yield return new object[] { t.kind, t.text };
        }
    }
    public static IEnumerable<object[]> GetTokenPairsData()
    {
        foreach (var t in GetTokenPairs())
        {
            yield return new object[] { t.t1Kind,t.t1Text,t.t2Kind,t.t2Text};
        }
    }
    public static IEnumerable<(SyntaxKind kind, string text)> GetTokens()
    {
        return new[]{

            (SyntaxKind.PlusToken,"+"),
            (SyntaxKind.MinusToken,"-"),
            (SyntaxKind.StarToken,"*"),
            (SyntaxKind.SlashToken,"/"),
            (SyntaxKind.BangToken,"!"),
            (SyntaxKind.EqualsToken,"="),
            (SyntaxKind.AmpersanToken,"&&"),
            (SyntaxKind.PipeToken,"||"),
            (SyntaxKind.EqualEqualToken,"=="),
            (SyntaxKind.NotEqualToken,"!="),
            (SyntaxKind.OpenParenthesisToken,"("),
            (SyntaxKind.CloseParenthesisToken,")"),

            (SyntaxKind.FalseKeyword,"false"),
            (SyntaxKind.TrueKeyword,"true"),
            (SyntaxKind.NumberToken,"1"),
            (SyntaxKind.NumberToken,"123"),
            (SyntaxKind.IdentifierToken,"a"),
            (SyntaxKind.IdentifierToken,"abc"),
        };
    }
     public static IEnumerable<(SyntaxKind kind, string text)> GetSeperators()
    {
        return new[]{

         
            (SyntaxKind.WhitespaceToken," "),
            (SyntaxKind.WhitespaceToken,"  "),
            (SyntaxKind.WhitespaceToken,"\r"),
            (SyntaxKind.WhitespaceToken,"\n"),
            (SyntaxKind.WhitespaceToken,"\r\n"),
         
        };
    }
    private static bool RequiresSeparator(SyntaxKind t1Kind,SyntaxKind t2Kind){
        var t1IsKeyword = t1Kind.ToString().EndsWith("Keyword");
        var t2IsKeyword = t2Kind.ToString().EndsWith("Keyword");

        if(t1Kind == SyntaxKind.IdentifierToken && t2Kind == SyntaxKind.IdentifierToken){
            return true ;

        }
        if(t1IsKeyword && t2IsKeyword){
            return true ;
        }
        if(t1IsKeyword && t2Kind ==SyntaxKind.IdentifierToken){
            return true ;
        }
        if(t1Kind == SyntaxKind.IdentifierToken &&t2IsKeyword){
            return true ;
        }
         if(t1Kind == SyntaxKind.NumberToken && t2Kind == SyntaxKind.NumberToken){
            return true ;

        }
        if(t1Kind == SyntaxKind.BangToken && t2Kind == SyntaxKind.EqualsToken){
            return true ;

        }
         if(t1Kind == SyntaxKind.BangToken && t2Kind == SyntaxKind.EqualEqualToken){
            return true ;

        }
        if(t1Kind == SyntaxKind.EqualsToken && t2Kind == SyntaxKind.EqualsToken){
            return true ;

        }
         if(t1Kind == SyntaxKind.EqualsToken && t2Kind == SyntaxKind.EqualEqualToken){
            return true ;

        }
        return false ;
    }
    private static IEnumerable<(SyntaxKind t1Kind,string t1Text,SyntaxKind t2Kind,string t2Text)> GetTokenPairs()
    {
        foreach (var t1 in GetTokens()){
            foreach (var t2 in GetTokens()){
                if(!RequiresSeparator(t1.kind,t2.kind)){
                yield return (t1.kind,t1.text,t2.kind,t2.text);
            
            }
        }
    }
}
}