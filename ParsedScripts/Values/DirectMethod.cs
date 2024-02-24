using WhatTimeIsIt.ParsedScripts.Statements;

namespace WhatTimeIsIt.ParsedScripts.Values;

public class DirectMethod : Value {
    public new ValueType Type = ValueType.DirectMethod;
    public readonly MethodDefinition Method;
    public readonly Value[] Arguments;
    
    public DirectMethod(MethodDefinition method, Value[] args) {
        Method = method;
        Arguments = args;
    }
    
    public override int GetHashCode() {
        return Method.GetHashCode();
    }
}