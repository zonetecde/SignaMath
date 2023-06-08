using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaMath.Classes
{
    internal class ExpressionElement
    {
        public ExpressionElement(bool interdite, string expression)
        {
            Interdite = interdite;
            Expression = expression;
        }

        public ExpressionElement(bool isNumerator, string expression, string exposant)
        {
            IsNumerator = isNumerator;
            Exposant = exposant;
            Expression = expression;
        }

        public bool IsNumerator { get; }
        public string Exposant { get; }

        internal bool Interdite { get; set; }
        internal string Expression { get; set; }
    }
}
