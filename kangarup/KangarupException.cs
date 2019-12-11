using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace kangarup
{
    [Serializable]
    public class KangarupException : Exception
    {

        public KangarupException()
        {
        }

        public KangarupException(string message) : base(message)
        {
        }

        public KangarupException(string message, Exception inner) : base(message, inner)
        {
        }

        protected KangarupException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
