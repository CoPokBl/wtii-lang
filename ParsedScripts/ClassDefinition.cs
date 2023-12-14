using WhatTimeIsIt.ParsedScripts.Statements;

namespace WhatTimeIsIt.ParsedScripts;

public class ClassDefinition {
    public string Name = null!;
    public MethodDefinition[] Methods = null!;
    public VariableInit[] Variables = null!;
}