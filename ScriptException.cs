using WhatTimeIsIt.ParsedScripts.Values;

namespace WhatTimeIsIt;

public class ScriptException : Exception {
    public readonly Value ExceptionObject;
    
    public ScriptException(Value exceptionObject) {
        ExceptionObject = exceptionObject;
    }
}