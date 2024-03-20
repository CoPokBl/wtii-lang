using System.Runtime.InteropServices;

namespace WhatTimeIsIt;

public static class CppInterpreter {

    [DllImport("libwtiiint.so")]
    public static extern int interpret(string filename);

}