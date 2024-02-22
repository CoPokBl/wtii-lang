namespace WhatTimeIsIt;

public interface IWtiiLibrary {
    
    /// <summary>
    /// Scope should be set either in the constructor or in the Init method.
    /// It should contain all the classes, functions, and variables that the library provides.
    /// </summary>
    public Scope Scope { get; set; }
    
    /// <summary>
    /// Initialise the library. DO NOT RUN ANY CODE IN THIS METHOD.
    /// THIS METHOD WILL BE RUN BY THE PARSER WHEN THE LIBRARY IS LOADED.
    /// DO NOT ACCESS Interpreter.
    /// </summary>
    public void Init();
    
    /// <summary>
    /// This is the entry point for the library. This is equivalent to top level statements in a script.
    /// You can run any code you want in this method, including accessing the interpreter.
    /// </summary>
    public void Run();
}