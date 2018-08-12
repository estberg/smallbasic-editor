﻿// <copyright file="GenerateDiagnostics.cs" company="2018 Omar Tawfik">
// Copyright (c) 2018 Omar Tawfik. All rights reserved. Licensed under the MIT License. See LICENSE file in the project root for license information.
// </copyright>

namespace SuperBasic.Generators
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;
    using SuperBasic.Utilities;

    public sealed class GenerateDiagnostics : BaseGeneratorTask
    {
        protected override void ExecuteConversion()
        {
            this.WriteFile<DiagnosticEntryCollection>(
                inputFilePath: Path.Combine(this.RootDirectory, "Source", "SuperBasic.Generators", "GenerateDiagnostics", "Input.xml"),
                outputFilePath: Path.Combine(this.RootDirectory, "Source", "SuperBasic.Compiler", "Diagnostics", "DiagnosticCode.Generated.cs"),
                converter: root =>
$@"// <copyright file=""DiagnosticCode.Generated.cs"" company=""2018 Omar Tawfik"">
// Copyright (c) 2018 Omar Tawfik. All rights reserved. Licensed under the MIT License. See LICENSE file in the project root for license information.
// </copyright>

/// <summary>
/// This file is auto-generated by a build task. It shouldn't be edited by hand.
/// </summary>
namespace SuperBasic.Compiler.Diagnostics
{{
    using SuperBasic.Utilities;
    using SuperBasic.Utilities.Resources;

    internal enum DiagnosticCode
    {{
{root.Select(entry => $"        {entry.Name},").Join(Environment.NewLine)}
    }}

    internal static partial class DiagnosticCodeExtensions
    {{
        public static string ToDisplayString(this DiagnosticCode kind)
        {{
            switch (kind)
            {{
{root.Select(entry => $"                case DiagnosticCode.{entry.Name}: return DiagnosticsResources.{entry.Name};").Join(Environment.NewLine)}
                default: throw ExceptionUtilities.UnexpectedValue(kind);
            }}
        }}
    }}
}}
");

            this.WriteFile<DiagnosticEntryCollection>(
                inputFilePath: Path.Combine(this.RootDirectory, "Source", "SuperBasic.Generators", "GenerateDiagnostics", "Input.xml"),
                outputFilePath: Path.Combine(this.RootDirectory, "Source", "SuperBasic.Compiler", "Diagnostics", "Diagnostic.Generated.cs"),
                converter: root =>
$@"// <copyright file=""Diagnostic.Generated.cs"" company=""2018 Omar Tawfik"">
// Copyright (c) 2018 Omar Tawfik. All rights reserved. Licensed under the MIT License. See LICENSE file in the project root for license information.
// </copyright>

/// <summary>
/// This file is auto-generated by a build task. It shouldn't be edited by hand.
/// </summary>
namespace SuperBasic.Compiler.Diagnostics
{{
    using SuperBasic.Compiler.Syntax;
    using SuperBasic.Utilities;

    internal sealed class Diagnostic
    {{
        private string[] args;

        private Diagnostic(DiagnosticCode kind, TextRange range, params string[] args)
        {{
            this.Kind = kind;
            this.Range = range;
            this.args = args;
        }}

        public DiagnosticCode Kind {{ get; private set; }}

        public TextRange Range {{ get; private set; }}
{root.Select(entry => $@"
        public static Diagnostic {entry.Name}(TextRange range{entry.Parameters.Select(parameter => $", {parameter.Type} {parameter.Name}").Join(string.Empty)})
        {{
            return new Diagnostic(DiagnosticCode.{entry.Name}, range{entry.Parameters.Select(parameter => $", {parameter.Name}.ToDisplayString()").Join(string.Empty)});
        }}").Join(Environment.NewLine)}

        public string ToDisplayString()
        {{
            return string.Format(this.Kind.ToDisplayString(), this.args);
        }}
    }}
}}
");
        }

        public sealed class Parameter
        {
            [XmlAttribute]
            public string Name { get; set; }

            [XmlAttribute]
            public string Type { get; set; }
        }

        public sealed class DiagnosticEntry
        {
            [XmlAttribute]
            public string Name { get; set; }

            [XmlArray(nameof(Parameters))]
            [XmlArrayItem(typeof(Parameter))]
            public List<Parameter> Parameters { get; set; }
        }

        [XmlRoot("root")]
        public sealed class DiagnosticEntryCollection : List<DiagnosticEntry>
        {
        }
    }
}