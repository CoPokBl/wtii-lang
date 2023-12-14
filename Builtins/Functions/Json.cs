using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WhatTimeIsIt.ParsedScripts.Values;

namespace WhatTimeIsIt.Builtins.Functions;

public static class Json {
    
    public static Value ToJson(Value[] args) {
        RealReference ar = Interpreter.ResolveValue(args[0]);
        Dictionary<string, Value> properties;
        if (ar is ClassInstance instance) {
            properties = instance.Properties;
        }
        else {
            // Constant
            properties = new Dictionary<string, Value> {
                {"value", ar}
            };
        }
        
        Dictionary<string, RealReference> realReferenceList = new();
        foreach (KeyValuePair<string, Value> kvp in properties) {
            RealReference rr = Interpreter.ResolveValue(kvp.Value);
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
                switch (constVal.Value) {
                    case "true":
                    case "false":
                        valueToken = bool.Parse(constVal.Value);
                        break;
                    case "null":
                        valueToken = null;
                        break;
                    default:
                        if (double.TryParse(constVal.Value, out double numVal))
                            valueToken = numVal;
                        else
                            valueToken = constVal.Value;
                        break;
                }
            }
            else if (value is ClassInstance classInstance) {
                JObject subObj = new();
                foreach (KeyValuePair<string, Value> prop in classInstance.Properties) {
                    Value val = Interpreter.ResolveValue(prop.Value);
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
    
}