using System.Collections.Generic;

namespace SignaMath.Classes
{
    internal class ColumnElement
    {
        public ColumnElement(string expression, double value, byte position, List<int> fromRows)
        {
            Expression = expression;
            Value = value;
            Position = position;
            FromRows = fromRows;
            ValeurInterdite = false;
            ColumnSign = '+';
        }

        internal string Expression { get; set; }
        internal double Value { get; set; }
        internal char ColumnSign { get; set; }
        internal bool ValeurInterdite { get; set; }
        internal byte Position { get; set; }
        internal List<int> FromRows { get; set; }

        internal static char LastColumnSign = '+';
        internal static double IntervalleMin = double.MinValue;
        internal static double IntervalleMax = double.MaxValue;
    }
}