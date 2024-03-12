using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json;
using WhatTimeIsIt.Builtins;
using WhatTimeIsIt.ParsedScripts;
using WhatTimeIsIt.ParsedScripts.Statements;
using WhatTimeIsIt.ParsedScripts.Values;

namespace WhatTimeIsIt; 

public static class Interpreter {
    
    /// <summary>
    /// A scope contains all variables and functions that are accessible in the current scope.
    /// </summary>
    private static Stack<Scope> _scopes = null!;
    
    /// <summary>
    /// The null constant.
    /// </summary>
    private static readonly Constant Null = new("NULL", "NULL");
    
    /// <summary>
    /// Whether or not to log debug messages.
    /// </summary>
    private const bool DebugLogging = false;
    
    /// <summary>
    /// Gets the currently loaded scope which will
    /// include all variables and functions from the current scope and all parent scopes.
    /// </summary>
    public static Scope CurrentScope => _scopes.Peek();
    
    /// <summary>
    /// Creates a new scope and pushes it to the stack.
    /// This scope will only contain the values from the previous scope.
    /// </summary>
    private static void NewScope() {
        _scopes.Push(new Scope(CurrentScope));
        if (DebugLogging) Debug($"==== NEW SCOPE ==== ({Utils.GetFileAndLine(2)})");
    }
    
    /// <summary>
    /// Removes the current scope from the stack.
    /// </summary>
    private static void EndScope() {
        Scope oldScope = _scopes.Pop();
        
        // If any of the variables exist in the higher level scope, update them
        foreach (KeyValuePair<string, (string, Value)> kvp in oldScope.Variables) {
            if (!CurrentScope.Variables.ContainsKey(kvp.Key)) continue;
            CurrentScope.Variables[kvp.Key] = kvp.Value;
        }
        if (DebugLogging) Debug($"==== END SCOPE ==== ({Utils.GetFileAndLine(2)})");
    }
    
    /// <summary>
    /// Print a debug message if debug logging is enabled.
    /// You should still check DebugLogging before calling this function
    /// if the message would require getting information from expensive functions.
    /// </summary>
    /// <param name="s">The message to print.</param>
    public static void Debug(string s) {
        if (DebugLogging) Console.WriteLine("[DEBUG] " + s);
    }
    
    /// <summary>
    /// Get a script exception with the given message.
    /// </summary>
    /// <param name="msg">The error message.</param>
    /// <returns>A script exception which can be thrown.</returns>
    public static ScriptException Error(string msg) {
        return new ScriptException(Constant.FromString(msg));
    }
    
    /// <summary>
    /// Print a system stack trace for debugging.
    /// </summary>
    private static void PrintStackTrace() {
        new StackTrace().Print();
    }
    
    /// <summary>
    /// Loads a set of classes into the current scope.
    /// </summary>
    /// <param name="classes">The classes to load.</param>
    public static void LoadClasses(params ClassDefinition[] classes) {
        foreach (ClassDefinition cl in classes) {
            CurrentScope.Classes[cl.Name] = cl;
        }
    }

    /// <summary>
    /// Dynamically loads a library from a file or from the built-in libraries.
    /// </summary>
    /// <param name="path">The library file or name of builtin library.</param>
    /// <exception cref="ScriptException">Thrown when library either does not exist or is not supported.</exception>
    public static void LoadLibrary(string path) {
        if (!File.Exists(path) && !BuiltIns.Libraries.ContainsKey(path)) {
            throw Error("Library file does not exist: " + path);
        }
        
        string fileExtension = Path.GetExtension(path);
        List<IWtiiLibrary> libraries = new();
        switch (fileExtension) {
            case "wtii": {
                ParsedScript script = Parser.Parse(File.ReadAllText(path));
                LoadClasses(script.Classes);
                MethodDefinition method = new("main", "int") {
                    Statements = script.Statements
                };
                ExecuteFunction(method);
                break;
            }

            case "dll": {
                Assembly assembly = Assembly.LoadFrom(path);
                foreach (Type type in assembly.GetTypes()) {
                    if (!typeof(IWtiiLibrary).IsAssignableFrom(type)) {
                        continue;
                    }

                    IWtiiLibrary library = (IWtiiLibrary) Activator.CreateInstance(type)!;
                    libraries.Add(library);
                }

                break;
            }

            case "": {
                if (!BuiltIns.Libraries.ContainsKey(path)) {
                    throw Error("Library file type not supported: " + fileExtension);
                }

                Type libraryType = BuiltIns.Libraries[path];
                IWtiiLibrary library = (IWtiiLibrary) Activator.CreateInstance(libraryType)!;
                libraries.Add(library);
                break;
            }

            default:
                throw Error("Library file type not supported: " + fileExtension);
        }

        foreach (IWtiiLibrary library in libraries) {
            library.Init();
            CurrentScope.AppendScope(library.Scope);
            library.Run();
        }
    }

