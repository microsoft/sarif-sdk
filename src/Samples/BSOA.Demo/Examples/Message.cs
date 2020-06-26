using BSOA.Demo.SoA;

namespace BSOA.Demo.Examples.SoA
{
    internal class MessageTable
    {
        internal SarifLogDatabase Database { get; }

        public string[] Text { get; set; }

        public int ToLocalIndex(Message message)
        {
            if (message._table == this)
            {
                return message._index;
            }
            else
            {
                // Copy Message here
                return message.Text.Length + 1;
            }
        }
    }

    public class Message
    {
        internal readonly MessageTable _table;
        internal readonly int _index;

        internal Message(MessageTable table, int index)
        {
            _table = table;
            _index = index;
        }

        public string Text
        {
            get => _table.Text[_index];
            set => _table.Text[_index] = value;
        } 
    }
}
