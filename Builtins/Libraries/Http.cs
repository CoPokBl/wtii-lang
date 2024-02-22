using WhatTimeIsIt.ParsedScripts;
using WhatTimeIsIt.ParsedScripts.Statements;
using WhatTimeIsIt.ParsedScripts.Values;

namespace WhatTimeIsIt.Builtins.Libraries;

public class Http : IWtiiLibrary {
    public Scope Scope { get; set; }
    
    private static readonly ClassDefinition HttpResponseClass = new() {
        Name = "HttpResponse",
        Variables = new VariableInit[] {
            new() {
                Name = "status",
                Value = new Constant("0", "int"),
                VariableType = "int"
            },
            new() {
                Name = "body",
                Value = new Constant("", "string"),
                VariableType = "string"
            },
            new() {
                Name = "headers",
                Value = new Constant("{}", "string"),
                VariableType = "json"
            }
        }
    };

    public Http() {
        Scope = new Scope();
        Scope.Classes.Add("HttpResponse", HttpResponseClass);
        Scope.Functions.Add("http_request", new MethodDefinition("http_request", "HttpResponse", 
            ("url", "string"), 
            ("method", "string"), 
            ("body", "string"), 
            ("headers", "string")) {
            CsFunc = HttpRequest
        });
        Scope.Variables.Add("http", new Constant("HTTP Exists!", "string"));
    }
    
    public void Init() { }

    public void Run() {
        Console.WriteLine("Http Lib Loaded!");
    }
    
    private Value HttpRequest(Value[] args) {
        string url = GetStringFromValue(args[0]);
        string method = GetStringFromValue(args[1]);
        string body = GetStringFromValue(args[2]);
        string headers = GetStringFromValue(args[3]);
        HttpClient client = new();
        HttpRequestMessage request = new(new HttpMethod(method), url);
        request.Content = new StringContent(body);
        request.Content.Headers.ContentType = new("application/json");
        request.Headers.Add("headers", headers);
        HttpResponseMessage response = client.Send(request);
        string responseString = response.Content.ReadAsStringAsync().Result;

        ClassInstance responseInstance = new(HttpResponseClass) {
            Properties = {
                ["status"] = new Constant(((int) response.StatusCode).ToString(), "int"),
                ["body"] = new Constant(responseString, "string"),
                ["headers"] = new Constant(response.Headers.ToString(), "string")
            }
        };
        return responseInstance;
    }
    
    private string GetStringFromValue(Value value) {
        return value switch {
            Constant constant => constant.Value,
            _ => throw new Exception("Value is not a string")
        };
    }
    
}