using WhatTimeIsIt.Builtins.Functions;
using WhatTimeIsIt.Builtins.Libraries;
using WhatTimeIsIt.ParsedScripts.Statements;
using WhatTimeIsIt.ParsedScripts.Values;
using Convert = WhatTimeIsIt.Builtins.Functions.Convert;

namespace WhatTimeIsIt.Builtins; 

public static class BuiltIns {
    public static readonly Dictionary<string, Type> Libraries = new() {
        { "http", typeof(Http) }
    };
    
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
        { "to_json", Json.ToJson },
        { "from_json", Json.FromJson },
        { "get_array_object", Standard.GetArrayObject },
        { "assert", Standard.Assert },
        { "split", Standard.Split },
        { "read_file_as_text", Files.ReadFileAsText },
        { "write_text_to_file", Files.WriteTextToFile },
        { "file_exists", Files.FileExists },
        { "delete_file", Files.DeleteFile },
        { "get_files", Files.GetFiles },
        { "get_directories", Files.GetDirectories },
        { "create_directory", Files.CreateDirectory },
        { "delete_directory", Files.DeleteDirectory },
        { "directory_exists", Files.DirectoryExists },
        { "get_current_directory", Files.GetCurrentDirectory },
        { "set_current_directory", Files.SetCurrentDirectory },
        { "get_full_path", Files.GetFullPath },
        { "load_lib", Standard.LoadLib },
        { "get_var", Standard.GetVar }
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
        { "to_json", new MethodDefinition("to_json", "string", ("val", "any")) },
        { "from_json", new MethodDefinition("from_json", "any", ("json", "string"), ("type", "class")) },
        { "get_array_object", new MethodDefinition("get_array_object", "any", ("arr", "any[]"), ("index", "int")) },
        { "assert", new MethodDefinition("assert", "null", ("condition", "bool"), ("message", "string")) },
        { "split", new MethodDefinition("split", "string[]", ("s", "string"), ("separator", "string")) },
        { "read_file_as_text", new MethodDefinition("read_file_as_text", "string", ("path", "string")) },
        { "write_text_to_file", new MethodDefinition("write_text_to_file", "null", ("path", "string"), ("text", "string")) },
        { "file_exists", new MethodDefinition("file_exists", "bool", ("path", "string")) },
        { "delete_file", new MethodDefinition("delete_file", "null", ("path", "string")) },
        { "get_files", new MethodDefinition("get_files", "string[]", ("path", "string")) },
        { "get_directories", new MethodDefinition("get_directories", "string[]", ("path", "string")) },
        { "create_directory", new MethodDefinition("create_directory", "null", ("path", "string")) },
        { "delete_directory", new MethodDefinition("delete_directory", "null", ("path", "string")) },
        { "directory_exists", new MethodDefinition("directory_exists", "bool", ("path", "string")) },
        { "get_current_directory", new MethodDefinition("get_current_directory", "string") },
        { "set_current_directory", new MethodDefinition("set_current_directory", "null", ("path", "string")) },
        { "get_full_path", new MethodDefinition("get_full_path", "string", ("path", "string")) },
        { "load_lib", new MethodDefinition("load_lib", "null", ("lib", "string")) },
        { "get_var", new MethodDefinition("get_var", "any", ("name", "string")) }
    };
}