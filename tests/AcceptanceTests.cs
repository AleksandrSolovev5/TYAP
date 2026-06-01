using System.ComponentModel;
using System.Diagnostics;
using System.Text;

using Interpreter;

using VirtualMachine;

using Xunit;

namespace Tests;

public class AcceptanceTests
{
    private readonly string _testDataPath;

    public AcceptanceTests()
    {
        _testDataPath = AppDomain.CurrentDomain.BaseDirectory;
    }

    /// <summary>
    /// приёмочные тесты для отдельных функций языка
    /// </summary>
    [Fact]
    public void FeatureOutputInteger()
    {
        string output = RunInterpreter("Features/Integer/OutputInteger.clvr");
        Assert.Equal("42 / -42", output);
    }

    [Fact]
    public void FeatureOutputFloatingPoint()
    {
        string output = RunInterpreter("Features/Float/OutputFloatingPoint.clvr");
        Assert.Contains("3.14159", output);
        Assert.Contains("2.7118281828", output);
    }

    [Fact]
    public void FeatureOutputStringLiteral()
    {
        string output = RunInterpreter("Features/String/OutputStringLiteral.clvr");
        Assert.Contains("abcdefghijklmnopqrstuvwxyz", output);
        Assert.Contains("ABCDEFGHIJKLMNOPQRSTUVWXYZ", output);
    }

    [Fact]
    public void FeatureCommentSingleLine()
    {
        string output = RunInterpreter("Features/CommentSingleLine.clvr");
        Assert.Equal("хорошыйтест", output);
        Assert.DoesNotContain("коммент", output);
    }

    [Fact]
    public void FeatureCommentMultiLine()
    {
        string output = RunInterpreter("Features/CommentMultiLine.clvr");
        Assert.Equal("MAX", output);
        Assert.DoesNotContain("VPN", output);
    }

    [Fact]
    public void FeatureOutputFloatVariable()
    {
        string output = RunInterpreter("Features/Float/OutputFloatVariable.clvr");
        Assert.Contains("42", output);
    }

    [Fact]
    public void FeatureOutputFloatExpression()
    {
        string output = RunInterpreter("Features/Float/OutputFloatExpression.clvr");
        Assert.Contains("65.4", output);
    }

    [Fact]
    public void FeatureOutputFloatOperations()
    {
        string output = RunInterpreter("Features/Float/OutputFloatOperations.clvr");
        Assert.Contains("2", output);
        Assert.Contains("6", output);
        Assert.Contains("8", output);
    }

    [Fact]
    public void FeatureTryingToChangeConstantValue()
    {
        Exception ex = Assert.Throws<Exception>(() => RunInterpreter("Features/Negative/TryingChangeConstValue.clvr"));
        Assert.Contains("Cannot assign value to constant", ex.Message);
    }

    [Fact]
    public void FeatureTryingInputToConstant()
    {
        Exception ex = Assert.Throws<Exception>(() => RunInterpreter("Features/Negative/TryingInputToConstant.clvr"));
        Assert.Contains("Cannot input value into constant", ex.Message);
    }

    [Fact]
    public void FeatureInputCorrectFloat()
    {
        const string envCourse = "2.71";
        IRuntimeEnvironment env = new TestRuntimeEnvironment(envCourse);
        string output = RunInterpreter("Features/Float/InputCorrectFloat.clvr", env);
        Assert.Contains(envCourse, output);
    }

    [Fact]
    public void FeatureFloatUnaryOperations()
    {
        IRuntimeEnvironment env = new TestRuntimeEnvironment("-3.14", "+23.8");
        string output = RunInterpreter("Features/Float/InputFloatWithUnaryOpers.clvr", env);
        Assert.Contains("-6.28", output);
        Assert.Contains("47.6", output);
    }

    [Fact]
    public void FeatureInputIncorrectFloat()
    {
        IRuntimeEnvironment env = new TestRuntimeEnvironment("osas");
        Exception ex = Assert.Throws<Exception>(() => RunInterpreter("Features/Float/InputIncorrectFloat.clvr", env));
        Assert.Contains("Invalid float input", ex.Message); // VMExecutor
    }