    /// <summary>
    /// Checks if a specified function is a valid conversion function for converting from fromType to toType.
    /// </summary>
    /// <param name="name">The function name.</param>
    /// <param name="fromType">The type that is being converted from.</param>
    /// <param name="toType">The type that is being converted to.</param>
    /// <returns>Whether or not the function is a valid conversion function for the 2 types.</returns>
    private static bool IsCorrectConversionName(string name, string fromType, string toType) {
        if (name == fromType.ToLower() + "_to_" + toType.ToLower()) return true;
        if (name == "to_" + toType.ToLower()) return true;
        if (name == fromType.CapitaliseFirstLetter() + "To" + toType.CapitaliseFirstLetter()) return true;
        if (name == toType.ToLower()) return true;
        if (name == toType.CapitaliseFirstLetter()) return true;
        return false;
    }

    /// <summary>
    /// Find a valid conversion method for converting from fromType to toType from the provided list.
    /// Returns null if no valid conversion method was found.
    /// </summary>
    /// <param name="definitions">The list of functions to check.</param>
    /// <param name="fromType">The type being converted from.</param>
    /// <param name="toType">The type being converted to.</param>
    /// <returns>The found conversion function, null if none was found.</returns>
    private static MethodDefinition? SearchDefsForConversion(Dictionary<string, MethodDefinition> definitions, string fromType, string toType) {
        foreach ((string? name, MethodDefinition? value) in definitions) {
            if (!IsCorrectConversionName(name, fromType, toType)) continue;
            if (value.ReturnType != toType) continue;
            if (value.Arguments.Length != 1) continue;
            if (value.ArgumentTypes[0] != fromType && value.ArgumentTypes[0] != "any") continue;
            return value;
        }
        return null;
    }

    /// <summary>
    /// Find a valid conversion method for converting from fromType to toType from all valid sourced.
    /// Returns null if no valid conversion method was found.
    /// </summary>
    /// <param name="fromType">The type being converted from.</param>
    /// <param name="exceptedType">The type being converted to.</param>
    /// <returns>The found conversion method, null if none was found.</returns>
    private static MethodDefinition? FindConversionMethod(string fromType, string exceptedType) {
        if (CurrentScope.Classes.TryGetValue(fromType, out ClassDefinition? classDef)) {
            MethodDefinition? classMethod = SearchDefsForConversion(classDef.Methods.ToMethodDictionary(), fromType, exceptedType);
            if (classMethod != null) return classMethod;
        }
        MethodDefinition? userMethod = SearchDefsForConversion(CurrentScope.Functions, fromType, exceptedType);
        if (userMethod != null) return userMethod;
        MethodDefinition? builtinMethod = SearchDefsForConversion(BuiltIns.MethodDefinitions, fromType, exceptedType);
        if (builtinMethod != null) {
            builtinMethod.CsFunc = BuiltIns.Functions[builtinMethod.Name];
        }
        return builtinMethod;
    }
    
    /// <summary>
    /// Resolves a value to its real value.
    /// </summary>
    /// <param name="value">The value to resolve.</param>
    /// <param name="exceptedType">The type to try and convert to, null if type doesn't matter.</param>
    /// <returns>The real reference represented by the value.</returns>
    public static RealReference ResolveValue(Value value, string? exceptedType = null) {
        switch (value) {
            case RealReference constant:
                if (constant.ObjectType != exceptedType) {  // Try to convert implicitly
                    MethodDefinition? method = exceptedType == null ? null : FindConversionMethod(constant.ObjectType, exceptedType);
                    if (method != null) {
                        Value result = EvalFunction(new DirectMethod(method, new Value[] {constant}));
                        return ResolveValue(result, exceptedType);
                    }
                }
                return constant;
            case Variable variable:
                return ResolveValue(EvalVariable(variable), exceptedType);
            case MethodCall call: {
                Value eval = EvalFunction(call.ToFunctionCall());
                return ResolveValue(eval, exceptedType);
            }
            default:
                return Null;
        }
    }

