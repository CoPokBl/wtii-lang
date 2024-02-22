namespace WhatTimeIsIt.ParsedScripts.Statements;

public abstract class Statement {
    public StatementType Type;
}

public enum StatementType {
    VariableSet,
    VariableInit,
    MethodCall,
    MethodDefinition,
    If,
    Return,
    While,
    EnterScope,
    ExitScope,
    Throw,
    TryCatch,
    LoadLib
}