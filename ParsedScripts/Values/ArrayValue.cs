namespace WhatTimeIsIt.ParsedScripts.Values;

public class ArrayValue : RealReference {
    public Value[] Values;
    public string BaseObjectType => ObjectType[..^2];
    
    public ArrayValue(string baseObjectType, Value[] values) {
        Values = values;
        ObjectType = baseObjectType + "[]";
    }
}