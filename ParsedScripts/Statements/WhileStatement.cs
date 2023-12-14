using WhatTimeIsIt.ParsedScripts.Values;

namespace WhatTimeIsIt.ParsedScripts.Statements;

public class WhileStatement : Statement {
    public new StatementType Type = StatementType.While;
    public Value Condition = null!;
    public Statement[] Statements = null!;
}