    /// <summary>
    /// Evaluates a variable and returns its value.
    /// </summary>
    /// <param name="variable">The variable to evaluate.</param>
    /// <returns>The value of the evaluated variable.</returns>
    /// <exception cref="ScriptException">Thrown when the variable is not a class instance or when an infinite loop is detected.</exception>
    public static RealReference EvalVariable(Variable variable) {
        if (variable.Path.Length == 1) {
            return ResolveValue(CurrentScope.Variables.GetValueOrDefault(variable.Path[0], ("NULL", Null)).Item2);
        }

        if (variable.Path.Length == 0) {
            throw Error("Variable path is empty.");
        }

        ClassInstance? currentHop = null;
        for (int i = 0; i < variable.Path.Length - 1; i++) {
            string currentToken = variable.Path[i];
            Debug("Current token (hop token): " + currentToken + ".");
            Dictionary<string, (string, Value)> properties = currentHop == null ? CurrentScope.Variables : currentHop.Properties;
            Value val = properties.GetValueOrDefault(currentToken, ("NULL", Null)).Item2;
            if (val is not ClassInstance classInstance) {
                throw Error("Variable " + currentToken + " is not a class instance.");
            }
            currentHop = classInstance;
        }
        Debug("Current token (final token): " + variable.Path[^1] + ".");
        Value solvedValue = currentHop!.Properties.GetValueOrDefault(variable.Path[^1], ("NULL", Null)).Item2;
        if (solvedValue is Variable) {
            throw Error("Variable " + variable.Path[^1] + " is a variable. INFINITE LOOP DETECTED.");
        }
        return ResolveValue(solvedValue);
    }
    
    /// <summary>
    /// Set the value of a variable.
    /// </summary>
    /// <param name="path">The path of the variable.</param>
    /// <param name="newValue">The value to set the variable to.</param>
    /// <exception cref="ScriptException">If the path is invalid.</exception>
    public static void SetVariable(string[] path, Value newValue) {
        if (path.Length == 1) {
            if (!CurrentScope.Variables.ContainsKey(path[0])) {
                throw Error("Variable '" + path[0] + "' does not exist in this scope.");
            }
            // Assign the variable a value while preserving the type
            CurrentScope.Variables[path[0]] = (CurrentScope.Variables[path[0]].Item1, newValue);
            return;
        }

        ClassInstance? currentHop = null;
        for (int i = 0; i < path.Length - 1; i++) {
            string currentToken = path[i];
            Dictionary<string, (string, Value)> properties = currentHop == null ? CurrentScope.Variables : currentHop.Properties;
            Value val = properties.GetValueOrDefault(currentToken, ("NULL", Null)).Item2;
            if (val is not ClassInstance classInstance) {
                throw Error("Variable " + currentToken + " is not a class instance.");
            }
            currentHop = classInstance;
        }
        // Assign the variable a value while preserving the type
        currentHop!.Properties[path[^1]] = (currentHop.Properties[path[^1]].Item1, newValue);
    }
    
    /// <summary>
    /// Retrieves the method definition of a function call.
    /// </summary>
    /// <param name="call">The function call to get the method definition for.</param>
    /// <param name="parentClass">The class instance that the function belongs to, if any.</param>
    /// <returns>The method definition of the function call, or null if not found.</returns>
    private static MethodDefinition? GetMethodDefinition(FunctionCall call, out ClassInstance? parentClass) {
        if (call.Path.Length == 1) {
            parentClass = null;
            return CurrentScope.Functions!.GetValueOrDefault(call.Path[0], null);
        }
        
        ClassInstance? currentHop = null;
        for (int i = 0; i < call.Path.Length - 1; i++) {
            string currentToken = call.Path[i];
            Dictionary<string, (string, Value)> properties = currentHop == null ? CurrentScope.Variables : currentHop.Properties;
            Value val = properties.GetValueOrDefault(currentToken, ("NULL", Null)).Item2;
            if (val is not ClassInstance classInstance) {
                throw Error("Variable " + currentToken + " is not a class instance.");
            }
            currentHop = classInstance;
        }
        parentClass = currentHop;
        return currentHop!.Methods!.GetValueOrDefault(call.Path[^1], null);
    }