    [Fact]
    public void FeatureTryToModFloats()
    {
        Exception ex = Assert.Throws<Exception>(() => RunInterpreter("Features/Float/TryToModFloats.clvr"));
        Assert.Contains("Operator '%' cannot be applied to float operands", ex.Message); // SemanticAnalyzer
    }

    [Fact]
    public void FeatureFloatDivisionByZero()
    {
        Exception ex = Assert.Throws<Exception>(() => RunInterpreter("Features/Float/FloatDivisionByZero.clvr"));
        Assert.Contains("Division by zero.", ex.Message);
    }

    [Fact]
    public void FeatureInputCorrectInt()
    {
        const string envCourse = "42";
        IRuntimeEnvironment env = new TestRuntimeEnvironment(envCourse);
        string output = RunInterpreter("Features/Integer/InputCorrectInteger.clvr", env);
        Assert.Contains("-" + envCourse, output);
    }

    [Fact]
    public void FeatureInputIncorrectInt()
    {
        IRuntimeEnvironment env = new TestRuntimeEnvironment("22.5"); // падает даже так
        Exception ex = Assert.Throws<Exception>(() => RunInterpreter("Features/Integer/InputIncorrectInteger.clvr", env));
        Assert.Contains("Invalid int input", ex.Message); // VMExecutor
    }

    [Fact]
    public void FeatureIntegerAddition() // 5 + 12
    {
        IRuntimeEnvironment env = new TestRuntimeEnvironment("5");
        string output = RunInterpreter("Features/Integer/IntegerAddition.clvr", env);
        Assert.Contains("17", output);
    }

    [Fact]
    public void FeatureIntegerSubtraction() // 5 - 12
    {
        IRuntimeEnvironment env = new TestRuntimeEnvironment("5");
        string output = RunInterpreter("Features/Integer/IntegerSubtraction.clvr", env);
        Assert.Contains("-7", output);
    }

    [Fact]
    public void FeatureIntegerMultiplying() // 12 * 5
    {
        IRuntimeEnvironment env = new TestRuntimeEnvironment("5");
        string output = RunInterpreter("Features/Integer/IntegerMultiplying.clvr", env);
        Assert.Contains("60", output);
    }

    [Fact]
    public void FeatureIntegerDivision() // 12 / 5
    {
        IRuntimeEnvironment env = new TestRuntimeEnvironment("5");
        string output = RunInterpreter("Features/Integer/IntegerDivision.clvr", env);
        Assert.Contains("2", output);
    }

    [Fact]
    public void FeatureIntegerModulo() // 14 % 5
    {
        IRuntimeEnvironment env = new TestRuntimeEnvironment("5");
        string output = RunInterpreter("Features/Integer/IntegerModulo.clvr", env);
        Assert.Contains("4", output);
    }

    [Fact]
    public void FeatureIntegerOverflow() // 4 * 10^9 - 2*INT_MAX
    {
        string output = RunInterpreter("Features/Integer/IntegerOverflow.clvr");
        Assert.Contains("-294967296", output);
    }

    [Fact]
    public void FeatureIntegerDivisionByZero()
    {
        IRuntimeEnvironment env = new TestRuntimeEnvironment("90");
        Exception ex = Assert.Throws<Exception>(() => RunInterpreter("Features/Integer/IntegerDivisionByZero.clvr", env));
        Assert.Contains("Division by zero.", ex.Message);
    }

    [Fact]
    public void FeatureInputString()
    {
        IRuntimeEnvironment env = new TestRuntimeEnvironment("мы вывели");
        string output = RunInterpreter("Features/String/InputString.clvr", env);
        Assert.Contains("Вы ввели: мы вывели", output);
    }

    [Fact]
    public void FeatureStringConcatenation()
    {
        string output = RunInterpreter("Features/String/StringConcatenation.clvr");
        Assert.Contains("а у меня камыш!", output);
    }

