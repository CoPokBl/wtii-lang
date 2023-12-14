using WhatTimeIsIt.Builtins.Functions;
using WhatTimeIsIt.ParsedScripts.Statements;
using WhatTimeIsIt.ParsedScripts.Values;
using Convert = WhatTimeIsIt.Builtins.Functions.Convert;

namespace WhatTimeIsIt.Builtins; 

public static class BuiltIns {
    public static readonly Dictionary<string, Func<Value[], Value>> Functions = new() {
        { "print", IO.Print },
        { "println", IO.PrintLine },
        { "exit", Standard.Exit },
        { "str", Standard.Str },
        { "string", Standard.Str },
        { "add", Standard.Add },
        { "subtract", Standard.Subtract },
        { "multiply", Standard.Multiply },
        { "divide", Standard.Divide },
        { "concat", Standard.Concat },
        { "equals", Standard.Equals },
        { "not", Standard.Not },
        { "to_int", Convert.ToInt },
        { "to_float", Convert.ToFloat },
        { "read_key", IO.ReadKey },
        { "read_line", IO.ReadLine },
        { "type_of", Standard.TypeOf },
        { "to_json", Json.ToJson }
    };
    
    public static readonly Dictionary<string, MethodDefinition> MethodDefinitions = new() {
        { "print", new MethodDefinition("print", "null", ("s", "string")) },
        { "println", new MethodDefinition("println", "null", ("s", "string")) },
        { "exit", new MethodDefinition("exit", "null", ("code", "int")) },
        { "str", new MethodDefinition("str", "string", ("val", "any")) },
        { "string", new MethodDefinition("string", "string", ("val", "any")) },
        { "add", new MethodDefinition("add", "float", ("n1", "float"), ("n2", "float")) },
        { "subtract", new MethodDefinition("subtract", "float", ("n1", "float"), ("n2", "float")) },
        { "multiply", new MethodDefinition("multiply", "float", ("n1", "float"), ("n2", "float")) },
        { "divide", new MethodDefinition("divide", "float", ("n1", "float"), ("n2", "float")) },
        { "concat", new MethodDefinition("concat", "string", ("v1", "string"), ("v2", "string")) },
        { "equals", new MethodDefinition("equals", "bool", ("v1", "any"), ("v2", "any")) },
        { "not", new MethodDefinition("not", "bool", ("condition", "bool")) },
        { "to_int", new MethodDefinition("to_int", "int", ("val", "float")) },
        { "to_float", new MethodDefinition("to_float", "float", ("val", "int")) },
        { "read_key", new MethodDefinition("read_key", "string") },
        { "read_line", new MethodDefinition("read_line", "string") },
        { "type_of", new MethodDefinition("type_of", "string", ("val", "any")) },
        { "to_json", new MethodDefinition("to_json", "string", ("val", "any")) }
    };
}