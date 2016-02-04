using System.Collections.Generic;

namespace Othello.Model
{
    public struct CellPosition
    {
        public int Row { get; set; }
        public int Column { get; set; }

        public CellPosition(int row, int column) : this()
        {
            Row = row;
            Column = column;
        }

        public override string ToString()
            =>((char)('a' + Column)).ToString() + (Row + 1);

        public static CellPosition Parse(string s)
        {
            var charArray = s.ToCharArray();
            var columnChar = charArray[0];
            var rowChar = charArray[1];
            var c = columnChar - 'a';
            var r = rowChar - '1';

            return new CellPosition(r, c);
        }

        public static IEnumerable<CellPosition> ParseList(string s)
        {
            foreach (var data in SplitRecords(s))
            {
                yield return Parse(data);
            }
        }

        public static IEnumerable<string> SplitRecords(string s)
        {
            var charArray = s.ToCharArray();
            for (var i = 0; i < charArray.Length; i += 2)
            {
                yield return (charArray[i] + "" + charArray[i + 1]);
            }
        }
    }
}