    [Fact]
    public void FeatureStringLength()
    {
        IRuntimeEnvironment env = new TestRuntimeEnvironment("ТЯП");
        string output = RunInterpreter("Features/String/StringLength.clvr", env);
        Assert.Contains("3", output);
    }

    [Fact]
    public void FeatureStoreAndOutputStringLength()
    {
        IRuntimeEnvironment env = new TestRuntimeEnvironment("итерация 3");
        string output = RunInterpreter("Features/String/StoreAndOutputStringLen.clvr", env);
        Assert.Contains("10", output);
    }

    [Fact]
    public void FeatureStringDecompositeByIndex()
    {
        IRuntimeEnvironment env = new TestRuntimeEnvironment("hello mom!");
        string output = RunInterpreter("Features/String/StringDecompositeByIndex.clvr", env);
        Assert.Contains("!", output);
    }

    [Fact]
    public void FeatureInputAFewDifferentVars()
    {
        IRuntimeEnvironment env = new TestRuntimeEnvironment("443", "3.14", "youtube.com");
        string output = RunInterpreter("Features/InputAFewDifferentVariables.clvr", env);
        Assert.Contains("PORT: 443, VER: 3.14, DOMAIN: youtube.com", output);
    }

    [Fact]
    public void FeatureTryToReadStringOutOfBounds()
    {
        Exception ex = Assert.Throws<Exception>(() => RunInterpreter("Features/String/TryToReadStringOutOfBounds.clvr"));
        Assert.Contains("String index is out of bounds", ex.Message); // VMExecutor
    }

    [Fact]
    public void FeatureTryTakeFloatIndex()
    {
        const string invalidIndex = "2.4";
        IRuntimeEnvironment env = new TestRuntimeEnvironment(invalidIndex);
        Exception ex = Assert.Throws<Exception>(() => RunInterpreter("Features/String/TryToTakeFloatIndex.clvr", env));
        Assert.Contains("String index must be int", ex.Message); // VMExecutor
    }

    [Fact]
    public void FeatureTryingToMissEntryPoint()
    {
        Exception ex = Assert.Throws<Exception>(() => RunInterpreter("Features/Negative/TryingToMissEntryPoint.clvr"));
        Assert.Contains("Expected main function", ex.Message); // VMExecutor
    }

    [Fact]
    public void FeatureTryingReadToNonExistingVar()
    {
        Exception ex = Assert.Throws<Exception>(() => RunInterpreter("Features/Negative/TryingReadToNonExistingVar.clvr"));
        Assert.Contains("Variable", ex.Message);
        Assert.Contains("is not declared", ex.Message); // SemanticAnalyzer
    }

    [Fact]
    public void FeatureTryingToDupVar()
    {
        Exception ex = Assert.Throws<Exception>(() => RunInterpreter("Features/Negative/TryingToDuplicateVar.clvr"));
        Assert.Contains("Variable", ex.Message);
        Assert.Contains("is already declared in this scope", ex.Message); // SemanticAnalyzer
    }

    [Fact]
    public void FeatureTryingToTakeInvalidIdent()
    {
        Exception ex = Assert.Throws<Exception>(() => RunInterpreter("Features/Negative/TryingToTakeInvalidIdent.clvr"));
        Assert.Contains("Invalid number literal", ex.Message); // Lexer
    }

    [Fact]
    public void FeatureStringEscapeSeqNewLine()
    {
        string output = RunInterpreter("Features/String/StringEscapeSeqNewLine.clvr");
        Assert.Contains('\n', output);
    }

    [Fact]
    public void FeatureStringEscapeSeqTabulate()
    {
        string output = RunInterpreter("Features/String/StringEscapeSeqTabulate.clvr");
        Assert.Contains('\t', output);
    }

    [Fact]
    public void FeatureStringEscapeSeqBackSlash()
    {
        string output = RunInterpreter("Features/String/StringEscapeSeqBackSlash.clvr");
        Assert.Contains('\\', output);
    }

