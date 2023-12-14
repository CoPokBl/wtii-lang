using WhatTimeIsIt.ParsedScripts.Values;

namespace WhatTimeIsIt.Builtins.Functions;

public static class IO {
    
    public static Value Print(Value[] args) {
        if (args.Length != 1) {
            throw Interpreter.Error("print() takes exactly 1 argument");
        }
        RealReference reference = Interpreter.ResolveValue(args[0]);
        if (reference is not Constant constant) {
            throw Interpreter.Error("print() argument must be a constant.");
        }
        Console.Write(constant.Value);
        return Constant.Null;
    }
    
    public static Value PrintLine(Value[] args) {
        if (args.Length != 1) {
            throw Interpreter.Error("println() takes exactly 1 argument");
        }
        RealReference reference = Interpreter.ResolveValue(args[0]);
        if (reference is not Constant constant) {
            throw Interpreter.Error("println() argument must be a constant.");
        }
        Console.WriteLine(constant.Value);
        return Constant.Null;
    }
    
    public static Value ReadKey(Value[] args) {
        if (args.Length != 0) {
            throw Interpreter.Error("read_key() takes exactly 0 arguments");
        }
        return new Constant(Console.ReadKey().KeyChar.ToString(), "string");
    }
    
    public static Value ReadLine(Value[] args) {
        if (args.Length != 0) {
            throw Interpreter.Error("read_line() takes exactly 0 arguments");
        }
        return new Constant(Console.ReadLine()!, "string");
    }
    
}