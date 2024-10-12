using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Silverfly.Generator.Definition;
using Silverfly.Nodes;

namespace Silverfly.Generator;

[Generator]
public class ParseletSourceGenerator : IIncrementalGenerator
{
    private const string Namespace = "Silverfly.Generator";
    private const string ParseletAttributeName = "ParseletAttribute";

    private const string ParseletAttributeSourceCode = $$"""
                                                         // <auto-generated/>

                                                         namespace {{Namespace}};

                                                         [System.AttributeUsage(System.AttributeTargets.Class)]
                                                         internal class {{ParseletAttributeName}}(string definition, Type nodeType) : System.Attribute
                                                         {
                                                             public string Definition { get; } = definition;
                                                             public Type NodeType { get; } = nodeType;
                                                         }


                                                         """;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Add the marker attribute to the compilation.
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "ParseletAttribute.g.cs",
            SourceText.From(ParseletAttributeSourceCode, Encoding.UTF8)));

        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                (s, _) => s is ClassDeclarationSyntax,
                (ctx, _) => GetClassDeclarationForSourceGen(ctx))
            .Where(t => t.reportAttributeFound)
            .Select((t, _) => t.Item1);

        // Generate the source code.
        context.RegisterSourceOutput(context.CompilationProvider.Combine(provider.Collect()),
            ((ctx, t) => GenerateCode(ctx, t.Left, t.Right)));
    }
    
    private static (ClassDeclarationSyntax, bool reportAttributeFound) GetClassDeclarationForSourceGen(
        GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

        // Go through all attributes of the class.
        foreach (var attributeSyntax in classDeclarationSyntax.AttributeLists.SelectMany(attributeListSyntax => attributeListSyntax.Attributes))
        {
            if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                continue; // if we can't get the symbol, ignore it

            var attributeName = attributeSymbol.ContainingType.ToDisplayString();

            if (attributeName == $"{Namespace}.{ParseletAttributeName}")
                return (classDeclarationSyntax, true);
        }

        return (classDeclarationSyntax, false);
    }
    
    private void GenerateCode(SourceProductionContext context, Compilation compilation,
        ImmutableArray<ClassDeclarationSyntax> classDeclarations)
    {
        // Go through all filtered class declarations.
        foreach (var classDeclarationSyntax in classDeclarations)
        {
            // We need to get semantic model of the class to retrieve metadata.
            var semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);

            // Symbols allow us to get the compile-time information.
            if (semanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not INamedTypeSymbol classSymbol)
                continue;

            var attribute = classSymbol.GetAttributes()
                .FirstOrDefault(attr => attr.AttributeClass?.Name == ParseletAttributeName);

            if (attribute == null)
            {
                continue;
            }


            // Get the property value from the attribute (replace "PropertyName" with the actual property name)
            var definition = attribute.ConstructorArguments[0].Value;
            var nodeType = (INamedTypeSymbol)attribute.ConstructorArguments[1].Value!;

            var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();

            // 'Identifier' means the token of the node. Get class name from the syntax node.
            var className = classDeclarationSyntax.Identifier.Text;
            var filename = $"{className}.g.cs";

            var parser = new DefinitionGrammar();
            var parsed = parser.Parse((string)definition!, filename);

            var method = GenerateParseMethod(parsed.Tree, classSymbol, nodeType);

            // Build up the source code
            var code = $$"""
                         // <auto-generated/>

                         using System;
                         using System.Collections.Generic;
                         using Silverfly;
                         using Silverfly.Nodes;

                         namespace {{namespaceName}};

                         //{{definition}}
                         public partial class {{className}} {
                         
                            {{method}}
                         }

                         """;

            context.AddSource(filename, SourceText.From(code, Encoding.UTF8));
        }
    }

    private string GenerateParseMethod(AstNode definition, INamedTypeSymbol classSymbol, INamedTypeSymbol nodeType)
    {
        var builder = new StringBuilder();

        var prefixType = Utils.ImplementsInterface(classSymbol, "IPrefixParselet");
        if (prefixType)
        {
            GeneratePrefixParse(builder, definition, nodeType);
        }

        var infixType = Utils.ImplementsInterface(classSymbol, "IInfixParselet");
        if (infixType)
        {
            GenerateInfixParse(builder, definition, nodeType);
        }

        return builder.ToString();
    }

    private void GeneratePrefixParse(StringBuilder builder, AstNode definition, INamedTypeSymbol nodeType)
    {
        var generator = new GeneratorVisitor(builder);

        builder.AppendLine("\tpublic AstNode Parse(Parser parser, Token token) {");

        foreach (var child in ((BlockNode)definition).Children)
        {
            child.Accept(generator);
        }

        builder.AppendLine($"\n\t\treturn new {nodeType}({GenerateCtorCall(generator)})\n\t\t\t.WithRange(token, parser.LookAhead(0));");
        //builder.Append("return null;");
        builder.AppendLine("\t}");
    }

    private string GenerateCtorCall(GeneratorVisitor generator)
    {
        //name: ref
        return string.Join(',', generator.Names.Select(n => $"{n}: _{n.ToLower()}"));
    }

    private void GenerateInfixParse(StringBuilder builder, AstNode definition, INamedTypeSymbol nodeType)
    {
        builder.Append("\tAstNode Parse(Parser parser, AstNode left, Token token) {");
        builder.Append("\t\treturn {};");
        builder.Append('}');
    }
}