    [Fact]
    public void FeatureStringEscapeSeqDQuote()
    {
        string output = RunInterpreter("Features/String/StringEscapeSeqDQuote.clvr");
        Assert.Contains('\"', output);
    }

    [Fact]
    public void FeatureStringOfEmojiLength()
    {
        string output = RunInterpreter("Features/String/StringOfEmojiLength.clvr");
        Assert.Contains("3", output);
        Assert.DoesNotContain("6", output);
    }

    [Fact]
    public void FeatureIndexOfEmojiInString()
    {
        string output = RunInterpreter("Features/String/IndexOfEmojiInString.clvr");
        Assert.Contains("🗿", output);
    }

    [Fact]
    public void FeatureSimpleIfElseTest()
    {
        IRuntimeEnvironment env1 = new TestRuntimeEnvironment("да");
        string output1 = RunInterpreter("Features/Bool/SimpleIfElseTest.clvr", env1);
        Assert.Contains("молодец!", output1);

        IRuntimeEnvironment env2 = new TestRuntimeEnvironment("нет");
        string output2 = RunInterpreter("Features/Bool/SimpleIfElseTest.clvr", env2);
        Assert.Contains("плохо, отчисляйся!", output2);

        IRuntimeEnvironment env3 = new TestRuntimeEnvironment("вбрбирвиьв");
        string output3 = RunInterpreter("Features/Bool/SimpleIfElseTest.clvr", env3);
        Assert.Contains("непонятно что ввёл", output3);
    }

    [Fact]
    public void FeatureOutputBoolLiteral()
    {
        IRuntimeEnvironment env1 = new TestRuntimeEnvironment("14");
        string output1 = RunInterpreter("Features/Bool/OutputBoolLiteral.clvr", env1);
        Assert.Contains("false", output1);

        IRuntimeEnvironment env2 = new TestRuntimeEnvironment("22");
        string output2 = RunInterpreter("Features/Bool/OutputBoolLiteral.clvr", env2);
        Assert.Contains("true", output2);
    }

    [Fact]
    public void FeatureLogicAND()
    {
        IRuntimeEnvironment env1 = new TestRuntimeEnvironment("n", "n");
        string output1 = RunInterpreter("Features/Bool/TestCaseLogicAND.clvr", env1);
        Assert.Contains("спокойно можем отправлять ему рассылки", output1);

        IRuntimeEnvironment env2 = new TestRuntimeEnvironment("n", "y");
        string output2 = RunInterpreter("Features/Bool/TestCaseLogicAND.clvr", env2);
        Assert.Contains("можем отправлять ему только офферные рассылки", output2);

        IRuntimeEnvironment env3 = new TestRuntimeEnvironment("y", "n");
        string output3 = RunInterpreter("Features/Bool/TestCaseLogicAND.clvr", env3);
        Assert.Contains("мы не можем ему ничего отправлять", output3);

        IRuntimeEnvironment env4 = new TestRuntimeEnvironment("y", "y");
        string output4 = RunInterpreter("Features/Bool/TestCaseLogicAND.clvr", env4);
        Assert.Contains("мы не можем ему ничего отправлять", output4);
    }

    [Fact]
    public void FeatureLogicOR()
    {
        IRuntimeEnvironment env1 = new TestRuntimeEnvironment("доширак");
        string output1 = RunInterpreter("Features/Bool/TestCaseLogicOR.clvr", env1);
        Assert.Contains("не жри эту отраву!!!", output1);

        IRuntimeEnvironment env2 = new TestRuntimeEnvironment("колбаса");
        string output2 = RunInterpreter("Features/Bool/TestCaseLogicOR.clvr", env2);
        Assert.Contains("не жри эту отраву!!!", output2);

        IRuntimeEnvironment env3 = new TestRuntimeEnvironment("творог");
        string output3 = RunInterpreter("Features/Bool/TestCaseLogicOR.clvr", env3);
        Assert.Contains("молодец, за здоровое питание", output3);

        IRuntimeEnvironment env4 = new TestRuntimeEnvironment("вода");
        string output4 = RunInterpreter("Features/Bool/TestCaseLogicOR.clvr", env4);
        Assert.Contains("молодец, за здоровое питание", output4);

        IRuntimeEnvironment env5 = new TestRuntimeEnvironment("резина");
        string output5 = RunInterpreter("Features/Bool/TestCaseLogicOR.clvr", env5);
        Assert.Contains("пересмотрите своё решение", output5);
    }

