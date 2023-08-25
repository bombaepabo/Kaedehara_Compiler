using Kaedehara.CodeAnalysis.Syntax;
namespace Kaedehara.Tests.CodeAnalysis.Syntax;
public class LexerTest
{   [Fact]
    public void Lexer_Tests_AllTokens()
    {
        var tokenKinds = Enum.GetValues(typeof(SyntaxKind))
                            .Cast<SyntaxKind>()
                            .Where(k => k.ToString().EndsWith("Keyword") || 
                            k.ToString().EndsWith("Token"));
        var testedTokenKinds = GetTokens().Concat(GetSeperators()).Select(t=>t.kind);
        var untestTokenKinds = new SortedSet<SyntaxKind>(tokenKinds);
        untestTokenKinds.Remove(SyntaxKind.BadToken);
        untestTokenKinds.Remove(SyntaxKind.EndOfFileToken);
        untestTokenKinds.ExceptWith(testedTokenKinds);
        Assert.Empty(untestTokenKinds);
    }
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
        Assert.Equal(t1Kind,tokens[0].Kind);
        Assert.Equal(t1Text,tokens[0].Text);
        Assert.Equal(t2Kind,tokens[1].Kind);
        Assert.Equal(t2Text,tokens[1].Text);

    }
     [Theory]
    [MemberData(nameof(GetTokenPairsWithSeperatorData))]
    public void Lexer_Lexes_TokenPairs_WithSeparators(SyntaxKind t1Kind, string t1Text,SyntaxKind seperatorKind,string seperatorText,SyntaxKind t2Kind, string t2Text)
    {
        var text = t1Text + seperatorText + t2Text;
        var tokens = SyntaxTree.ParseToken(text).ToArray();
        Assert.Equal(3,tokens.Length);
        Assert.Equal(t1Kind,tokens[0].Kind);
        Assert.Equal(t1Text,tokens[0].Text);

        Assert.Equal(seperatorKind,tokens[1].Kind);
        Assert.Equal(seperatorText,tokens[1].Text);

        Assert.Equal(t2Kind,tokens[2].Kind);
        Assert.Equal(t2Text,tokens[2].Text);
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
      public static IEnumerable<object[]> GetTokenPairsWithSeperatorData()
    {
        foreach (var t in GetTokenPairsWithSeperator())
        {
            yield return new object[] { t.t1Kind,t.t1Text,t.seperatorKind,t.seperatorText,t.t2Kind,t.t2Text};
        }
    }
    public static IEnumerable<(SyntaxKind kind, string text)> GetTokens()
    {
        var fixedTokens = Enum.GetValues(typeof(SyntaxKind)).Cast<SyntaxKind>().Select(k => (kind: k, text: SyntaxFacts.GetText(k))).Where(t => t.text != null);
        var dynamicTokens = new[]
        {
            (SyntaxKind.NumberToken,"1"),
            (SyntaxKind.NumberToken,"123"),
            (SyntaxKind.IdentifierToken,"a"),
            (SyntaxKind.IdentifierToken,"abc"),
        };
        return fixedTokens.Concat(dynamicTokens);

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
         if(t1Kind == SyntaxKind.LessToken && t2Kind == SyntaxKind.EqualsToken){
            return true ;
        }if(t1Kind == SyntaxKind.LessToken && t2Kind == SyntaxKind.EqualEqualToken){
            return true ;
        }
         if(t1Kind == SyntaxKind.GreatToken && t2Kind == SyntaxKind.EqualsToken){
            return true ;
        }if(t1Kind == SyntaxKind.GreatToken && t2Kind == SyntaxKind.EqualEqualToken){
            return true ;
        }
        if(t1Kind == SyntaxKind.AmpersanToken && t2Kind == SyntaxKind.AmpersanToken){
            return true ;
        }
        if(t1Kind == SyntaxKind.AmpersanToken && t2Kind == SyntaxKind.AmpersanAmpersanToken){
            return true ;
        }
         if(t1Kind == SyntaxKind.PipeToken && t2Kind == SyntaxKind.PipeToken){
            return true ;
        }
        if(t1Kind == SyntaxKind.PipeToken && t2Kind == SyntaxKind.PipePipeToken){
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
  private static IEnumerable<(SyntaxKind t1Kind,string t1Text,SyntaxKind seperatorKind,string seperatorText,SyntaxKind t2Kind,string t2Text)> GetTokenPairsWithSeperator()
    {
        foreach (var t1 in GetTokens()){
            foreach (var t2 in GetTokens()){
                if(!RequiresSeparator(t1.kind,t2.kind)){
                    foreach(var s in GetSeperators()){
                        yield return (t1.kind,t1.text,s.kind,s.text,t2.kind,t2.text);
                    }
            
            }
        }
    }
}
}