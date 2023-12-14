using WhatTimeIsIt.ParsedScripts.Values;

namespace WhatTimeIsIt.ParsedScripts.Statements;

public class VariableSet : Statement {
    public new StatementType Type = StatementType.VariableSet;
    public string[] Path = null!;
    public Value Value = null!;
    public string ObjectType = null!;
}