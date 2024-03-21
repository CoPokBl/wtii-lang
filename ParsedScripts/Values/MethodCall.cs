using WhatTimeIsIt.ParsedScripts.Statements;

namespace WhatTimeIsIt.ParsedScripts.Values;

public class MethodCall : Value {
    public new ValueType Type = ValueType.MethodCall;
    public string[] Path = null!;
    public Value[] Arguments = null!;

    public MethodCall(string[] path, Value[] args, string? objectType = null) {
        Path = path;
        Arguments = args;
        ObjectType = objectType!;
    }
    
    public FunctionCall ToFunctionCall() {
        return new FunctionCall {
            Path = Path,
            Arguments = Arguments
        };
    }
    
    public static MethodCall Not(Value value) {
        return new MethodCall("not".SingleEnumerate(), new Value[] {
            value
        }, "bool");
    }

    public override int GetHashCode() {
        return Path.GetHashCode();
    }
}