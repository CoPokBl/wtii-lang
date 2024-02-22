using WhatTimeIsIt.ParsedScripts.Values;

namespace WhatTimeIsIt.Builtins.Functions;

public static class Files {
    
    public static Value ReadFileAsText(Value[] args) {
        if (args.Length != 1) {
            throw Interpreter.Error("read_file_as_text() takes exactly 1 argument");
        }
        RealReference reference = Interpreter.ResolveValue(args[0]);
        if (reference is not Constant constant) {
            throw Interpreter.Error("read_file_as_text() argument must be a constant.");
        }
        string file = constant.Value;
        if (!File.Exists(file)) {
            throw Interpreter.Error($"File not found: {file}");
        }
        return new Constant(File.ReadAllText(file), "string");
    }
    
    public static Value WriteTextToFile(Value[] args) {
        if (args.Length != 2) {
            throw Interpreter.Error("write_text_to_file() takes exactly 2 arguments");
        }
        RealReference reference = Interpreter.ResolveValue(args[0]);
        if (reference is not Constant constant) {
            throw Interpreter.Error("write_text_to_file() first argument must be a constant.");
        }
        string file = constant.Value;
        reference = Interpreter.ResolveValue(args[1]);
        if (reference is not Constant constant2) {
            throw Interpreter.Error("write_text_to_file() second argument must be a constant.");
        }
        File.WriteAllText(file, constant2.Value);
        return Constant.Null;
    }
    
    public static Value FileExists(Value[] args) {
        if (args.Length != 1) {
            throw Interpreter.Error("file_exists() takes exactly 1 argument");
        }
        RealReference reference = Interpreter.ResolveValue(args[0]);
        if (reference is not Constant constant) {
            throw Interpreter.Error("file_exists() argument must be a constant.");
        }
        return new Constant(File.Exists(constant.Value).ToString(), "bool");
    }
    
    public static Value DeleteFile(Value[] args) {
        if (args.Length != 1) {
            throw Interpreter.Error("delete_file() takes exactly 1 argument");
        }
        RealReference reference = Interpreter.ResolveValue(args[0]);
        if (reference is not Constant constant) {
            throw Interpreter.Error("delete_file() argument must be a constant.");
        }
        File.Delete(constant.Value);
        return Constant.Null;
    }
    
    public static Value GetFiles(Value[] args) {
        if (args.Length != 1) {
            throw Interpreter.Error("get_files() takes exactly 1 argument");
        }
        RealReference reference = Interpreter.ResolveValue(args[0]);
        if (reference is not Constant constant) {
            throw Interpreter.Error("get_files() argument must be a constant.");
        }
        List<Value> files = new();
        foreach (string file in Directory.GetFiles(constant.Value)) {
            files.Add(new Constant(file, "string"));
        }
        return new ArrayValue("string", files.ToArray());
    }
    
    public static Value GetDirectories(Value[] args) {
        if (args.Length != 1) {
            throw Interpreter.Error("get_directories() takes exactly 1 argument");
        }
        RealReference reference = Interpreter.ResolveValue(args[0]);
        if (reference is not Constant constant) {
            throw Interpreter.Error("get_directories() argument must be a constant.");
        }
        List<Value> dirs = new();
        foreach (string dir in Directory.GetDirectories(constant.Value)) {
            dirs.Add(new Constant(dir, "string"));
        }
        return new ArrayValue("string", dirs.ToArray());
    }
    
    public static Value CreateDirectory(Value[] args) {
        if (args.Length != 1) {
            throw Interpreter.Error("create_directory() takes exactly 1 argument");
        }
        RealReference reference = Interpreter.ResolveValue(args[0]);
        if (reference is not Constant constant) {
            throw Interpreter.Error("create_directory() argument must be a constant.");
        }
        Directory.CreateDirectory(constant.Value);
        return Constant.Null;
    }
    
    public static Value DeleteDirectory(Value[] args) {
        if (args.Length != 1) {
            throw Interpreter.Error("delete_directory() takes exactly 1 argument");
        }
        RealReference reference = Interpreter.ResolveValue(args[0]);
        if (reference is not Constant constant) {
            throw Interpreter.Error("delete_directory() argument must be a constant.");
        }
        Directory.Delete(constant.Value);
        return Constant.Null;
    }
    
    public static Value DirectoryExists(Value[] args) {
        if (args.Length != 1) {
            throw Interpreter.Error("directory_exists() takes exactly 1 argument");
        }
        RealReference reference = Interpreter.ResolveValue(args[0]);
        if (reference is not Constant constant) {
            throw Interpreter.Error("directory_exists() argument must be a constant.");
        }
        return new Constant(Directory.Exists(constant.Value).ToString(), "bool");
    }
    
    public static Value GetCurrentDirectory(Value[] args) {
        if (args.Length != 0) {
            throw Interpreter.Error("get_current_directory() takes exactly 0 arguments");
        }
        return new Constant(Directory.GetCurrentDirectory(), "string");
    }
    
    public static Value SetCurrentDirectory(Value[] args) {
        if (args.Length != 1) {
            throw Interpreter.Error("set_current_directory() takes exactly 1 argument");
        }
        RealReference reference = Interpreter.ResolveValue(args[0]);
        if (reference is not Constant constant) {
            throw Interpreter.Error("set_current_directory() argument must be a constant.");
        }
        Directory.SetCurrentDirectory(constant.Value);
        return Constant.Null;
    }
    
    public static Value GetFullPath(Value[] args) {
        if (args.Length != 1) {
            throw Interpreter.Error("get_full_path() takes exactly 1 argument");
        }
        RealReference reference = Interpreter.ResolveValue(args[0]);
        if (reference is not Constant constant) {
            throw Interpreter.Error("get_full_path() argument must be a constant.");
        }
        return new Constant(Path.GetFullPath(constant.Value), "string");
    }
    
}