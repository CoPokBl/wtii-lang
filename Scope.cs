using WhatTimeIsIt.Builtins;
using WhatTimeIsIt.ParsedScripts;
using WhatTimeIsIt.ParsedScripts.Statements;
using WhatTimeIsIt.ParsedScripts.Values;

namespace WhatTimeIsIt; 

public class Scope {
    public readonly Dictionary<string, (string, Value)> Variables = new();  // <name, (type, value)>
    public readonly Dictionary<string, MethodDefinition> Functions = new();
    public readonly Dictionary<string, ClassDefinition> Classes = new();
    
    /// <summary>
    /// Whether or not to check for undefined variables/methods/classes.
    /// If this is false, the parsed will not check for anything.
    /// Should only be enabled when script loads libraries that cannot
    /// be parsed by the parser.
    /// </summary>
    internal bool ChecksEnabled = true;

    public Scope() { }
    
    public Scope(Scope parent) {
        Variables = new Dictionary<string, (string, Value)>(parent.Variables);
        Functions = new Dictionary<string, MethodDefinition>(parent.Functions);
        Classes = new Dictionary<string, ClassDefinition>(parent.Classes);
        ChecksEnabled = parent.ChecksEnabled;
    }

    public Scope(bool loadBuiltins) {
        if (!loadBuiltins) return;
        foreach (KeyValuePair<string, MethodDefinition> def in BuiltIns.MethodDefinitions) {
            Functions.Add(def.Key, def.Value);
        }
    }
    
    public void AppendScope(Scope scope) {
        foreach (KeyValuePair<string, (string, Value)> variable in scope.Variables) {
            Variables[variable.Key] = variable.Value;
        }
        foreach (KeyValuePair<string, MethodDefinition> function in scope.Functions) {
            Functions[function.Key] = function.Value;
        }
        foreach (KeyValuePair<string, ClassDefinition> classDef in scope.Classes) {
            Classes[classDef.Key] = classDef.Value;
        }
    }
    
    public void SetVariable(string name, Value value) {
        if (Variables.ContainsKey(name)) Variables[name] = (Variables[name].Item1, value);
        else Variables.Add(name, (value.ObjectType, value));
    }

}