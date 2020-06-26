using BSOA.Demo.SoA;
using BSOA.Model;

namespace BSOA.Demo.Examples.SoA
{
    public struct Result
    {
        internal readonly ResultTable _table;
        internal readonly int _index;

        public string RuleId
        {
            get => _table.RuleId[_index];
            set => _table.RuleId[_index] = value;
        }

        public Message Message
        {
            get => new Message(_table.DB.Message, _table.Message[_index]);
            set => _table.Message[_index] = _table.DB.Message.ToLocalIndex(value);
        }
        public string Guid
        {
            get => _table.Guid[_index];
            set => _table.Guid[_index] = value;
        }
    }

    internal class ResultTable
    {
        internal SarifLogDatabase DB { get; }
        public IColumn<string> RuleId { get; }
        public IColumn<int> Message { get; }
        public IColumn<string> Guid { get; }
    }

    // IColumn<int>.this[index]
    // {
    //     get => (_array?.Length > index ? _array[index] : _default);
    //     set { ... }
    // }
}
