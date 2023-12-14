using WhatTimeIsIt.ParsedScripts.Values;

namespace WhatTimeIsIt.ParsedScripts.Statements;

public class VariableInit : Statement {
    public new StatementType Type = StatementType.VariableInit;
    public string Name = null!;
    public Value? Value;
    public string VariableType = null!;
    
    public Value ToValue() {
        return Value!;
    }
}
