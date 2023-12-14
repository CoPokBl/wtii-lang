namespace WhatTimeIsIt.ParsedScripts.Values;

public class Constant : RealReference {
    public new ValueType Type = ValueType.Constant;
    public readonly string Value;
    public static readonly Constant Null = new("null", "null");
    public static readonly Constant True = new("true", "bool");
    public static readonly Constant False = new("false", "bool");

    public Constant(string s, string type) {
        Value = s;
        ObjectType = type;
    }

    public override int GetHashCode() {
        return Value.GetHashCode();
    }
    
    public static Constant FromString(string s) {
        return new Constant(s, "string");
    }

    public override string ToString() {
        return Value;
    }
}