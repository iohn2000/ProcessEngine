using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Utils
{
    /// <summary>
    /// Represents a GUID as used in EMD consisting of an 4 uppercase letter prefix and 32 hex-digits which are separated by an underline in the string-representation.
    /// It also provides implicit casts from/to string.
    /// </summary>
    public class EMDGuid
    {
        public static readonly Regex PATTERN = new Regex("^[A-Z]{4}_[0-9a-f]{32}$");

        public string Prefix { get; }
        public string Guid { get; }

        /// <summary>
        /// Constructs a new EMDGuid from the given string. 
        /// </summary>
        /// <param name="guid">String-representation of the GUID needs to match <see cref="EMDGuid.PATTERN"/> or an exception is thrown.</param>
        /// <exception cref="GuidCastException">Is thrown if the given string doesn't match the pattern of an EMDGuid as provided in <see cref="PATTERN"/>.</exception>
        public EMDGuid(string guid)
        {
            if (!PATTERN.IsMatch(guid))
            {
                throw new GuidCastException(ErrorCodeHandler.E_EDP_ENTITY, guid);
            }
            Prefix = guid.Substring(0, 4);
            //4 is underline
            Guid = guid.Substring(5);
        }

        /// <summary>
        /// Converts this EMDGuid into a string-representation where PREFIX_GUID;
        /// </summary>
        /// <returns>String representing this GUID.</returns>
        public override string ToString()
        {
            return Prefix + "_" + Guid;
        }

        /// <summary>
        /// Implicit converting a EMDGuid to a string by calling <see cref="EMDGuid.ToString"/>
        /// </summary>
        /// <param name="guid">String representing this GUID.</param>
        public static implicit operator string(EMDGuid guid)
        {
            return guid.ToString();
        }

        /// <summary>
        /// Implicit converting an string to a EMDGuid via Constructor (<see cref="EMDGuid"/>)
        /// </summary>
        /// <param name="guid">String representing a GUID. Needs to match <see cref="PATTERN"/>.</param>
        public static implicit operator EMDGuid(string guid)
        {
            return new EMDGuid(guid);
        }
    }
}
