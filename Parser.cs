using System.Reflection;
using Newtonsoft.Json;
using WhatTimeIsIt.Builtins;
using WhatTimeIsIt.ParsedScripts;
using WhatTimeIsIt.ParsedScripts.Statements;
using WhatTimeIsIt.ParsedScripts.Values;

namespace WhatTimeIsIt;

public static class Parser {

    private static readonly char[] ReservedCharacters = {
        '+', '=', '-', '/', '*', ' ', '(', ')', '{', '}', ';', '[', ']', '!', '"'
    };
    
    /// <summary>
    /// A scope contains all variables and functions that are accessible in the current scope.
    /// </summary>
    private static readonly Stack<Scope> Scopes = new();
    
    /// <summary>
    /// Gets the currently loaded scope which will
    /// include all variables and functions from the current scope and all parent scopes.
    /// </summary>
    private static Scope CurrentScope => Scopes.Peek();
    
    /// <summary>
    /// Gets a scope that only contains builtins.
    /// </summary>
    private static Scope DefaultScope => new(true);
    
    /// <summary>
    /// Creates a new scope and pushes it to the stack.
    /// This scope will only contain the values from the previous scope.
    /// </summary>
    private static void NewScope() => Scopes.Push(Scopes.Count == 0 ? DefaultScope : new Scope(CurrentScope));
    
    /// <summary>
    /// Removes the current scope from the stack.
    /// </summary>
    private static void EndScope() => Scopes.Pop();
    
    private static string CurrentLine = "";

    /// <summary>
    /// Gets the class that the dot notation variable exists in.
    /// </summary>
    /// <param name="value">The dot notation variable (Ex. person.name).</param>
    /// <param name="parts">The path of the variable.</param>
    /// <param name="lastSymbol">The variable that exists in the returned class instance. It is the last value in the path.</param>
    /// <returns>A class instance which contains the lastSymbol value.</returns>
    /// <exception cref="Exception">If any values do not exist or are invalid.</exception>
    private static ClassInstance? GetFinalDotNotationValue(string value, out string[] parts, out string lastSymbol) {
        parts = value.Split('.');
        string varName = parts[^1];
        
        if (parts.Length == 1) {
            lastSymbol = value;
            return null;
        }
        
        // For each part except the last part
        (string, Value)? lastVar = null;
        for (int i = 0; i < parts.Length - 1; i++) {
            string part = parts[i];
            string requiredNextVariableName = parts[i + 1];
            if (lastVar != null && lastVar.Value.Item2 is not ClassInstance) {
                throw new Exception("lastVar is not a class instance.");
            }
            ClassInstance? classInstance = lastVar?.Item2 as ClassInstance;
            Dictionary<string, (string, Value)> vars = lastVar == null ? CurrentScope.Variables : classInstance!.Properties;
            if (!vars.TryGetValue(part, out (string, Value) var)) {
                throw new Exception("Unknown variable:" + part + ".");
            }
            lastVar = var;
            if (!CurrentScope.Classes.TryGetValue(var.Item1, out ClassDefinition? classDef)) {
                throw new Exception("Unknown class: " + var.Item1 + ".");
            }
            if (classDef.Variables.All(v => v.Name != requiredNextVariableName) && classDef.Methods.All(v => v.Name != requiredNextVariableName)) {
                throw new Exception("Unknown variable: " + varName + ".");
            }
        }

        lastSymbol = parts[^1];
        
        if (lastVar?.Item2 is MethodCall mc) {  // Get the return type because it's a reference
            string classType = lastVar.Value.Item1;
            if (!CurrentScope.Classes.TryGetValue(classType, out ClassDefinition? classDef)) {
                throw new Exception("Unknown class: " + classType + ".");
            }
            return new ClassInstance(classDef);
        }
        
        if (lastVar?.Item2 is not ClassInstance cI) {
            throw new Exception("lastVar is null or not class.");
        }
        return cI;
    }
    