    /// <summary>
    /// Evaluates a method call and returns its result.
    /// </summary>
    /// <param name="call">The method call to execute.</param>
    /// <returns>The return value of the method.</returns>
    /// <exception cref="ScriptException">Invalid method call.</exception>
    private static Value EvalFunction(FunctionCall call) {
        string callName = call.Path[0];
        
        if (BuiltIns.Functions.TryGetValue(callName, out Func<Value[], Value>? function)) {
            // Check arg validity
            MethodDefinition def = BuiltIns.MethodDefinitions[callName];
            if (def.Arguments.Length != call.Arguments.Length) {
                throw Error("Argument count mismatch in function call: " + callName + ". Expected " + def.Arguments.Length + " but got " + call.Arguments.Length);
            }
            Value[] evaledArgs = new Value[call.Arguments.Length];
            for (int i = 0; i < call.Arguments.Length; i++) {
                evaledArgs[i] = ResolveValue(call.Arguments[i], def.ArgumentTypes[i]);
            }
            for (int i = 0; i < def.Arguments.Length; i++) {
                if (def.ArgumentTypes[i] == "any" || (def.ArgumentTypes[i] == "any[]" && evaledArgs[i].ObjectType.EndsWith("[]"))) {
                    continue;
                }
                if (def.ArgumentTypes[i] != evaledArgs[i].ObjectType) {
                    throw Error("Argument type mismatch in function call: " + callName + ". Expected " + def.ArgumentTypes[i] + " but got " + call.Arguments[i].ObjectType + ".");
                }
            }
            return function.Invoke(evaledArgs);
        }
        
        MethodDefinition? method = GetMethodDefinition(call, out ClassInstance? parentClass);
        if (method == null) {
            throw Error("Unknown function call: " + callName);
        }

        NewScope();
        if (parentClass != null) {
            // Add its variables and methods to the scope
            foreach (KeyValuePair<string, (string, Value)> kvp in parentClass.Properties) {
                CurrentScope.Variables[kvp.Key] = kvp.Value;
            }
            foreach (KeyValuePair<string, MethodDefinition> kvp in parentClass.Methods) {
                CurrentScope.Functions[kvp.Key] = kvp.Value;
            }
        }

        Value result = EvalFunction(method, call.Arguments, callName);
        
        // Check for changed variables
        if (parentClass != null) {
            Debug("Checking for changed variables in class...");
            foreach (KeyValuePair<string, (string, Value)> kvp in parentClass.Properties) {
                if (CurrentScope.Variables[kvp.Key] == parentClass.Properties[kvp.Key]) continue;
                parentClass.Properties[kvp.Key] = CurrentScope.Variables[kvp.Key.Debug("Changed var:")].Debug("To Value:");
            }
        }
        
        EndScope();
        if (parentClass != null) {
            SetVariable(call.Path[..^1], parentClass);
        }
        return result;
    }

    /// <summary>
    /// Evaluates a method call and returns its result.
    /// </summary>
    /// <param name="call">The method call to execute.</param>
    /// <returns>The return value of the method.</returns>
    /// <exception cref="ScriptException">Invalid method call.</exception>
    private static Value EvalFunction(DirectMethod call) {
        return EvalFunction(call.Method, call.Arguments, "DIRECT METHOD");
    }

