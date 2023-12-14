using WhatTimeIsIt.ParsedScripts.Values;

namespace WhatTimeIsIt.Builtins.Functions;

public static class Convert {
    
    public static Value ToFloat(Value[] args) {
        RealReference reference = Interpreter.ResolveValue(args[0]);
        if (reference is not Constant constant) {
            throw Interpreter.Error("to_float() argument must be a constant.");
        }
        return new Constant(constant.Value, "float");
    }
    
    public static Value ToInt(Value[] args) {
        RealReference reference = Interpreter.ResolveValue(args[0]);
        if (reference is not Constant constant) {
            throw Interpreter.Error("to_int() argument must be a constant.");
        }
        string intStr = constant.Value;
        if (!int.TryParse(intStr, out int val)) {
            throw Interpreter.Error("to_int() argument must be an integer.");
        }
        return new Constant(val.ToString(), "int");
    }
    
}