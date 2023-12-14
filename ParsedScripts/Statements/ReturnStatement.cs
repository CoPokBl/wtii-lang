using WhatTimeIsIt.ParsedScripts.Values;

namespace WhatTimeIsIt.ParsedScripts.Statements;

public class ReturnStatement : Statement {
    public new StatementType Type = StatementType.Return;
    public Value Value = null!;
}