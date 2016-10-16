// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   http://www.isthe.com/chongo/tech/comp/fnv/index.html
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Text;

namespace HttpGatewayWebApi
{
    /// <summary>
    /// Hash evaluator used for partitioning (<see cref="http://www.isthe.com/chongo/tech/comp/fnv/index.html"/>).
    /// </summary>
    public static class Fnv1AHashCode
    {
        /// <summary>
        /// Gets 64 bit hash code for the specified byte array.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The computed hash value.
        /// </returns>
        /// <exception cref="ArgumentNullException">The input <paramref name="value"/> is null.</exception>
        public static long Get64BitHashCode(byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value.Length == 0)
            {
                return 0;
            }

            const ulong OffsetBasis = 14695981039346656037;
            const ulong Prime = 1099511628211;

            ulong hashCode = OffsetBasis;

            for (int index = 0; index < value.Length; index++)
            {
                hashCode ^= value[index];
                hashCode *= Prime;
            }

            return (long)hashCode;
        }

        /// <summary>
        /// Gets the 64 bit hash code for the specified string.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="encoding">
        /// The optional encoding. If not specified, UTF8 will be used.
        /// </param>
        /// <returns>
        /// The computed hash value.
        /// </returns>
        /// <exception cref="ArgumentNullException">The input <paramref name="value"/> is null.</exception>
        public static long Get64BitHashCode(string value, Encoding encoding = null)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return Get64BitHashCode((encoding ?? Encoding.UTF8).GetBytes(value));
        }
    }
}