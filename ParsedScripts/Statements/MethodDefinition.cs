using WhatTimeIsIt.ParsedScripts.Values;

namespace WhatTimeIsIt.ParsedScripts.Statements;

public class MethodDefinition : Statement {
    public new StatementType Type = StatementType.MethodDefinition;
    public string Name = null!;
    public string ReturnType = null!;
    public string[] Arguments = null!;
    public string[] ArgumentTypes = null!;
    public Statement[] Statements = null!;
    public Func<Value[], Value>? CsFunc = null;

    public MethodDefinition() { }
    
    public MethodDefinition(string name, string returnType, params (string, string)[] args) {
        Name = name;
        ReturnType = returnType;
        Arguments = new string[args.Length];
        ArgumentTypes = new string[args.Length];
        for (int i = 0; i < args.Length; i++) {
            Arguments[i] = args[i].Item1;
            ArgumentTypes[i] = args[i].Item2;
        }
    }
}