    /// <summary>
    /// Evaluates a method call and returns its result.
    /// </summary>
    /// <param name="method">The method to execute.</param>
    /// <param name="args">The arguments to pass to the method.</param>
    /// <param name="callName">The name of the method call.</param>
    /// <returns>The return value of the method.</returns>
    /// <exception cref="ScriptException">Invalid method call.</exception>
    private static Value EvalFunction(MethodDefinition method, Value[] args, string callName = "NOT SPECIFIC") {
        if (method.Arguments.Length != args.Length) {
            throw Error("Argument count mismatch in function call: " + callName + ". Expected " + method.Arguments.Length + " but got " + args.Length);
        }
        NewScope();
        for (int i = 0; i < method.Arguments.Length; i++) {
            string argumentType = method.ArgumentTypes[i];
            bool isAny = argumentType == "any" || (argumentType == "any[]" && args[i].ObjectType.EndsWith("[]"));
            if (argumentType != args[i].ObjectType && !isAny) {
                throw Error("Argument type mismatch in function call: " + callName + ". Expected " + argumentType + " but got " + args[i].ObjectType + ".");
            }
            string arg = method.Arguments[i];
            CurrentScope.SetVariable(arg, ResolveValue(args[i], argumentType));
        }

        Value result = method.CsFunc != null ? method.CsFunc.Invoke(args) : ExecuteFunction(method);
        EndScope();
        return result;
    }

    /// <summary>
    /// Execute a script.
    /// </summary>
    /// <param name="script">The script to execute.</param>
    /// <returns>The exit code of the script.</returns>
    public static int Execute(ParsedScript script) {
        // Global scope
        _scopes = new Stack<Scope>();
        _scopes.Push(new Scope());

        foreach (ClassDefinition cl in script.Classes) {
            CurrentScope.Classes[cl.Name] = cl;
        }

        MethodDefinition main = new() {
            Name = "main",
            Arguments = new[] {"main"},
            Statements = script.Statements,
            ReturnType = "int"
        };
        Value rawReturn;
        try {
            rawReturn = ExecuteFunction(main);
        }
        catch (ScriptException e) {
            string json = JsonConvert.SerializeObject(e.ExceptionObject);
            Console.WriteLine("An unhandled exception occurred: " + json);
            return 1;
        }
        Value result = ResolveValue(rawReturn);
        if (result.ObjectType != "int" || result is not Constant constant) {
            return 0;
        }
        return int.Parse(constant.Value);
    }

    /// <summary>
    /// Execute a function from its method definition.
    /// </summary>
    /// <param name="function">The method to execute.</param>
    /// <returns>The return value of the function.</returns>
    /// <exception cref="ScriptException">If a statement is invalid.</exception>
    public static Value ExecuteFunction(MethodDefinition function) {
        return ExecuteFunction(function, out _);
    }
    