    /// <summary>
    /// Parses the variable reference string into a variable value object.
    /// </summary>
    /// <param name="value">The reference string (Ex. person.name).</param>
    /// <param name="type">The type of the returned variable.</param>
    /// <returns>The variable that the string represents.</returns>
    /// <exception cref="Exception">If any of the variables don't exist or are invalid.</exception>
    private static Variable ParseDotNotationVariable(string value, out string type) {
        ClassInstance? finalInstance = GetFinalDotNotationValue(value, out string[] parts, out string finalPart);

        if (finalInstance == null) {  // Current scope
            if (!CurrentScope.Variables.ContainsKey(value)) {
                throw new Exception("Unknown variable:" + value + ".");
            }
            type = CurrentScope.Variables[value].Item1;
            return new Variable(parts, type);
        }
        
        if (!finalInstance.Properties.TryGetValue(finalPart, out (string, Value) finalVar)) {
            throw new Exception("Unknown variable:" + finalPart + ".");
        }
        
        // Every check passed
        type = finalVar.Item1;
        return new Variable(parts, type);
    }
    
    /// <summary>
    /// Parses the method reference string into a method definition object.
    /// </summary>
    /// <param name="value">The reference string [Ex. person.kill()].</param>
    /// <param name="parts">The path of the method.</param>
    /// <param name="type">The return type of the returned method.</param>
    /// <returns>The method represented by the value string.</returns>
    /// <exception cref="Exception">If any of the variables don't exist or are invalid or the method doesn't exist.</exception>
    private static MethodDefinition ParseDotNotationMethod(string value, out string[] parts, out string type) {
        ClassInstance? finalInstance = GetFinalDotNotationValue(value, out parts, out string finalPart);
        
        if (finalInstance == null) {  // Current scope
            if (!CurrentScope.Functions.ContainsKey(value)) {
                throw new Exception("Unknown function:" + value + ".");
            }
            type = CurrentScope.Functions[value].ReturnType;
            return CurrentScope.Functions[value];
        }
        
        if (!finalInstance.Methods.TryGetValue(finalPart, out MethodDefinition? finalMethod)) {
            throw new Exception("Unknown method:" + finalPart + ".");
        }
        
        // Every check passed
        type = finalMethod.ReturnType;
        return finalMethod;
    }
    
    /// <inheritdoc cref="EvalValue(string, out string)"/>
    private static Value EvalValue(string value) => EvalValue(value, out _);
    
