using Microsoft.CodeAnalysis.Sarif;

namespace BSOA.Demo.Examples.Normal
{
    public class Result
    {
        public string RuleId { get; set; }
        public Message Message { get; set; }
        public string Guid { get; set; }
    }
}
