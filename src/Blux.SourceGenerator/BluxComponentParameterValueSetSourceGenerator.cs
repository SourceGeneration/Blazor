﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Blux.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public partial class BluxComponentParameterValueSetSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var declarations = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, token) =>
            {
                if (node is not ClassDeclarationSyntax classDeclaration
                    || !classDeclaration.IsPartial()
                    || classDeclaration.IsNested()
                    || classDeclaration.IsAbstract()
                    || classDeclaration.IsStatic()
                    || classDeclaration.TypeParameterList != null)
                    return false;

                return classDeclaration.IsDerivedFrom("BluxComponentBase", "BluxComponent");
            },
            transform: static (context, token) =>
            {
                return (ClassDeclarationSyntax)context.Node;
            });

        var source = declarations.Combine(context.CompilationProvider);

        context.RegisterSourceOutput(source, static (sourceContext, source) =>
        {
            CancellationToken cancellationToken = sourceContext.CancellationToken;
            var classDeclaration = source.Left;
            var compilation = source.Right;
            var root = (CompilationUnitSyntax)classDeclaration.SyntaxTree.GetRoot();

            var parameters = AnalysisParameters(classDeclaration, compilation, cancellationToken);
            if (parameters.Count == 0)
                return;

            var ns = classDeclaration.GetNamespace();
            CSharpCodeBuilder builder = new();

            builder.AppendAutoGeneratedComment();
            builder.AppendBlock($"namespace {ns}", () =>
            {
                builder.AppendBlock($"partial class {classDeclaration.Identifier.Text}", () =>
                {
                    builder.AppendBlock("protected override void SetParametersValue(global::Microsoft.AspNetCore.Components.ParameterView parameters)", () =>
                    {
                        builder.AppendBlock("foreach (var parameter in parameters)", () =>
                        {
                            builder.AppendBlock("switch (parameter.Name)", () =>
                            {
                                foreach (var parameter in parameters)
                                {
                                    builder.AppendLine($"case \"{parameter.Key}\":");
                                    var propertyName = parameter.Key;
                                    if (char.IsUpper(propertyName[0]))
                                    {
                                        propertyName = char.ToLower(propertyName[0]) + propertyName.Substring(1);
                                        builder.AppendLine($"case \"{propertyName}\":");
                                    }

                                    builder.AppendBlock(() =>
                                    {
                                        builder.AppendLine($"this.{parameter.Key} = ({parameter.Value})parameter.Value;");
                                        builder.AppendLine("break;");
                                    });
                                }
                            });
                        });
                    });
                });
            });

            var code = builder.ToString();
            sourceContext.AddSource($"{ns}.{classDeclaration.Identifier.Text}.SetParametersValue.g.cs", code);
        });
    }

    static Dictionary<string, string> AnalysisParameters(ClassDeclarationSyntax classDeclarationSyntax, Compilation compilation, CancellationToken cancellationToken)
    {
        Dictionary<string, string> parameters = [];

        if (classDeclarationSyntax.HasMethod("SetParametersAsync"))
        {
            return parameters;
        }

        foreach (var property in classDeclarationSyntax.Members.OfType<PropertyDeclarationSyntax>())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (property.HasAttribute("Parameter") ||
                property.HasAttribute("CascadingParameter") ||
                property.HasAttribute("SupplyParameterFromQuery") ||
                property.HasAttribute("SupplyParameterFromForm"))
            {
                parameters.Add(property.Identifier.Text, property.Type.GetTypeName());
            }
        }
        return parameters;
    }
}