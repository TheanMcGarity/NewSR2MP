using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewSR2MP.Networking.Data
{
    public class NetException : Exception
    {
        /// <summary>
		/// NetException constructor
		/// </summary>
		public NetException()
            : base()
        {
        }

        /// <summary>
        /// NetException constructor
        /// </summary>
        public NetException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// NetException constructor
        /// </summary>
        public NetException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Throws an exception, in DEBUG only, if first parameter is false
        /// </summary>
        [Conditional("DEBUG")]
        public static void Assert(bool isOk, string message)
        {
            if (!isOk)
                throw new NetException(message);
        }

        /// <summary>
        /// Throws an exception, in DEBUG only, if first parameter is false
        /// </summary>
        [Conditional("DEBUG")]
        public static void Assert(bool isOk)
        {
            if (!isOk)
                throw new NetException();
        }
    }
}
