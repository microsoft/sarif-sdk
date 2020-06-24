using BSOA.Collections;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class ResultList : TypedList<Result>
    {
        private Run Run { get; }

        internal ResultList(Run run, ResultTable table, NumberList<int> indices) : base(indices, (index) => new Result(table, index) { Run = run }, (result) => table.LocalIndex(result))
        {
            Run = run;
        }

        internal ResultList(ExternalProperties externalProperties, ResultTable table, NumberList<int> indices) : base(indices, (index) => new Result(table, index), (result) => table.LocalIndex(result))
        {

        }
    }
}
