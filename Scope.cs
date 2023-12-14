using WhatTimeIsIt.Builtins;
using WhatTimeIsIt.ParsedScripts;
using WhatTimeIsIt.ParsedScripts.Statements;
using WhatTimeIsIt.ParsedScripts.Values;

namespace WhatTimeIsIt; 

public class Scope {
    public readonly Dictionary<string, Value> Variables = new();
    public readonly Dictionary<string, MethodDefinition> Functions = new();
    public readonly Dictionary<string, ClassDefinition> Classes = new();

    public Scope() { }
    
    public Scope(Scope parent) {
        Variables = new Dictionary<string, Value>(parent.Variables);
        Functions = new Dictionary<string, MethodDefinition>(parent.Functions);
    }

    public Scope(bool loadBuiltins) {
        if (!loadBuiltins) return;
        foreach (KeyValuePair<string, MethodDefinition> def in BuiltIns.MethodDefinitions) {
            Functions.Add(def.Key, def.Value);
        }
    }

}