    /// <summary>
    /// Execute a function from its method definition.
    /// </summary>
    /// <param name="function">The method to execute.</param>
    /// <param name="didReturn">True if the function returned explicitly, false otherwise.</param>
    /// <returns>The return value of the function.</returns>
    /// <exception cref="ScriptException">If a statement is invalid.</exception>
    public static Value ExecuteFunction(MethodDefinition function, out bool didReturn) {

        try {
            if (function.Statements == null!) {  // If this is true then this function will throw
                function.Name.Print("Function has no statements:");
            }
            foreach (Statement statement in function.Statements!) {
                switch (statement) {

                    case FunctionCall call: {
                        EvalFunction(call);
                        break;
                    }
                    
                    case ThrowStatement throwStatement: {
                        Value result = ResolveValue(throwStatement.Exception);
                        throw new ScriptException(result);
                    }
                    
                    case TryCatchStatement tryCatchStatement: {
                        try {
                            NewScope();
                            Value retVal = ExecuteFunction(new MethodDefinition {
                                Name = "try",
                                Arguments = new[] {"try"},
                                Statements = tryCatchStatement.Statements,
                                ReturnType = "any"
                            }, out bool returned);
                            EndScope();
                            if (returned) {
                                didReturn = true;
                                return retVal;
                            }
                        }
                        catch (ScriptException e) {
                            if (tryCatchStatement.CatchStatements == null) {
                                throw;
                            }
                            NewScope();
                            CurrentScope.Variables[tryCatchStatement.ExceptionName] = (e.ExceptionObject.ObjectType, e.ExceptionObject);
                            Value retVal = ExecuteFunction(new MethodDefinition {
                                Name = "catch",
                                Arguments = new[] {"catch"},
                                Statements = tryCatchStatement.CatchStatements,
                                ReturnType = "any"
                            }, out bool returned);
                            EndScope();
                            if (returned) {
                                didReturn = true;
                                return retVal;
                            }
                        }
                        break;
                    }

                    case VariableInit init: {
                        if (CurrentScope.Variables.ContainsKey(init.Name)) {
                            throw Error("Variable " + init.Name + " already exists in this scope.");
                        }
                        string expectedType = init.VariableType;
                        Value val = ResolveValue(init.Value, expectedType);
                        if (val.ObjectType != expectedType) {
                            // Wrong type
                            throw Error("Variable " + init.Name + " is of type " + expectedType + " but tried to set it to " + val.ObjectType);
                        }
                        CurrentScope.SetVariable(init.Name, val);
                        Debug("Set variable " + init.Name + " to " + val);
                        break;
                    }

                    case MethodDefinition def: {
                        CurrentScope.Functions[def.Name] = def;
                        Debug("Defined function " + def.Name);
                        break;
                    }

                    case ReturnStatement ret: {
                        if (function.ReturnType != ret.Value.ObjectType && function.ReturnType != "any") {
                            throw Error("Return type mismatch in function " + function.Name + ". Expected " +
                                        function.ReturnType + " but got " + ret.Value.ObjectType + ".");
                        }
                        didReturn = true;
                        return ret.Value;
                    }

                    case VariableSet set: {
                        Value val = ResolveValue(set.Value, set.ObjectType);
                        if (val.ObjectType != set.ObjectType) {  // Wrong type
                            throw Error("Variable " + set.Path.CombineVariablePath() + " is of type " + set.ObjectType + " but tried to set it to " + val.ObjectType);
                        }
                        SetVariable(set.Path, val);
                        Debug("Set variable " + set.Path.CombineVariablePath() + " to " + val);
                        break;
                    }

                    case IfStatement ifStatement: {
                        RealReference conditionRef = ResolveValue(ifStatement.Condition, "bool");
                        if (conditionRef.ObjectType != "bool" || conditionRef is not Constant condition) {
                            throw Error("If condition must be of type bool");
                        }

                        if (condition.Value == "true") {
                            Debug("If condition is true");
                            Value result = ExecuteFunction(new MethodDefinition {
                                Name = "if",
                                Arguments = new[] {"if"},
                                Statements = ifStatement.Statements,
                                ReturnType = "any"
                            }, out bool returned);
                            if (returned) {
                                didReturn = true;
                                return result;
                            }
                        }
                        else {
                            Debug("If condition is false");
                            Value result = ExecuteFunction(new MethodDefinition {
                                Name = "if",
                                Arguments = new[] {"if"},
                                Statements = ifStatement.ElseStatements,
                                ReturnType = "any"
                            }, out bool returned);
                            if (returned) {
                                didReturn = true;
                                return result;
                            }
                        }

                        break;
                    }
                    
                    case WhileStatement whileStatement: {
                        RealReference conditionRef = ResolveValue(whileStatement.Condition, "bool");
                        if (conditionRef.ObjectType != "bool" || conditionRef is not Constant condition) {
                            throw Error("While condition must be of type bool");
                        }

                        while (condition.Value == "true") {
                            Debug("While condition is true");
                            Value result = ExecuteFunction(new MethodDefinition {
                                Name = "while",
                                Arguments = new[] {
                                    "while"
                                },
                                Statements = whileStatement.Statements,
                                ReturnType = "null"
                            }, out bool returned);
                            if (returned) {
                                didReturn = true;
                                return result;
                            }
                            conditionRef = ResolveValue(whileStatement.Condition, "bool");
                            if (conditionRef.ObjectType != "bool" || conditionRef is not Constant @ref) {
                                throw Error("While condition must be of type bool");
                            }
                            condition = @ref;
                        }

                        break;
                    }
                    
                    case EnterScopeStatement: {
                        NewScope();
                        break;
                    }
                    
                    case ExitScopeStatement: {
                        EndScope();
                        break;
                    }
                    
                    case LoadLibStatement loadLib: {
                        LoadLibrary(loadLib.Lib);
                        break;
                    }
                    
                    default: {
                        throw Error("Unknown statement type: " + statement.Type);
                    }

                }
            }
        }
        catch (ProgramExitException e) {
            Environment.Exit(e.ExitCode);
        }
        
        didReturn = false;
        return Constant.Null;
    }
    
}