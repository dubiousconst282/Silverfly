namespace Silverfly.Testing;

public record TestOptions(bool UseStatementsAtToplevel, string Filename = "syntetic.dsl")
{
}