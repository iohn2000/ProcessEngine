using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System.Linq.Expressions;
using Kapsch.IS.EDP.Core.Entities;

//namespace Kapsch.IS.EMD.EMD20Web.Models
namespace System.Web.Mvc.Html
{
    public static class HtmlHelperExtensions
    {
        public static MvcHtmlString RadioButtonAndLabelFor<TModel, TProperty>(this HtmlHelper<TModel> self, Expression<Func<TModel, TProperty>> expression, bool value, string labelText)
        {
            // Retrieve the qualified model identifier
            string name = ExpressionHelper.GetExpressionText(expression);
            string fullName = self.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);

            // Generate the base ID
            TagBuilder tagBuilder = new TagBuilder("input");
            tagBuilder.GenerateId(fullName);
            string idAttr = tagBuilder.Attributes["id"];

            // Create an ID specific to the boolean direction
            idAttr = String.Format("{0}_{1}", idAttr, value);

            // Create the individual HTML elements, using the generated ID
            MvcHtmlString radioButton = self.RadioButtonFor(expression, value, new { id = idAttr });
            MvcHtmlString label = self.Label(idAttr, labelText);

            return new MvcHtmlString(radioButton.ToHtmlString() + label.ToHtmlString());
        }

        public static string ExtractUserName(string name)
        {
            string userName = name.Trim();

            string[] names = name.Split('\\');

            if (names.Length > 1)
            {
                userName = names[1].Trim();
            }

            return userName;
        }

        public static string GetParameterFromUrl(Uri uri, string parameter)
        {
            string parameterValue = HttpUtility.ParseQueryString(uri.Query).Get(parameter);

            if (string.IsNullOrWhiteSpace(parameterValue))
            {
                parameterValue = string.Empty;
            }

            return parameterValue;         
        }

        public static string GetDateString(DateTime dateTime)
        {
            string dateString = dateTime.ToShortDateString();
            if (dateTime == EMDObject<object>.INFINITY || dateTime == DateTime.MinValue || dateTime == new DateTime(2299, 12, 31) || dateTime == new DateTime(0001, 01, 01))
            {
                dateString = "Not set";
            }

            return dateString;
        }
    }
}