    /// <summary>
    /// Evaluates a value string to a Value object.
    /// </summary>
    /// <param name="value">The value string.</param>
    /// <param name="type">The object type of the returned value.</param>
    /// <returns>The value object represented by the value string.</returns>
    /// <exception cref="Exception">If the value string is invalid.</exception>
    private static Value EvalValue(string value, out string type) {
        value = value.Trim();
        
        if (value == "NULL") {
            type = "NULL";
            return Constant.Null;
        }
        
        if (value.Contains('(') && value.Contains(')')) {
            // To get the method name we need to go backwards from the ( until we hit a reserved character
            string methodName = "";
            for (int i = value.IndexOf('(') - 1; i >= 0; i--) {
                char c = value[i];
                if (ReservedCharacters.Contains(c)) {
                    break;
                }
                methodName = c + methodName;
            }
            
            ParseDotNotationMethod(methodName, out string[] parts, out type);
            string argsString = value.Split('(', 2)[1][..^1];
            string[] args = argsString.Split(',');
            Value?[] valueArgs = argsString == "" ? Array.Empty<Value>() : args.Select(EvalValue).ToArray();
            return new MethodCall(parts, valueArgs.Select(v => v ?? new Constant("NULL", "NULL")).ToArray(), type);
        }
        
        // Check for operators by going through each character checking if it's an operator and ignoring if it's part of a string
        bool inString = false;
        for (int i = 0; i < value.Length; i++) {
            char c = value[i];
            if (c == '"') {
                inString = !inString;
            }

            if (inString) {
                continue;
            }

            char nc = value.Length-1 <= i ? ' ' : value[i + 1];
            string? op = null;
            if (c == '!' && nc == '=') op = "!=";
            else if (c == '=' && nc == '=') op = "==";
            else if (c == '<' && nc == '=') op = "<=";
            else if (c == '>' && nc == '=') op = ">=";
            else if (c == '|' && nc == '|') op = "||";
            else if (c == '&' && nc == '&') op = "&&";
            else if (c == '<') op = "<";
            else if (c == '>') op = ">";

            // Logical boolean operators
            if (!inString && op != null) {
                string[] parts = value.Split(op, 2);
                string left = parts[0];
                string right = parts[1];
                Value leftValue = EvalValue(left);
                Value rightValue = EvalValue(right);
                if (leftValue.ObjectType != rightValue.ObjectType) {
                    throw new Exception("Cannot compare values of different types. Left: " +
                                        leftValue.ObjectType + ", Right: " + rightValue.ObjectType + ".");
                }

                string opFunc = op switch {
                    "==" => "equals",
                    "!=" => "not_equals",
                    "<" => "less_than",
                    "<=" => "less_than_or_equals",
                    ">" => "more_than",
                    ">=" => "more_than_or_equals",
                    "||" => "or",
                    "&&" => "and",
                    _ => throw new Exception("Unknown operator: " + op)
                };
                
                type = "bool";
                MethodCall call = new(opFunc.SingleEnumerate(), new[] {
                    leftValue, rightValue
                }, "bool");
                
                // Handle special ops
                if (op == "!=") {  // Just puts equals through a not
                    call.Path = "equals".SingleEnumerate();
                    return MethodCall.Not(call);
                }

                return call;
            }

            if (!inString && c == '!') {  // Not is special because it has no 'left' and 'right' side.
                string right = value[(i + 1)..];
                Value rightValue = EvalValue(right);
                if (rightValue.ObjectType != "bool") {
                    throw new Exception("Cannot use not operator on non-boolean value.");
                }
                type = "bool";
                return MethodCall.Not(rightValue);
            }

        }
        
        inString = false;
        for (int i = 0; i < value.Length; i++) {
            char c = value[i];
            if (c == '"') {
                inString = !inString;
            }

            if (!inString && c is '+' or '-' or '*' or '/') {
                string[] parts = value.Split(c, 2);
                string left = parts[0];
                string right = parts[1];
                Value leftValue = EvalValue(left);
                Value rightValue = EvalValue(right);
                if (leftValue.ObjectType != rightValue.ObjectType) {
                    throw new Exception("Cannot perform operation on values of different types. Left: " +
                                        leftValue.ObjectType + ", Right: " + rightValue.ObjectType + ".");
                }

                string opFunc = c switch {
                    '+' => leftValue.ObjectType == "string" ? "concat" : "add",
                    '-' => "subtract",
                    '*' => "multiply",
                    '/' => "divide",
                    _ => throw new Exception("Unknown operator: " + c)
                };
                Value result = new MethodCall(opFunc.SingleEnumerate(), new[] {
                    leftValue, rightValue
                }, leftValue.ObjectType);
                type = result.ObjectType;
                return result;
            }

            
        }

        if (CurrentScope.Classes.ContainsKey(value)) {  // We have a class reference
            type = "class";
            return new Constant(value, "class");
        }
        
        if (value is "true" or "false") {
            type = "bool";
            return new Constant(value, "bool");
        }
        
        if (int.TryParse(value, out int _)) {
            type = "int";
            return new Constant(value, "int");
        }

        if (float.TryParse(value, out float _)) {
            type = "float";
            return new Constant(value, "float");
        }
        
        if (value.StartsWith("\"") && value.EndsWith("\"")) {
            value = value[1..^1];
            type = "string";
            return new Constant(value, "string");
        }

        if (value.StartsWith("new ")) {
            // New instance of class
            string className = value[4..];
            if (!CurrentScope.Classes.TryGetValue(className, out ClassDefinition? classDef)) {
                throw new Exception("Unknown class: " + className + ".");
            }
            type = className;
            return new ClassInstance(classDef);
        }

        if (value.StartsWith('[') && value.EndsWith(']')) {
            // Array init
            string[] parts = value[1..^1].SafeSplit(',');
            parts = parts.Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            Value[] values = parts.Select(EvalValue).ToArray();
            string? vType = null;
            if (!values.All(v => {
                    if (vType == null) {
                        vType = v.ObjectType;
                        return true;
                    }
                    bool didMatch = v.ObjectType == vType;
                    vType = v.ObjectType;
                    return didMatch;
                })) {
                throw new Exception("Array values must all be the same type.");
            }
            vType ??= "NULL";
            type = vType;
            return new ArrayValue(type, values);
        }
        
        bool hasArrayIndex = value.Contains('[') && value.EndsWith(']');
        string dotNotationValue = hasArrayIndex ? value.Split('[')[0] : value;
        Variable var = ParseDotNotationVariable(dotNotationValue, out type);
        
        if (!hasArrayIndex) return var;  // Normal variable, not indexed
        
        {  // Navigate array
            string[] parts = value.Split('[');
            string indexString = parts[1][..^1];
            Value index = EvalValue(indexString);
            if (index.ObjectType != "int") {
                throw new Exception("Array index must be an integer.");
            }
            return new MethodCall("get_array_object".SingleEnumerate(), new[] {var, index}, "string");
        }
    }
    
