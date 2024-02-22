namespace WhatTimeIsIt.ParsedScripts.Statements;

public class LoadLibStatement : Statement {
    public new StatementType Type = StatementType.LoadLib;
    public string Lib = null!;
    
    public LoadLibStatement(string lib) {
        Lib = lib;
    }
    
    public LoadLibStatement() { }
}