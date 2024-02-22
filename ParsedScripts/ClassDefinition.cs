using WhatTimeIsIt.ParsedScripts.Statements;

namespace WhatTimeIsIt.ParsedScripts;

public class ClassDefinition {
    public string Name = null!;
    public MethodDefinition[] Methods = Array.Empty<MethodDefinition>();
    public VariableInit[] Variables = Array.Empty<VariableInit>();
}