namespace WhatTimeIsIt.ParsedScripts.Values;

public class Variable : Value {
    public new ValueType Type = ValueType.Variable;
    public readonly string[] Path;
    //public string Name => Path[^1];

    public override int GetHashCode() {
        return Path.GetHashCode();
    }

    public Variable(string[] path) {
        Path = path;
    }
    
    public Variable(string[] path, string ot) {
        Path = path;
        ObjectType = ot;
    }
}