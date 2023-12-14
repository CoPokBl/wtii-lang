using System.Globalization;
using WhatTimeIsIt.ParsedScripts.Values;

namespace WhatTimeIsIt.Builtins.Functions; 

public static class Standard {

    public static Value Exit(Value[] args) {
        int exitCode;
        if (args.Length == 0 || args[0] == Constant.Null) {
            exitCode = 0;
        }
        else {
            RealReference rr = Interpreter.ResolveValue(args[0]);
            if (rr.ObjectType != "int") {
                throw Interpreter.Error("exit() argument must be an integer.");
            }
            if (rr is not Constant c) {
                throw Interpreter.Error("exit() argument must be a constant.");
            }
            exitCode = int.Parse(c.Value);
        }
        throw new ProgramExitException(exitCode);
    }
    
    public static Value Str(Value[] args) {
        RealReference reference = Interpreter.ResolveValue(args[0]);
        if (reference is not Constant constant) {
            throw Interpreter.Error("str() argument must be a constant.");
        }
        return new Constant(constant.Value, "string");
    }
    
    public static Value Add(Value[] args) {
        RealReference ar = Interpreter.ResolveValue(args[0]);
        RealReference br = Interpreter.ResolveValue(args[1]);
        if (ar is not Constant a || br is not Constant b) {
            throw Interpreter.Error("add() arguments must be constants");
        }
        return new Constant((float.Parse(a.Value) + float.Parse(b.Value)).ToString(CultureInfo.InvariantCulture), "float");
    }
    
    public static Value Subtract(Value[] args) {
        RealReference ar = Interpreter.ResolveValue(args[0]);
        RealReference br = Interpreter.ResolveValue(args[1]);
        if (ar is not Constant a || br is not Constant b) {
            throw Interpreter.Error("subtract() arguments must be constants");
        }
        return new Constant((float.Parse(a.Value) - float.Parse(b.Value)).ToString(CultureInfo.InvariantCulture), "float");
    }
    
    public static Value Multiply(Value[] args) {
        RealReference ar = Interpreter.ResolveValue(args[0]);
        RealReference br = Interpreter.ResolveValue(args[1]);
        if (ar is not Constant a || br is not Constant b) {
            throw Interpreter.Error("multiply() arguments must be constants");
        }
        return new Constant((float.Parse(a.Value) * float.Parse(b.Value)).ToString(CultureInfo.InvariantCulture), "float");
    }
    
    public static Value Divide(Value[] args) {
        RealReference ar = Interpreter.ResolveValue(args[0]);
        RealReference br = Interpreter.ResolveValue(args[1]);
        if (ar is not Constant a || br is not Constant b) {
            throw Interpreter.Error("divide() arguments must be constants");
        }
        return new Constant((float.Parse(a.Value) / float.Parse(b.Value)).ToString(), "float");
    }
    
    public static Value Concat(Value[] args) {
        RealReference ar = Interpreter.ResolveValue(args[0]);
        RealReference br = Interpreter.ResolveValue(args[1]);
        if (ar is not Constant a || br is not Constant b) {
            throw Interpreter.Error("concat() arguments must be constants");
        }
        return new Constant(a.Value + b.Value, "string");
    }
    
    public static Value Equals(Value[] args) {
        RealReference a = Interpreter.ResolveValue(args[0]);
        RealReference b = Interpreter.ResolveValue(args[1]);
        return a.GetHashCode() == b.GetHashCode() ? Constant.True : Constant.False;
    }
    
    public static Value Not(Value[] args) {
        RealReference ar = Interpreter.ResolveValue(args[0]);
        if (ar.ObjectType != "bool") {
            throw Interpreter.Error("not() argument must be a constant");
        }
        return ((Constant) ar).Value == "true" ? Constant.False : Constant.True;
    }
    
    public static Value TypeOf(Value[] args) {
        return new Constant(args[0].ObjectType, "string");
    }
    
}