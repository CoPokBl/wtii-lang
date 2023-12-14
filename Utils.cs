using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using WhatTimeIsIt.ParsedScripts.Statements;

namespace WhatTimeIsIt; 

public static class Utils {
    
    public static Dictionary<string, string> ParseSwitches(string[] args) {
        Dictionary<string, string> switches = new();
        for (int i = 0; i < args.Length; i++) {
            string arg = args[i];
            if (!arg.StartsWith("--")) continue;
            string key = arg[2..];
            string value = args.Length > i + 1 ? args[i + 1] : "";
            switches[key] = value;
        }
        return switches;
    }

    public static void DumpHelp() {
        _p("WhatTimeIsIt - Powerful scripting language");
        _p("Usage: WhatTimeIsIt [file] [options]");
    }

    private static void _p(string s) {
        Console.WriteLine(s);
    }

    public static bool TryGetArg(string[] args, int index, out string val) {
        if (args.Length > index) {
            val = args[index];
            return true;
        }
        val = "";
        return false;
    }
    
    public static T[] Print<T>(this T[] arr, string prefix = "") {
        foreach (T s in arr) {
            Console.WriteLine(prefix + s);
        }
        return arr;
    }
    
    public static T Print<T>(this T obj, string prefix = "", string suffix = "") {
        Console.WriteLine(prefix + obj + suffix);
        return obj;
    }
    
    // Safe split function that splits by a char while not splitting on that char if it's in a string ""
    public static string[] SafeSplit(this string s, string separator) {
        List<string> parts = new();
        string current = "";
        bool inString = false;
        bool inAnotherMethod = false;
        for (int i = 0; i < s.Length; i++) {
            char c = s[i];
            if (c == '"') {
                inString = !inString;
            }
            if (c == '(') {
                inAnotherMethod = true;
            }
            if (c == ')') {
                inAnotherMethod = false;
            }
            if (c == separator[0] && !inString && !inAnotherMethod) {
                parts.Add(current);
                current = "";
                continue;
            }
            current += c;
        }
        parts.Add(current);
        return parts.ToArray();
    }
    
    public static string[] SafeSplit(this string s, char separator) {
        return s.SafeSplit(separator.ToString());
    }
    
    public static string CapitaliseFirstLetter(this string s) {
        return s[0].ToString().ToUpper() + s[1..].ToUpper();
    }

    public static T FromJson<T>(this string json) {
        return JsonSerializer.Deserialize<T>(json)!;
    }
    
    public static Dictionary<string, MethodDefinition> ToMethodDictionary(this IEnumerable<MethodDefinition> methods) {
        Dictionary<string, MethodDefinition> dict = new();
        foreach (MethodDefinition method in methods) {
            dict[method.Name] = method;
        }
        return dict;
    }
    
    public static T[] SingleEnumerate<T>(this T obj) {
        return new[] { obj };
    }

    public static T Debug<T>(this T o, string s) {
        Interpreter.Debug(s);
        return o;
    }
    
    /// <summary>
    /// WHY CAN"T I JUST PRINT A STACK TRACE
    /// </summary>
    /// <exception cref="Exception">You asked for a stacktrace, here you go</exception>
    [DoesNotReturn]
    public static void PrintStackTrace() {
        throw new Exception("End me");
    }
    
    public static string CombineVariablePath(this string[] path) {
        return string.Join(".", path);
    }
    
}