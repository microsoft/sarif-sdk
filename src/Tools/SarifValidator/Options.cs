using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.SarifValidator
{
    internal class Options
    {
        [Option(
            's',
            "schema-file-path",
            HelpText = "Path of the SARIF JSON schema file",
            Default = "Sarif.schema.json")]
        public string SchemaFilePath { get; set; }

        [Option(
            'i',
            "instance-file-path",
            HelpText = "Path of the SARIF file to validate",
            Required = true)]
        public string InstanceFilePath { get; set; }
    }
}
