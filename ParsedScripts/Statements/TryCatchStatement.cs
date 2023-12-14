namespace WhatTimeIsIt.ParsedScripts.Statements;

public class TryCatchStatement : Statement {
    public new StatementType Type = StatementType.TryCatch;
    public Statement[] Statements = null!;
    public Statement[] CatchStatements = null!;
    public string ExceptionName = null!;
}