    /// <summary>
    /// Gets body lines between two braces.
    /// </summary>
    /// <param name="lines">The full set of lines that includes the intended body.</param>
    /// <param name="startIndex">The index to start looking at. Should be the index of the first brace.</param>
    /// <param name="finishPoint">The index of the closing brace.</param>
    /// <returns>A list of all lines in the body.</returns>
    private static string[] GetBody(string[] lines, int startIndex, out int finishPoint) {
        int closingBracket = -1;
        int bracketCount = 0;
        for (int i = startIndex; i < lines.Length; i++) {
            string line2 = lines[i];
            for (int j = 0; j < line2.Length; j++) {
                char c2 = line2[j];
                if (c2 == '{') {
                    bracketCount++;
                } else if (c2 == '}') {
                    bracketCount--;
                    if (bracketCount == 0) {
                        closingBracket = i;
                        break;
                    }
                }
            }
            if (closingBracket != -1) {
                break;
            }
        }

        // Get a list of lines in the body and remove the brackets
        string[] bodyLines = lines[(startIndex + 2)..(closingBracket)];
        finishPoint = closingBracket;
        return bodyLines;
    }

    /// <summary>
    /// Parse a class into a class definition.
    /// </summary>
    /// <param name="name">The name of the class.</param>
    /// <param name="lines">The body lines of the class.</param>
    /// <returns>The class definition.</returns>
    /// <exception cref="Exception">If the class is invalid.</exception>
    private static ClassDefinition ParseClass(string name, string[] lines) {
        NewScope();
        List<MethodDefinition> methods = new();
        List<VariableInit> variables = new();

        for (int statement = 0; statement < lines.Length; statement++) {
            string line = lines[statement];
            string l = line.Trim();

            if (l == "") {
                continue;
            }

            string token = "";
            bool handled = false;
            
            foreach (char c in l) {
                switch (c) {
                    case '(':
                    // Function definition
                    {
                        // Check if it's a function definition instead of a function call
                        if (token.Contains(' ')) {
                            string[] parts = token.Split(' ');
                            string returnType = parts[0];
                            string funcName = parts[1];
                            string argsString = l[(token.Length + 1)..^1];
                            string[] args = argsString.Split(',').Where(s => s.Trim() != "").ToArray();
                            Dictionary<string, string> argTypes = new();
                            foreach (string arg in args) {
                                string[] argParts = arg.Split(' ');
                                string argType = argParts[0];
                                string argName = argParts[1];
                                argTypes[argName] = argType;
                            }
                            
                            // Get body by finding the matching closing bracket
                            string[] bodyLines = GetBody(lines, statement, out int closingBracket);
                            
                            NewScope();
                            
                            // Add the arguments to the scope
                            foreach ((string? argName, string? argType) in argTypes) {
                                CurrentScope.Variables[argName] = ("NULL", new Constant("NULL", argType));
                            }
                            
                            // Add class variables and method to scope
                            foreach (VariableInit variable in variables) {
                                CurrentScope.Variables[variable.Name] = (variable.VariableType, variable.ToValue());
                            }
                            foreach (MethodDefinition method in methods) {
                                CurrentScope.Functions[method.Name] = method;
                            }
                            
                            MethodDefinition def = new() {
                                Name = funcName,
                                ReturnType = returnType,
                                Arguments = argTypes.Keys.ToArray(),
                                ArgumentTypes = argTypes.Values.ToArray(),
                                Statements = Parse(bodyLines).Statements
                            };
                            EndScope();  // End the scope that Parse creates
                            
                            methods.Add(def);
                            CurrentScope.Functions[funcName] = def;
                            handled = true;
                            
                            // Move the statement index to the closing bracket
                            statement = closingBracket;
                        }

                        break;
                    }

                    case '=':
                    // Variable init
                    // Example: int x = 5;
                    {
                        if (l.Split('=')[0].Trim().Split(' ').Length == 2) {
                            string[] parts = l.Split('=', 2);

                            string declaration = parts[0].Trim();
                            string value = parts[1].Trim();
                            string varName = declaration.Split(' ')[1];
                            string type = declaration.Split(' ')[0];
                            VariableInit init = new() {
                                Name = varName,
                                Value = EvalValue(value),
                                VariableType = type
                            };
                            variables.Add(init);
                            CurrentScope.Variables[varName] = (init.VariableType, init.Value);
                            handled = true;
                        }

                        break;
                    }
                    
                }

                if (handled) break;
                token += c;
            }

            if (handled) continue;

            // It did not get handled
            // It's invalid
            throw new Exception($"Invalid statement: {l}");
        }
        
        EndScope();
        return new ClassDefinition {
            Name = name,
            Methods = methods.ToArray(),
            Variables = variables.ToArray()
        };
    }

