using WhatTimeIsIt.ParsedScripts.Values;

namespace WhatTimeIsIt.ParsedScripts.Statements;

public class IfStatement : Statement {
    public new StatementType Type = StatementType.If;
    public Value Condition = null!;
    public Statement[] Statements = null!;
    public Statement[] ElseStatements = null!;
}