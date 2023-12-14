namespace WhatTimeIsIt; 

public class ProgramExitException : Exception {
    
    public int ExitCode;
    
    public ProgramExitException(int exitCode) {
        ExitCode = exitCode;
    }
    
}