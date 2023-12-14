using WhatTimeIsIt.ParsedScripts.Statements;

namespace WhatTimeIsIt.ParsedScripts; 

public class ParsedScript {
    public Statement[] Statements = null!;
    public ClassDefinition[] Classes = null!;
    
    public int Run() {
        return Interpreter.Execute(this);
    }
}