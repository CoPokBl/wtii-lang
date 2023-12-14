using WhatTimeIsIt.ParsedScripts.Values;

namespace WhatTimeIsIt.ParsedScripts.Statements;

public class FunctionCall : Statement {
    public new StatementType Type = StatementType.MethodCall;
    public string[] Path = null!;
    public Value[] Arguments = null!;
}