    [Fact]
    public void FeatureLogicNOT()
    {
        IRuntimeEnvironment env1 = new TestRuntimeEnvironment("14");
        string output1 = RunInterpreter("Features/Bool/TestCaseLogicNOT.clvr", env1);
        Assert.Contains("за руль рано", output1);

        IRuntimeEnvironment env2 = new TestRuntimeEnvironment("22");
        string output2 = RunInterpreter("Features/Bool/TestCaseLogicNOT.clvr", env2);
        Assert.Contains("за руль можно", output2);
    }

    [Fact]
    public void FeatureShortCircuitEvaluation()
    {
        IRuntimeEnvironment env1 = new TestRuntimeEnvironment("true");
        string output1 = RunInterpreter("Features/Bool/ShortCircuitEvaluation.clvr", env1);
        Assert.Contains("final destination", output1);

        IRuntimeEnvironment env2 = new TestRuntimeEnvironment("false");
        Exception ex = Assert.Throws<Exception>(() => RunInterpreter("Features/Bool/ShortCircuitEvaluation.clvr", env2));
        Assert.Contains("Division by zero.", ex.Message);
    }

    [Fact]
    public void FeatureInputBoolLiteral()
    {
        IRuntimeEnvironment env1 = new TestRuntimeEnvironment("true");
        string output1 = RunInterpreter("Features/Bool/InputBoolLiteral.clvr", env1);
        Assert.Contains("вы ввели true", output1);

        IRuntimeEnvironment env2 = new TestRuntimeEnvironment("false");
        string output2 = RunInterpreter("Features/Bool/InputBoolLiteral.clvr", env2);
        Assert.Contains("вы ввели false", output2);

        IRuntimeEnvironment env3 = new TestRuntimeEnvironment("maybe");
        Exception ex = Assert.Throws<Exception>(() => RunInterpreter("Features/Bool/InputBoolLiteral.clvr", env2));
        Assert.Contains("No more input", ex.Message);
    }

    [Fact]
    public void FeatureLogicOperatorsPriority()
    {
        string output = RunInterpreter("Features/Bool/LogicOperatorsPriority.clvr");
        Assert.Contains("fact1 = true", output);
        Assert.Contains("fact2 = false", output);
        Assert.Contains("fact3 = false", output);
        Assert.Contains("fact4 = true", output);
        Assert.Contains("fact5 = true", output);
        Assert.Contains("fact6 = false", output);
        Assert.Contains("fact7 = true", output);
    }

    [Fact]
    public void FeatureInnerIfElseConditions() // проблемы висячего else у нас нет, [] явно
    {
        IRuntimeEnvironment env1 = new TestRuntimeEnvironment("12");
        string output1 = RunInterpreter("Features/Bool/InnerIfElseConditions.clvr", env1);
        Assert.Contains("between 11 and 14 inclusive", output1);

        IRuntimeEnvironment env2 = new TestRuntimeEnvironment("10");
        string output2 = RunInterpreter("Features/Bool/InnerIfElseConditions.clvr", env2);
        Assert.Contains("less than 10 or equal", output2);

        IRuntimeEnvironment env3 = new TestRuntimeEnvironment("15");
        string output3 = RunInterpreter("Features/Bool/InnerIfElseConditions.clvr", env3);
        Assert.Contains("greater than 15 or equal", output3);
    }

