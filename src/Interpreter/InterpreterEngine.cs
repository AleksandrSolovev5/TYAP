using Ast;
using Bytecode;
using Codegen;
using Lexing;
using Parsing;
using Semantics;
using VirtualMachine;

namespace Interpreter;

public class InterpreterEngine
{
    public void Execute(string code)
    {
        Lexer lexer = new(code);
        Parser parser = new(lexer);
        ProgramNode program = parser.ParseProgram();

        SemanticAnalyzer semanticAnalyzer = new();
        semanticAnalyzer.Analyze(program);

        CodeGenerator codeGenerator = new();
        List<Instruction> instructions = codeGenerator.Generate(program);

        VirtualMachineExecutor virtualMachineExecutor = new(instructions);
        virtualMachineExecutor.Run();
    }
}