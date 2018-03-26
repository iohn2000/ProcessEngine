using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Kapsch.IS.ProcessEngine
{
    #region Documentation -----------------------------------------------------------------------------
    /// <summary>  represents a variable defined in the workflow  </summary>
    ///
    /// <remarks>   Fleckj, 10.02.2015. </remarks>
    #endregion



    public class Variable
    {
        private IEDPLogger logger = EDPLogger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public string Name { get; set; }
        public string VarValue { get; set; } // string, integer, datetime
        public EnumVariablesDataType DataType { get; set; }
        public EnumVariableDirection Direction { get; set; }

        public Variable(string name, string varValue, EnumVariablesDataType dataType, EnumVariableDirection direction)
        {
            this.Name = name;
            this.DataType = dataType;
            this.VarValue = varValue;
            this.Direction = direction;
        }

        public Variable()
        {
        }

        public string GetStringValue()
        {
            return this.VarValue;
        }
        public double GetDoubleValue()
        {
            double d;
            if (double.TryParse(this.VarValue, out d))
                return d;
            else
                return double.MinValue;
        }
        /// <summary>
        /// return int value or throws BaseException (already logged error)
        /// </summary>
        /// <returns></returns>
        public int GetIntValue()
        {
            int d;

            try
            {
                d = int.Parse(this.VarValue);
                return d;
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("Error trying to parse string to int. Variable Name='{0}' Value='{1}'", this.Name, this.VarValue);
                logger.Error(errMsg, ex);
                throw new BaseException(ErrorCodeHandler.E_WF_PROCESSENGINE, errMsg, ex);
            }
        }
        /// <summary>
        /// converts to datetime, throws exception
        /// </summary>
        /// <returns></returns>
        public DateTime? GetDateValue()
        {
            try
            {
                DateTime datum = DataHelper.Iso8601ToDateTime(this.VarValue);
                return datum;
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("Error trying to parse string to date. Variable Name='{0}' Value='{1}'", this.Name, this.VarValue);
                logger.Error(errMsg, ex);
                throw new BaseException(ErrorCodeHandler.E_WF_PROCESSENGINE, errMsg, ex);
            }
        }

        public bool? GetBooleanValue()
        {
            bool d;
            if (bool.TryParse(this.VarValue, out d))
                return d;
            else
                return null;
        }

        /// <summary>
        /// convert string to EnumVariableDataType
        /// </summary>
        /// <param name="dType"></param>
        /// <returns></returns>
        public static EnumVariablesDataType ConvertToEnumDataType(string dType)
        {
            EnumVariablesDataType parsedType;
            if (Enum.TryParse(dType, out parsedType))
            {
                if (Enum.IsDefined(typeof(EnumVariablesDataType), parsedType))
                {
                    return parsedType;
                }
            }

            return EnumVariablesDataType.stringType;
        }

        /// <summary>
        /// convert string to EnumVariableDirection
        /// </summary>
        /// <param name="dType"></param>
        /// <returns></returns>
        public static EnumVariableDirection ConvertToEnumDirectionType(string dType)
        {
            EnumVariableDirection parsedType;
            if (Enum.TryParse(dType, out parsedType))
            {
                if (Enum.IsDefined(typeof(EnumVariableDirection), parsedType))
                {
                    return parsedType;
                }
            }

            return EnumVariableDirection.both;
        }

        /// <summary>
        /// converts a string into an enum for enums in workflow variables
        /// (enumVariablesDataType, enumVariableDirection)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ConvertToEnum<T>() where T : struct
        {
            T returnEnum = default(T); // initialise with default enum value

            if (Enum.TryParse<T>(this.VarValue, out returnEnum))
            {
                if (Enum.IsDefined(typeof(T), returnEnum))
                {
                    return returnEnum;
                }
            }
            else
            {
                string msg = string.Format("cannot convert '{0}' into enum value. using default enum value (='{0}')", this.VarValue, returnEnum.ToString());
                logger.Warn(msg);
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, msg);
            }
            return returnEnum;
        }

        /// <summary>
        /// Creates Variables from activity properties
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public static List<Variable> GetVariablesFromActivity(Activity activity)
        {
            List<Variable> variables = new List<Variable>();


            foreach (ActivityProperty activityProperty in activity.GetAllActivityProperties)
            {
                variables.Add(new Variable(activityProperty.Name, null, activityProperty.DataType, activityProperty.Direction));
            }

            return variables;
        }

        public XElement GetInstanceNode(int instanceNumber)
        {

            return new XElement("variable",
                new XAttribute("direction", this.Direction.ToString()),
                new XAttribute("dataType", this.DataType.ToString()),
                new XAttribute("name", string.Format("{0}.{1}", instanceNumber, this.Name.ToString()))
                );

        }
    }
}
