using BSOA.Collections;
using BSOA.Model;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class ResultList : TypedList<Result>
    {
        internal ResultList(Run run, ResultTable table, NumberList<int> indices) : base(indices, (index) => new Result(table, index) { Run = run }, (result) => table.LocalIndex(result))
        { }

        internal static ResultList Get(Run run, ResultTable table, IColumn<NumberList<int>> column, int index)
        {
            NumberList<int> indices = column[index];
            return (indices == null ? null : new ResultList(run, table, indices));
        }
    }
}
