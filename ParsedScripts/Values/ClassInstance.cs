using WhatTimeIsIt.ParsedScripts.Statements;

namespace WhatTimeIsIt.ParsedScripts.Values;

public class ClassInstance : RealReference {
    public Dictionary<string, Value> Properties = new();
    public readonly Dictionary<string, MethodDefinition> Methods = new();
    
    public ClassInstance(ClassDefinition def) {
        ObjectType = def.Name;
        foreach (VariableInit variable in def.Variables) {
            Properties.Add(variable.Name, variable.ToValue());
        }
        foreach (MethodDefinition method in def.Methods) {
            Methods.Add(method.Name, method);
        }
    }

    public override int GetHashCode() {
        return Properties.GetHashCode();
    }
}