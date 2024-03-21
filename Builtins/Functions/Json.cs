using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WhatTimeIsIt.ParsedScripts;
using WhatTimeIsIt.ParsedScripts.Values;

namespace WhatTimeIsIt.Builtins.Functions;

public static class Json {
    
    public static Value ToJson(Value[] args) {
        RealReference ar = Interpreter.ResolveValue(args[0]);
        Dictionary<string, (string, Value)> properties;
        if (ar is ClassInstance instance) {
            properties = instance.Properties;
        }
        else {
            // Constant
            properties = new Dictionary<string, (string, Value)> {
                {"value", (ar.ObjectType, ar)}
            };
        }
        
        Dictionary<string, RealReference> realReferenceList = new();
        foreach (KeyValuePair<string, (string, Value)> kvp in properties) {
            RealReference rr = Interpreter.ResolveValue(kvp.Value.Item2);
            realReferenceList.Add(kvp.Key, rr);
        }

        string json = SerializeRealReferenceDict(realReferenceList);
        return new Constant(json, "string");
    }
    
    public static string SerializeRealReferenceDict(Dictionary<string, RealReference> realReferenceDict) {
        JObject json = new();

        foreach ((string? name, RealReference? value) in realReferenceDict) {
            JToken? valueToken;
            if (value is Constant constVal) {
                switch (constVal.ObjectType) {
                    case "bool":  // True or false
                        valueToken = bool.Parse(constVal.Value);
                        break;
                    
                    case "string":
                        valueToken = constVal.Value;
                        break;
                    
                    case "null":
                        valueToken = null;
                        break;
                    
                    case "int":
                        valueToken = int.Parse(constVal.Value);
                        break;
                    
                    case "float":
                        valueToken = double.Parse(constVal.Value, CultureInfo.InvariantCulture);
                        break;
                    
                    default:
                        throw Interpreter.Error($"Unknown type in RealReference dictionary. Type: {constVal.ObjectType}");
                }
            }
            else if (value is ClassInstance classInstance) {
                JObject subObj = new();
                foreach (KeyValuePair<string, (string, Value)> prop in classInstance.Properties) {
                    Value val = Interpreter.ResolveValue(prop.Value.Item2);
                    Constant subConstant = Constant.FromString(val.ToString()!);
                    // recursively call the method for type safety
                    Dictionary<string, RealReference> subDict = new() { { prop.Key, subConstant } };
                    string subJson = SerializeRealReferenceDict(subDict);
                    subObj.Add(prop.Key, JObject.Parse(subJson)[prop.Key]);
                }
                valueToken = subObj;
            }
            else {
                throw Interpreter.Error("Unknown type in RealReference dictionary.");
            }

            json.Add(name, valueToken);
        }
        return json.ToString(Formatting.Indented); // Pretty-printed
    }
    
    public static Value FromJson(Value[] args) {
        string json = Interpreter.ResolveValue(args[0]).ToString()!;
        string resultTypeName = (args[1] as Constant)!.Value;
        if (!Interpreter.CurrentScope.Classes.TryGetValue(resultTypeName, out ClassDefinition? resultType)) {
            throw Interpreter.Error("Unknown type in FromJson method: " + resultTypeName);
        }
        
        JObject jObject = JObject.Parse(json);
        Dictionary<string, (string, Value)> properties = new();
        foreach (KeyValuePair<string, JToken> kvp in jObject) {
            Value value = kvp.Value.Type switch {
                JTokenType.Boolean => new Constant(kvp.Value.Value<bool>().ToString(), "bool"),
                JTokenType.Integer => new Constant(kvp.Value.Value<int>().ToString(), "int"),
                JTokenType.Float => new Constant(kvp.Value.Value<double>().ToString(CultureInfo.InvariantCulture), "float"),
                JTokenType.String => new Constant(kvp.Value.Value<string>()!, "string"),
                JTokenType.Null => new Constant("null", "null"),
                JTokenType.Object => FromJson(new Value[] {
                    new Constant(kvp.Value.ToString(), "string"),
                    new Constant(resultType.Variables.Single(i => i.Name == kvp.Key).VariableType, "string")  // Parse this object to the type of the corresponding field
                }),
                _ => throw Interpreter.Error("Unknown type in JSON object.")
            };
            properties.Add(kvp.Key, (value.ObjectType, value));
        }

        ClassInstance instance = new(resultType, properties);
        return instance;
    }
    
}