using Newtonsoft.Json;
using WhatTimeIsIt;
using WhatTimeIsIt.ParsedScripts;

Dictionary<string, string> switches = Utils.ParseSwitches(args);

if (switches.ContainsKey("help")) {
    Utils.DumpHelp();
    return 0;
}

if (switches.TryGetValue("interpret", out string? fileToRun)) {
    string parsedCode = File.ReadAllText(fileToRun);
    ParsedScript s = JsonConvert.DeserializeObject<ParsedScript>(parsedCode)!;
    return s.Run();
}

if (!Utils.TryGetArg(args, 0, out string file)) {
    Utils.DumpHelp();
    return 1;
}

if (!File.Exists(file)) {
    Console.Error.WriteLine($"File not found: {file}");
    return 1;
}

string code = File.ReadAllText(file);
ParsedScript script = Parser.Parse(code);

string filename = Path.GetFileNameWithoutExtension(file);
File.WriteAllText($"{filename}.wtiic", JsonConvert.SerializeObject(script, Formatting.Indented));

if (File.Exists("libwtiiint.so")) {  // Run with C++ for speed if available
    return CppInterpreter.interpret("parsed_script.json");
}

// Run with C# interpreter
int exitCode = script.Run();
return exitCode;