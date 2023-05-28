using System.Collections.Generic;

namespace SignaMath.Classes
{
    internal class ColumnElement
    {
        public ColumnElement(string expression, double value, List<int> fromRows)
        {
            Expression = expression;
            Value = value;
            FromRows = fromRows;
            ValeurInterdite = false;
            ColumnSign = '+';
        }

        internal string Expression { get; set; }
        internal double Value { get; set; }
        internal char ColumnSign { get; set; }
        internal bool ValeurInterdite { get; set; }
        internal List<int> FromRows { get; set; }
    }
}