    [Fact]
    public void FeatureCompareLess()
    {
        IRuntimeEnvironment env1 = new TestRuntimeEnvironment("12");
        string output1 = RunInterpreter("Features/Bool/CompareLess.clvr", env1);
        Assert.Contains("вы несовершеннолетний", output1);

        IRuntimeEnvironment env2 = new TestRuntimeEnvironment("18");
        string output2 = RunInterpreter("Features/Bool/CompareLess.clvr", env2);
        Assert.Contains("вы совершеннолетний", output2);

        IRuntimeEnvironment env3 = new TestRuntimeEnvironment("19");
        string output3 = RunInterpreter("Features/Bool/CompareLess.clvr", env3);
        Assert.Contains("вы совершеннолетний", output3);
    }

    [Fact]
    public void FeatureCompareLessEqual()
    {
        IRuntimeEnvironment env1 = new TestRuntimeEnvironment("17");
        string output1 = RunInterpreter("Features/Bool/CompareLessEqual.clvr", env1);
        Assert.Contains("не призывной возраст", output1);

        IRuntimeEnvironment env2 = new TestRuntimeEnvironment("18");
        string output2 = RunInterpreter("Features/Bool/CompareLessEqual.clvr", env2);
        Assert.Contains("потенциальный призывник", output2);

        IRuntimeEnvironment envMid = new TestRuntimeEnvironment("24");
        string outputMid = RunInterpreter("Features/Bool/CompareLessEqual.clvr", envMid);
        Assert.Contains("потенциальный призывник", outputMid);

        IRuntimeEnvironment env3 = new TestRuntimeEnvironment("30");
        string output3 = RunInterpreter("Features/Bool/CompareLessEqual.clvr", env3);
        Assert.Contains("потенциальный призывник", output3);

        IRuntimeEnvironment env4 = new TestRuntimeEnvironment("31");
        string output4 = RunInterpreter("Features/Bool/CompareLessEqual.clvr", env4);
        Assert.Contains("не призывной возраст", output4);
    }

    [Fact]
    public void FeatureCompareGreater()
    {
        IRuntimeEnvironment env1 = new TestRuntimeEnvironment("Александр");
        string output1 = RunInterpreter("Features/Bool/CompareGreater.clvr", env1);
        Assert.Contains("ваше имя длиннее, чем \"Василий\"", output1);

        IRuntimeEnvironment env2 = new TestRuntimeEnvironment("Виталий");
        string output2 = RunInterpreter("Features/Bool/CompareGreater.clvr", env2);
        Assert.Contains("ваше имя не длиннее, чем \"Василий\"", output2);

        IRuntimeEnvironment env3 = new TestRuntimeEnvironment("Илья");
        string output3 = RunInterpreter("Features/Bool/CompareGreater.clvr", env3);
        Assert.Contains("ваше имя не длиннее, чем \"Василий\"", output3);
    }

    [Fact]
    public void FeatureCompareGreaterEqual()
    {
        IRuntimeEnvironment env1 = new TestRuntimeEnvironment("18");
        string output1 = RunInterpreter("Features/Bool/CompareGreaterEqual.clvr", env1);
        Assert.Contains("вы совершеннолетний", output1);

        IRuntimeEnvironment env2 = new TestRuntimeEnvironment("19");
        string output2 = RunInterpreter("Features/Bool/CompareGreaterEqual.clvr", env2);
        Assert.Contains("вы совершеннолетний", output2);

        IRuntimeEnvironment env3 = new TestRuntimeEnvironment("17");
        string output3 = RunInterpreter("Features/Bool/CompareGreaterEqual.clvr", env3);
        Assert.Contains("вы несовершеннолетний", output3);
    }