    /// <summary>
    /// Parses a script into a ParsedScript object.
    /// </summary>
    /// <param name="code">The raw code to parse.</param>
    /// <returns>A fully parsed script object.</returns>
    /// <exception cref="Exception">If the script it invalid in any way.</exception>
    public static ParsedScript Parse(string code) {
        // Split code into lines (newline or semicolon as delimiter but \ escapes newline)
        string[] lines = SplitCode(code);
        return Parse(lines);
    }
    
    public static void LoadLibrary(string path) {
        if (!File.Exists(path) && !BuiltIns.Libraries.ContainsKey(path)) {
            throw new Exception("Library file does not exist: " + path);
        }
        
        string fileExtension = Path.GetExtension(path);
        List<IWtiiLibrary> libraries = new();
        switch (fileExtension) {
            case ".wtii": {
                ParsedScript script = Parse(File.ReadAllText(path));
                foreach (ClassDefinition cd in script.Classes) {
                    CurrentScope.Classes[cd.Name] = cd;
                }
                foreach (Statement statement in script.Statements) {
                    if (statement is not MethodDefinition method) {
                        continue;
                    }
                    CurrentScope.Functions[method.Name] = method;
                }
                break;
            }

            case ".dll": {
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
                    throw new Exception("Library file type not supported: " + fileExtension);
                }

                Type libraryType = BuiltIns.Libraries[path];
                IWtiiLibrary library = (IWtiiLibrary) Activator.CreateInstance(libraryType)!;
                libraries.Add(library);
                break;
            }

            default:
                throw new Exception("Library file type not supported: " + fileExtension);
        }

