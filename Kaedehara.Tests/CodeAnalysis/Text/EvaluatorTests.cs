using System.Numerics;
using Kaedehara.CodeAnalysis;
using Kaedehara.CodeAnalysis.Binding;
using Kaedehara.CodeAnalysis.Symbols;
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
    [InlineData("~1", -2)]

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
    [InlineData("true && true", true)]
    [InlineData("false || false", false)]

    [InlineData("false | false", false)]
    [InlineData("false | true", true)]
    [InlineData("true | false", true)]
    [InlineData("true | true", true)]

    [InlineData("false & false", false)]
    [InlineData("false & true", false)]
    [InlineData("true & false", false)]
    [InlineData("true & true", true)]

    [InlineData("false ^ false", false)]
    [InlineData("true ^ false", true)]
    [InlineData("false ^ true", true)]
    [InlineData("true ^ true", false)]

    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("!true", false)]
    [InlineData("!false", true)]
    [InlineData("{var a = 0 (a = 10) * a}", 100)]
    [InlineData("3 < 4",true)]
    [InlineData("5 < 4",false)]
    [InlineData("4 <= 4",true)]
    [InlineData("4 <= 5",true)]
    [InlineData("5 <= 4",false)]

    [InlineData("4 > 3",true)]
    [InlineData("4 > 5",false)]
    [InlineData("4 >= 4",true)]
    [InlineData("5 >= 4",true)]
    [InlineData("4 >= 5",false)]

    [InlineData("1 | 2 ",3)]
    [InlineData("1 | 0 ",1)]
    [InlineData("1 & 3 ",1)]
    [InlineData("1 & 0 ",0)]
    [InlineData("1 ^ 0 ",1)]
    [InlineData("0 ^ 1 ",1)]
    [InlineData("1 ^ 3 ",2)]


    [InlineData("{var a = 0 if a == 0 a = 10 a}", 10)]
    [InlineData("{var a = 0 if a == 4 a = 10 a}", 0)]

    [InlineData("{var a = 0 if a == 0 a = 10 else a = 5 a}", 10)]
    [InlineData("{var a = 0 if a == 4 a = 10 else a = 5 a}", 5)]

    [InlineData("{var i = 10 var result = 0 while i > 0 {result = result + i i = i - 1} result }",55)]
    [InlineData("{var result = 0 for i = 0 to 10 { result = result + i } result }", 55)]
    [InlineData("{var a = 10 for i =1 to (a = a - 1) {} a}",9)]

    [InlineData("2.5 + 3.0", 5.5)]
    [InlineData("5.5 - 1.5", 4.0)]
    [InlineData("2.0 * 3.5", 7.0)]
    [InlineData("9.0 / 2.0", 4.5)]
    [InlineData("-3.5", -3.5)]
    [InlineData("+3.5", 3.5)]
    [InlineData("2 + 3.5", 5.5)]
    [InlineData("3.5 + 2", 5.5)]
    [InlineData("5 * 2.0", 10.0)]
    [InlineData("9 / 2.0", 4.5)]
    [InlineData("3.5 < 4.0", true)]
    [InlineData("3.5 >= 3.5", true)]
    [InlineData("3.5 == 3.5", true)]
    [InlineData("3.5 != 2.0", true)]
    [InlineData("'a'", 'a')]
    [InlineData("'a' == 'a'", true)]
    [InlineData("'a' != 'b'", true)]
    [InlineData("'a' < 'b'", true)]
    [InlineData("'c' >= 'b'", true)]
    [InlineData("string(3.5)", "3.5")]
    [InlineData("float(10)", 10.0)]
    [InlineData("int(3.5)", 3)]
    [InlineData("char(\"abc\")", 'a')]
    [InlineData("int('a')", 97)]
    [InlineData("char(97)", 'a')]

    public void Evaluator_Computes_CorrectValues(string text, object expectedValue)
    {
        AssertValue(text, expectedValue);

    }

    private static void AssertValue(string text, object expectedValue)
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
     [Fact]
    public void Evaluator_NameExpression_Reports_Undefined()
    {
        var text = @"[x] * 10";
        var diagnostics = @"
            Variables 'x' doesn't exist.
        ";

        AssertDiagnostics(text, diagnostics);
    }
      [Fact]
    public void Evaluator_NameExpression_Reports_NoErrorForInsertedToken()
    {
        var text = @"[]";
        var diagnostics = @"
            Unexpected token <EndOfFileToken>, expected <IdentifierToken>
        ";

        AssertDiagnostics(text, diagnostics);
    }
      [Fact]
    public void Evaluator_BlockStatement_NoInfiniteLoop()
    {
        var text = @"
        {
          [)][]
        
        
        ";
        var diagnostics = @"
           Unexpected token <CloseParenthesisToken>, expected <IdentifierToken>
           Unexpected token <EndOfFileToken>, expected <CloseBraceToken>
        ";

        AssertDiagnostics(text, diagnostics);
    }
     [Fact]
    public void Evaluator_IfStatement_Reports_CannotConvert()
    {
        var text = @"
        {
            var x = 10 
            if[10]
                x = 10
        }
        
        ";
        var diagnostics = @"
            Cannot convert type 'int' to 'bool'.
        ";

        AssertDiagnostics(text, diagnostics);
    }
      [Fact]
    public void Evaluator_WhileStatement_Reports_CannotConvert()
    {
        var text = @"
        {
            var x = 10 
            while [10]
                x = 10
        }
        
        ";
        var diagnostics = @"
            Cannot convert type 'int' to 'bool'.
        ";

        AssertDiagnostics(text, diagnostics);
    }
       [Fact]
    public void Evaluator_ForStatement_Reports_CannotConvert_LowerBound()
    {
        var text = @"
        {
            var result = 0 
            for i = [false] to 10
                result = result +i
        }
        
        ";
        var diagnostics = @"
            Cannot convert type 'bool' to 'int'.
        ";

        AssertDiagnostics(text, diagnostics);
    }
    [Fact]
    public void Evaluator_ForStatement_Reports_CannotConvert_UpperBound()
    {
        var text = @"
        {
            var result = 0 
            for i = 1  to [true]
                result = result +i
        }
        
        ";
        var diagnostics = @"
            Cannot convert type 'bool' to 'int'.
        ";

        AssertDiagnostics(text, diagnostics);
    }
    [Fact]
    public void Evaluator_AssignmentExpression_Reports_CannotAssign()
    {
        var text = @"
        {
            let x = 10 
            x [=] 0
        }
        
        ";
        var diagnostics = @"
            Variable 'x' is read-only and cannot be assigned to.
        ";

        AssertDiagnostics(text, diagnostics);
    }
     [Fact]
    public void Evaluator_AssignmentExpression_Reports_CannotConvert()
    {
        var text = @"
        {
            var x = 10 
            x = [true]
        }
        
        ";
        var diagnostics = @"
            Cannot convert type 'bool' to 'int'.
        ";

        AssertDiagnostics(text, diagnostics);
    }
    [Fact]
     public void Evaluator_AssignmentExpression_Reports_Undefined()
        {  
            var text = @"[x] = 10";
            var diagnostics = @"
                Variables 'x' doesn't exist.
            ";

            AssertDiagnostics(text, diagnostics);
        }

     [Fact]
    public void Evaluator_UnaryExpression_Reports_Undefined()
    {
        var text = @"[+]true";
        var diagnostics = @"
            Unary operator '+' is not defined for type 'bool'.
        ";

        AssertDiagnostics(text, diagnostics);
    }
    [Fact]
    public void Evaluator_BinaryExpression_Reports_Undefined()
    {
        var text = @"10 [*] false";
        var diagnostics = @"
            Binary operator '*' is not defined for types 'int' and 'bool'.
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

    [Theory]
    [InlineData("1 + 2", 3)]
    [InlineData("3 * 4 + 2", 14)]
    [InlineData("true && false", false)]
    [InlineData("string(3.5)", "3.5")]
    [InlineData("int(5.5)", 5)]
    [InlineData("1.25 + 2.25", 3.5)]
    [InlineData("'a'", 'a')]
    public void Evaluator_ConstantFolding_Folds_Expressions(string text, object expectedValue)
    {
        var syntaxTree = SyntaxTree.Parse(text);
        var compilation = new Compilation(syntaxTree);
        var statement = compilation.GlobalScope.Statement;
        var exprStatement = Assert.IsType<BoundExpressionStatement>(statement);
        var literalExpr = Assert.IsType<BoundLiteralExpression>(exprStatement.Expression);
        Assert.Equal(expectedValue, literalExpr.Value);
    }

    [Fact]
    public void Evaluator_Functions_Evaluate_Correctly()
    {
        AssertValue(@"
        {
            fn add(x: int, y: int) -> int {
                return x + y
            }
            add(5, 10)
        }
        ", 15);
    }

    [Fact]
    public void Evaluator_Functions_TypePromotion()
    {
        AssertValue(@"
        {
            fn add(x: float, y: float) -> float {
                return x + y
            }
            add(5, 10.5)
        }
        ", 15.5);
    }

    [Fact]
    public void Evaluator_Functions_Scoping()
    {
        AssertValue(@"
        {
            var x = 10
            fn foo(x: int) -> int {
                return x * 2
            }
            foo(5) + x
        }
        ", 20);
    }

    [Fact]
    public void Evaluator_Functions_Recursion()
    {
        AssertValue(@"
        {
            fn fib(n: int) -> int {
                if n < 2 {
                    return n
                }
                return fib(n - 1) + fib(n - 2)
            }
            fib(6)
        }
        ", 8);
    }

    [Fact]
    public void Evaluator_Functions_VoidReturn()
    {
        AssertValue(@"
        {
            var x = 0
            fn setX(val: int) {
                x = val
            }
            setX(10)
            x
        }
        ", 10);
    }

    [Fact]
    public void Evaluator_Functions_Report_UndefinedType()
    {
        var text = @"
        {
            fn add(x: [badtype]) {}
        }
        ";
        var diagnostics = @"
            Type 'badtype' doesn't exist.
        ";
        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void Evaluator_Functions_Report_InvalidReturnType()
    {
        var text = @"
        {
            fn add(x: int) -> int {
                return [false]
            }
        }
        ";
        var diagnostics = @"
            Function return type is 'int' but return statement expression type is 'bool'.
        ";
        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void Evaluator_Functions_Report_AllPathsMustReturn()
    {
        var text = @"
        {
            fn [add](x: int) -> int {
                if x > 0 {
                    return 1
                }
            }
        }
        ";
        var diagnostics = @"
            Not all code paths return a value.
        ";
        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void Evaluator_Functions_Report_InvalidReturnStatement()
    {
        var text = @"
        [return] 10
        ";
        var diagnostics = @"
            The 'return' keyword can only be used inside a function.
        ";
        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void Evaluator_Functions_Report_DuplicateDeclaration()
    {
        var text = @"
        {
            fn foo() {}
            fn [foo]() {}
        }
        ";
        var diagnostics = @"
            Function 'foo' is already declared.
        ";
        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void Evaluator_Arrays_CanDeclareAndAccess()
    {
        var text = @"
        {
            let arr = [1, 2, 3]
            arr[0] + arr[1] + arr[2]
        }
        ";
        AssertValue(text, 6);
    }

    [Fact]
    public void Evaluator_Arrays_CanMutate()
    {
        var text = @"
        {
            let arr = [1, 2, 3]
            arr[1] = 20
            arr[0] + arr[1] + arr[2]
        }
        ";
        AssertValue(text, 24);
    }

    [Fact]
    public void Evaluator_Arrays_WithExplicitTypeClause()
    {
        var text = @"
        {
            let arr: int[] = [1, 2, 3]
            arr[0]
        }
        ";
        AssertValue(text, 1);
    }

    [Fact]
    public void Evaluator_Arrays_PromoteElementTypes()
    {
        var text = @"
        {
            let arr = [1, 2.5]
            arr[0] + arr[1]
        }
        ";
        AssertValue(text, 3.5);
    }

    [Fact]
    public void Evaluator_Arrays_Report_CannotIndex()
    {
        var text = @"
        {
            let x = 10
            x[0]
        }
        ";
        var diagnostics = @"
            Cannot index into a value of type 'int'.
        ";
        AssertArrayDiagnostics(text, diagnostics);
    }

    [Fact]
    public void Evaluator_Arrays_Report_IndexMustBeInt()
    {
        var text = @"
        {
            let arr = [1, 2]
            arr[true]
        }
        ";
        var diagnostics = @"
            Index must be an integer, but was 'bool'.
        ";
        AssertArrayDiagnostics(text, diagnostics);
    }

    [Fact]
    public void Evaluator_Arrays_Report_CannotConvertType()
    {
        var text = @"
        {
            let arr: int[] = [true]
        }
        ";
        var diagnostics = @"
            Cannot convert type 'bool[]' to 'int[]'.
        ";
        AssertArrayDiagnostics(text, diagnostics);
    }

    private static void AssertArrayDiagnostics(string text, string expectedDiagnosticText)
    {
        var syntaxTree = SyntaxTree.Parse(text);
        var compilation = new Compilation(syntaxTree);
        var result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());

        var expectedDiagnostics = AnnotatedText.UnindentLines(expectedDiagnosticText);
        Assert.Equal(expectedDiagnostics.Length, result.Diagnostics.Length);
        for (var i = 0; i < expectedDiagnostics.Length; i++)
        {
            var expectedMessage = expectedDiagnostics[i];
            var actualMessage = result.Diagnostics[i].Message;
            Assert.Equal(expectedMessage, actualMessage);
        }
    }
}
