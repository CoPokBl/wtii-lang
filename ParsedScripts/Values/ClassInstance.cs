using WhatTimeIsIt.ParsedScripts.Statements;

namespace WhatTimeIsIt.ParsedScripts.Values;

public class ClassInstance : RealReference {
    public readonly Dictionary<string, (string, Value)> Properties = new();  // <name, (type, value)>
    public readonly Dictionary<string, MethodDefinition> Methods = new();
    
    public ClassInstance(ClassDefinition def, Dictionary<string, (string, Value)>? props = null) {
        ObjectType = def.Name;
        foreach (VariableInit variable in def.Variables) {
            Properties.Add(variable.Name, (variable.VariableType, variable.ToValue()));
        }
        foreach (MethodDefinition method in def.Methods) {
            Methods.Add(method.Name, method);
        }

        if (props == null) return;
        
        foreach (KeyValuePair<string, (string, Value)> prop in props) {
            Properties[prop.Key] = prop.Value;
        }
    }

    public override int GetHashCode() {
        return Properties.GetHashCode();
    }
}