using WhatTimeIsIt.ParsedScripts.Values;

namespace WhatTimeIsIt.ParsedScripts;

public class ScriptException : Exception {
    public Value ExceptionObject;
    
    public ScriptException(Value exceptionObject) {
        ExceptionObject = exceptionObject;
    }
}