    [Fact]
    public void FeatureCompareEqual()
    {
        IRuntimeEnvironment env1 = new TestRuntimeEnvironment("1900");
        string output1 = RunInterpreter("Features/Bool/CompareEqual.clvr", env1);
        Assert.Contains("1900 - невисокосный год", output1);

        IRuntimeEnvironment env2 = new TestRuntimeEnvironment("2000");
        string output2 = RunInterpreter("Features/Bool/CompareEqual.clvr", env2);
        Assert.Contains("2000 - високосный год", output2);

        IRuntimeEnvironment env3 = new TestRuntimeEnvironment("2012");
        string output3 = RunInterpreter("Features/Bool/CompareEqual.clvr", env3);
        Assert.Contains("2012 - високосный год", output3);

        IRuntimeEnvironment env4 = new TestRuntimeEnvironment("2026");
        string output4 = RunInterpreter("Features/Bool/CompareEqual.clvr", env4);
        Assert.Contains("2026 - невисокосный год", output4);
    }

    [Fact]
    public void FeatureCompareNotEqual()
    {
        IRuntimeEnvironment env1 = new TestRuntimeEnvironment("27188218");
        string output1 = RunInterpreter("Features/Bool/CompareNotEqual.clvr", env1);
        Assert.Contains("верный пароль", output1);

        IRuntimeEnvironment env2 = new TestRuntimeEnvironment("22866611");
        string output2 = RunInterpreter("Features/Bool/CompareNotEqual.clvr", env2);
        Assert.Contains("вы не угадали пароль", output2);
    }

    /// <summary>
    /// приёмочные тесты для полноценных программ
    /// </summary>
    [Fact]
    public void ProgramHelloWorld()
    {
        string output = RunInterpreter("Programs/HelloWorld.clvr");
        Assert.Equal("Hello, World!", output);
    }

    [Fact]
    public void ProgramComplexOutput()
    {
        string output = RunInterpreter("Programs/ComplexOutput.clvr");
        const string expected = "3.14 / 42 / Lorem ipsum dolor sit amet, consectetur adipiscing elit";
        Assert.Equal(expected, output);
    }

    [Fact]
    public void ProgramSequentialOutput()
    {
        string output = RunInterpreter("Programs/SequentialOutput.clvr");
        const string expected = "Hello, World!  /  Hello, World!";
        Assert.Equal(expected, output);
    }

    [Fact]
    public void ExecuteCircleSquare()
    {
        IRuntimeEnvironment env = new TestRuntimeEnvironment("5");
        string output = RunInterpreter("ProgramExamples/CircleSquare.clvr", env);
        Assert.Contains("Circle square: 78.53", output);
    }

    [Fact]
    public void ExecuteRectangleSquare()
    {
        IRuntimeEnvironment env = new TestRuntimeEnvironment("10", "20");
        string output = RunInterpreter("ProgramExamples/RectangleSquare.clvr", env);
        Assert.Contains("rectangle square: 200", output);
    }

    [Fact]
    public void ExecuteSimpleTest()
    {
        string output = RunInterpreter("ProgramExamples/SimpleTest.clvr");
        Assert.Contains("2 + 2 * 2 = 6", output);
    }

    private string RunInterpreter(string relativeFilePath)
    {
        return RunInterpreter(relativeFilePath, new ConsoleRuntimeEnvironment());
    }

    private string RunInterpreter(string relativeFilePath, IRuntimeEnvironment env)
    {
        string fullPath = Path.Combine(_testDataPath, relativeFilePath);
        if (!File.Exists(fullPath))
        {
            throw new Exception($"test file not found: {fullPath}");
        }

        string sourceCode = File.ReadAllText(fullPath);

        StringWriter stringWriter = new StringWriter();
        TextWriter originalOut = Console.Out;
        TextWriter originalError = Console.Error;

        try
        {
            Console.SetOut(stringWriter);
            Console.SetError(stringWriter);

            InterpreterEngine interpreter = new InterpreterEngine();
            interpreter.Execute(sourceCode, env);

            return stringWriter.ToString().TrimEnd();
        }
        catch (Exception ex)
        {
            throw new Exception($"{stringWriter.ToString()}{ex.Message}", ex);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
            stringWriter.Dispose();
        }
    }
}