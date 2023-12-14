using WhatTimeIsIt.ParsedScripts.Values;

namespace WhatTimeIsIt.ParsedScripts.Statements;

public class ThrowStatement : Statement {
    public new StatementType Type = StatementType.Throw;
    public readonly Value Exception;
    
    public ThrowStatement(Value exception) {
        Exception = exception;
    }
}