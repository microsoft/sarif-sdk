namespace Microsoft.CodeAnalysis.Sarif.SarifValidator
{
    public enum JSONErrorKind
    {
        Syntax,
        Validation
    }

    public enum JSONErrorLocation
    {
        InstanceDocument,
        Schema
    }

    public class JSONError
    {
        public JSONErrorLocation Location { get; set; }

        public JSONErrorKind Kind { get; set; }

        public string Message { get; set; }

        public int Start { get; set; }

        public int Length { get; set; }

        public override string ToString()
        {
            return $"{Location}: {Kind}: ({Start}, {Length}): {Message}";
        }
    }
}
