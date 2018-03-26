using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.EDP.Core.Utils;

namespace Kapsch.IS.EDP.Core.Framework
{
    public class EntityQuery
    {
        private EntityPrefix entityPrefix;

        public EntityQuery()
        {
            entityPrefix = EntityPrefix.Instance;
        }

        /// <summary>
        /// makes it possible to calculate all @@ queries in string that contains also other stuff
        /// e.g.: 'Roberts email lautet MainMail@@PERS_adfkjasdkdf und hat eine ZiNr Text@@SearchContactItemForEmployment(COTY_26a1bebc72004b82a87338a67e2e45f7)@@EMPL_afdax3sfd'
        /// </summary>
        /// <param name="queryString"></param>
        /// <param name="propType"></param>
        /// <returns></returns>
        public object QueryMixedString(string expression, out Type propType)
        {
            propType = new string(' ', 1).GetType();

            if (expression.IndexOf("@@") == -1)
                return expression;

            do
            {
                string atat = this.extractAtAtExpression(expression);
                string evaluatedAtAt = this.Query(atat, out propType).ToString(); //"samefake" + DateTime.Now.Ticks.ToString(); //

                //replace @@expression with evaluated value
                expression = expression.Replace(atat, evaluatedAtAt);

                if (expression.IndexOf("@@") == -1)
                    break;
            }
            while (true);

            return expression;
        }

        private string extractAtAtExpression(string expression)
        {
            // "Your E-Mail is : " + "MainMail@@P_Guid@@SearchRespRoleForEmpl(10500)@@EMPL_1212" + "and this is cool. Name = " + "FamilyName@@P_Guid@@EMPL_1212"

            if (expression.IndexOf("@@") == -1)
                return expression;

            int atatStart = 0;

            //find first @@
            atatStart = expression.IndexOf("@@", 0);
            // work towards start of string until first space
            int separatorBefore = expression.Substring(0, atatStart).LastIndexOf("\"");

            //find next space from atatStart --> the end of expression
            int separatorAfter = expression.IndexOf("\"", atatStart);
            if (separatorAfter == -1)
                separatorAfter = expression.Length;

            string atatExpression = expression.Substring(separatorBefore + 1,
                                        separatorAfter - separatorBefore - 1);

            return atatExpression;

        }

        public object Query(string queryString, out Type propType)
        {
            String property;
            String entity;

            if (queryString.IndexOf("@@") == -1)
            {
                propType = "".GetType();
                return StringHelper.ReplaceNewlines(queryString);
            }

            property = queryString.Substring(0, queryString.IndexOf("@@"));
            entity = queryString.Substring(property.Length + 2);

            if (entity.Contains("@@"))
            {
                //result must be a guid, therfore string
                Type innerPropType;
                //recursive call
                String innerresult = this.Query(entity, out innerPropType).ToString();


                if (string.IsNullOrWhiteSpace(innerresult))
                {
                    //    throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "EntityQuery did not return a value");
                    innerresult = "";
                }
                else if (!entityPrefix.isPrefix(innerresult))
                    throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "EntityQuery did not contain valid Guid");

                entity = innerresult;
            }

            if (entity != "")
            {
                if (property.Contains("("))
                {
                    if (string.IsNullOrWhiteSpace(entity))
                        throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "EntityQuery did not return a value");

                    //parse property for functioname and paramvalue (which is always handled as a string);
                    String function = queryString.Substring(0, queryString.IndexOf("("));
                    String param = queryString.Substring(function.Length + 1);
                    param = param.Substring(0, param.IndexOf(")"));
                    propType = new String(' ', 1).GetType();

                    return this.RemoveNewLineFromObject(entityPrefix.GetGuidByFunctionCall(function, param, entity));
                }
                else
                {
                    return this.RemoveNewLineFromObject(entityPrefix.GetPropertyByNameandGuid(entity, property, out propType));
                }
            }
            else
            {
                propType = "".GetType();
                return StringHelper.ReplaceNewlines(entity);
            }

        }

        private object RemoveNewLineFromObject(object obj)
        {
            if (obj.GetType() == typeof(String))
            {
                return StringHelper.ReplaceNewlines(obj.ToString());
            }
            else
                return obj;

        }

    }
}
