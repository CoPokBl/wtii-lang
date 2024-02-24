namespace WhatTimeIsIt.ParsedScripts.Values;

public abstract class Value {
    public ValueType Type;
    public string ObjectType = null!;
}

public enum ValueType {
    Constant,
    Variable,
    MethodCall,
    ArrayValue,
    ClassInstance,
    DirectMethod
}