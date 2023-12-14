using WhatTimeIsIt;
using WhatTimeIsIt.ParsedScripts;

Dictionary<string, string> switches = Utils.ParseSwitches(args);

if (switches.ContainsKey("help")) {
    Utils.DumpHelp();
    return 0;
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
int exitCode = script.Run();

return exitCode;