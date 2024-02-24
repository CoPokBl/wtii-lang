namespace WhatTimeIsIt.ParsedScripts.Values;

public class ArrayValue : RealReference {
    public new ValueType Type = ValueType.ArrayValue;
    public Value[] Values;
    public string BaseObjectType => ObjectType[..^2];
    
    public ArrayValue(string baseObjectType, Value[] values) {
        Values = values;
        ObjectType = baseObjectType + "[]";
    }
}