        foreach (IWtiiLibrary library in libraries) {
            library.Init();
            CurrentScope.AppendScope(library.Scope);
        }
    }

    /// <summary>
    /// Parses a script into a ParsedScript object.
    /// </summary>
    /// <param name="lines">The lines in the script.</param>
    /// <param name="newScope">Whether or not to create a new scope for the script.</param>
    /// <returns>A fully parsed script object.</returns>
    /// <exception cref="Exception">If the script it invalid in any way.</exception>
    private static ParsedScript Parse(string[] lines, bool newScope = true) {
        if (newScope) NewScope();
        List<Statement> statements = new();
        List<ClassDefinition> classes = new();

        try {
            for (int statement = 0; statement < lines.Length; statement++) {
                string line = lines[statement];
                string l = line.Trim();
                CurrentLine = l;  // For debugging

                if (l == "") {
                    continue;
                }

                string token = "";
                bool handled = false;
                foreach (char c in l) {
                    switch (c) {
                        case '(':
                        // Function definition
                        {
                            // Check if it's a function definition instead of a function call
                            if (token.Contains(' ')) {
                                string[] parts = token.Split(' ');
                                string returnType = parts[0];
                                string name = parts[1];
                                string argsString = l[(token.Length + 1)..^1];
                                string[] args = argsString.Split(',').Where(s => s.Trim() != "").ToArray();
                                Dictionary<string, string> argTypes = new();
                                foreach (string arg in args) {
                                    string[] argParts = arg.Split(' ');
                                    string argType = argParts[0];
                                    string argName = argParts[1];
                                    argTypes[argName] = argType;
                                }
                                
                                // Get body by finding the matching closing bracket
                                string[] bodyLines = GetBody(lines, statement, out int closingBracket);
                                
                                NewScope();
                                
                                // Add the arguments to the scope
                                foreach ((string? argName, string? argType) in argTypes) {
                                    CurrentScope.Variables[argName] = ("NULL", new Constant("NULL", argType));
                                }
                                
                                MethodDefinition def = new() {
                                    Name = name,
                                    ReturnType = returnType,
                                    Arguments = argTypes.Keys.ToArray(),
                                    ArgumentTypes = argTypes.Values.ToArray(),
                                    Statements = Parse(bodyLines).Statements
                                };
                                EndScope();  // End the scope that Parse creates
                                
                                statements.Add(def);
                                CurrentScope.Functions[name] = def;
                                handled = true;
                                
                                // Move the statement index to the closing bracket
                                statement = closingBracket + 1;
                                break;
                            }
                        }


                        // Function call
                        {
                            string argsString = l[token.Length..];
                            string[] args = argsString == "()" ? 
                                Array.Empty<string>() : 
                                argsString[1..^1].SafeSplit(',').Select(s => s.Trim()).ToArray();

                            ParseDotNotationMethod(token, out string[] parts, out string type);
                            FunctionCall call = new() {
                                Path = parts,
                                Arguments = args.Select(EvalValue).ToArray()
                            };
                            statements.Add(call);
                            handled = true;
                            break;
                        }


                        case '=':
                        // Variable init
                        // Example: int x = 5;
                        {
                            if (l.Split('=')[0].Trim().Split(' ').Length == 2) {
                                string[] parts = l.Split('=', 2);

                                string declaration = parts[0].Trim();
                                string value = parts[1].Trim();
                                string name = declaration.Split(' ')[1];
                                string type = declaration.Split(' ')[0];
                                VariableInit init = new() {
                                    Name = name,
                                    Value = EvalValue(value),
                                    VariableType = type
                                };
                                statements.Add(init);
                                CurrentScope.Variables[name] = (type, init.Value);
                                handled = true;
                                break;
                            }
                        }
                        
                        // Variable set
                        {
                                string[] parts = l.Split('=', 2);
                                string name = parts[0].Trim();
                                string value = parts[1].Trim();
                                Variable var = ParseDotNotationVariable(name, out _);
                                VariableSet set = new() {
                                    Path = var.Path,
                                    Value = EvalValue(value),
                                    ObjectType = var.ObjectType
                                };
                                statements.Add(set);
                                handled = true;
                                break;
                        }
                        
                        
                        // All space based keyword statements
                        case ' ': {
                            if (token == "return") {
                                string value = l[7..];
                                ReturnStatement ret = new() {
                                    Value = EvalValue(value)
                                };
                                statements.Add(ret);
                                handled = true;
                            }

                            else if (token == "use") {
                                string value = l[4..];
                                
                                Value fileName = EvalValue(value);
                                if (fileName.ObjectType != "string") {
                                    throw new Exception("use() argument must be a string.");
                                }
                                if (fileName is not Constant constFileName) {
                                    throw new Exception("use() argument must be a constant.");
                                }
                                
                                LoadLibrary(constFileName.Value);
                                statements.Add(new LoadLibStatement(constFileName.Value));
                                handled = true;
                            }
                            
                            else if (token == "if") {
                                string condition = l[4..^1];  // Without the brackets
                                Value conditionValue = EvalValue(condition);
                                if (conditionValue.ObjectType != "bool") {
                                    throw new Exception("if() condition must be a boolean.");
                                }
                                IfStatement ifStatementStatement = new() {
                                    Condition = conditionValue
                                };

                                string[] bodyLines = GetBody(lines, statement, out int finishPoint);
                                
                                NewScope();
                                ifStatementStatement.Statements = Parse(bodyLines, false).Statements;
                                EndScope();
                                
                                // Check for else statement
                                if (lines.Length >= finishPoint && lines[finishPoint+1].Trim() == "else") {
                                    string[] elseBodyLines = GetBody(lines, finishPoint+1, out finishPoint);
                                    NewScope();
                                    ifStatementStatement.ElseStatements = Parse(elseBodyLines, false).Statements;
                                    EndScope();
                                }
                                else {
                                    ifStatementStatement.ElseStatements = Array.Empty<Statement>();
                                }
                                
                                statements.Add(ifStatementStatement);
                                statement = finishPoint;
                                handled = true;
                            }

                            else if (token == "while") {
                                string condition = l[7..^1];  // Without the brackets
                                Value conditionValue = EvalValue(condition);
                                if (conditionValue.ObjectType != "bool") {
                                    throw new Exception("while() condition must be a boolean.");
                                }
                                WhileStatement whileStatementStatement = new() {
                                    Condition = conditionValue
                                };

                                string[] bodyLines = GetBody(lines, statement, out int finishPoint);
                                
                                NewScope();
                                whileStatementStatement.Statements = Parse(bodyLines, false).Statements;
                                EndScope();
                                
                                statements.Add(whileStatementStatement);
                                statement = finishPoint;
                                handled = true;
                            }
                            
                            else if (token == "class") {
                                string className = l[6..];
                                string[] bodyLines = GetBody(lines, statement, out int finishPoint);
                                ClassDefinition classDefinition = ParseClass(className, bodyLines);
                                statement = finishPoint;
                                classes.Add(classDefinition);
                                CurrentScope.Classes[className] = classDefinition;
                                handled = true;
                            }
                            
                            else if (token == "throw") {
                                string value = l[6..];
                                Value exception = EvalValue(value);
                                ThrowStatement throwStatement = new(exception);
                                statements.Add(throwStatement);
                                handled = true;
                            }

                            break;
                        }

                        case '{': {
                            NewScope();
                            statements.Add(new EnterScopeStatement());
                            handled = true;
                            break;
                        }
                        
                        case '}': {
                            EndScope();
                            statements.Add(new ExitScopeStatement());
                            handled = true;
                            break;
                        }
                        
                    }

                    if (handled) break;
                    token += c;
                    
                    if (token == "try") {
                        TryCatchStatement tryStatement = new();
                        string[] bodyLines = GetBody(lines, statement, out int finishPoint);
                                
                        NewScope();
                        tryStatement.Statements = Parse(bodyLines, false).Statements;
                        EndScope();
                                
                        // Check for catch statement (must be after the try statement) and must have variable name:
                        // catch (e)
                        if (lines.Length > finishPoint && lines[finishPoint+1].Trim().StartsWith("catch")) {
                            string exceptionName = lines[finishPoint+1].Trim()[lines[finishPoint+1].IndexOf('(')..^1].Trim();
                            string[] catchBodyLines = GetBody(lines, finishPoint+1, out finishPoint);
                            NewScope();
                            CurrentScope.Variables[exceptionName] = ("NULL", Constant.Null);
                            tryStatement.CatchStatements = Parse(catchBodyLines, false).Statements;
                            EndScope();
                            tryStatement.ExceptionName = exceptionName;
                        }
                        else {
                            throw new Exception("try statement must have a catch statement.");
                        }
                                
                        statements.Add(tryStatement);
                        statement = finishPoint;
                        handled = true;
                        break;
                    }
                }

                if (handled) continue;

                // It did not get handled
                // It's invalid
                throw new Exception($"Invalid statement: {l}");
            }
        }
        catch (Exception e) {
            // Something failed
            // Dump debug info
            string debugInfo = "Error on line: " + CurrentLine;
            //debugInfo += "Statements: " + JsonConvert.SerializeObject(statements, Formatting.Indented);
            //debugInfo += "\nClasses: " + JsonConvert.SerializeObject(classes, Formatting.Indented);
            //debugInfo += "\nScope: " + JsonConvert.SerializeObject(CurrentScope, Formatting.Indented);
            Console.WriteLine(debugInfo);
            throw;
        }
        

        ParsedScript script = new() {
            Statements = statements.ToArray(),
            Classes = classes.ToArray()
        };
        
        // It has now been 'compiled' so that it can be saved and loaded
        File.WriteAllText("script_parsed.json", JsonConvert.SerializeObject(script, Formatting.Indented));

        return script;
    }
    
    /// <summary>
    /// Split raw code into lines that have been cleaned up.
    /// </summary>
    /// <param name="code">The raw code.</param>
    /// <returns>A list of lines.</returns>
    private static string[] SplitCode(string code) {
        List<string> lines = new();
        string currentLine = "";
        bool escape = false;
        bool inString = false;
        for (int i = 0; i < code.Length; i++) {
            char c = code[i];
            if (c == '"') {  // So that strings aren't treated as code
                inString = !inString;
            }
            bool isLongEnough = i + 1 < code.Length;
            if (!inString && c == '/' && isLongEnough && code[i + 1] == '/') {
                // Comment
                while (code[i] != '\n') {
                    i++;
                }
                lines.Add(currentLine);
                continue;
            }
            if (!inString && c == '/' && isLongEnough && code[i + 1] == '*') {
                // Multiline comment
                i += 2;
                while (code[i] != '*' && code[i + 1] != '/') {
                    i++;
                }
                i += 2;
                continue;
            }
            if (c is '}' or '{' && !escape) {  // Make sure {} are on their own line
                if (currentLine.Trim() != "") lines.Add(currentLine);
                currentLine = "";
                currentLine += c;
                lines.Add(currentLine);
                currentLine = "";
            } else if (c == '\n' && !escape) {
                if (currentLine.Trim() != "") lines.Add(currentLine);
                currentLine = "";
            } else if (c == ';' && !escape) {
                if (currentLine.Trim() != "") lines.Add(currentLine);
                currentLine = "";
            } else if (c == '\\' && !escape) {
                escape = true;
            } else {
                currentLine += c;
                escape = false;
            }
        }
        if (currentLine != "") {
            lines.Add(currentLine);
        }
        lines.Add("");  // Add an empty line to the end so that the last line is parsed
        return lines.ToArray();
    }
    
}