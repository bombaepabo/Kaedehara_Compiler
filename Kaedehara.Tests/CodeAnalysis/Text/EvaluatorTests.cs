using System.Numerics;
using Kaedehara.CodeAnalysis;
using Kaedehara.CodeAnalysis.Syntax;
using NuGet.Frameworks;
using Xunit;
namespace Kaedehara.Tests.CodeAnalysis;
public class EvaluatorTests
{
    [Theory]
    [InlineData("1", 1)]
    [InlineData("+1", 1)]
    [InlineData("-1", -1)]
    [InlineData("14 + 12", 26)]
    [InlineData("12 - 3", 9)]
    [InlineData("4 * 2", 8)]
    [InlineData("9 / 3", 3)]
    [InlineData("(10)", 10)]
    [InlineData("12 == 3", false)]
    [InlineData("3 == 3", true)]
    [InlineData("12 != 3", true)]
    [InlineData("3 != 3", false)]
    [InlineData("false == false", true)]
    [InlineData("true == false", false)]
    [InlineData("false != false", false)]
    [InlineData("true != false", true)]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("!true", false)]
    [InlineData("!false", true)]
    [InlineData("{var a = 0 (a = 10) * a}", 100)]
    public void SyntaxFacts_GetText_RoundTrips(string text, object expectedValue)
    {
        var syntaxTree = SyntaxTree.Parse(text);
        var compilation = new Compilation(syntaxTree);
        var variables = new Dictionary<VariableSymbol, object>();
        var result = compilation.Evaluate(variables);
        Assert.Empty(result.Diagnostics);
        Assert.Equal(expectedValue, result.Value);

    }
    [Fact]
    public void Evaluator_VariableDeclaration_Reports_Redeclaration()
    {
        var text = @"
        {
            var x = 10
            var y = 100
            {
                var x = 10
            }
            var [x] = 5
        }
        ";
        var diagnostics = @"
            Variable 'x' is already declared.
        ";

        AssertDiagnostics(text, diagnostics);
    }
    private void AssertDiagnostics(string text, string diagnosticText)
    {
        var annotatedText = AnnotatedText.Parse(text);
        var syntaxTree = SyntaxTree.Parse(annotatedText.Text);
        var compilation = new Compilation(syntaxTree);
        var result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());

        var expectedDiagnostic = AnnotatedText.UnindentLines(diagnosticText);
        if (annotatedText.Spans.Length != expectedDiagnostic.Length)
        {
            throw new Exception("ERROR: must mark as many spans as there are expected diagnostics");

        }
        Assert.Equal(expectedDiagnostic.Length, result.Diagnostics.Length);
        for (var i = 0; i < expectedDiagnostic.Length; i++)
        {
            var expectedMessage = expectedDiagnostic[i];
            var actualMessage = result.Diagnostics[i].Message;
            Assert.Equal(expectedMessage, actualMessage);

            var expectedSpan = annotatedText.Spans[i];
            var actualSpan = result.Diagnostics[i].Span;
            Assert.Equal(expectedSpan, actualSpan);


        }
    }

}
