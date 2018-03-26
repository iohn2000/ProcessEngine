using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

namespace Kapsch.IS.EMD.EMD20Web.HelperExtensions
{
    /// <summary>
    /// Use this class to enhance the HTML class with method LabelForRequired
    /// LabelForRequired adds a new TAG span with class "label-required" inside the Label-TAG
    /// </summary>
    public static class LabelExtensions
    {
        public static MvcHtmlString LabelForRequired<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, object htmlAttributes = null)
        {
            return LabelForRequired(html, expression, new RouteValueDictionary(htmlAttributes));
        }

        public static MvcHtmlString LabelForRequired<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, IDictionary<string, object> htmlAttributes)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            string htmlFieldName = ExpressionHelper.GetExpressionText(expression);
            String labelText = metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();
            if (String.IsNullOrEmpty(labelText.ToString()))
            {
                return MvcHtmlString.Empty;
            }

            TagBuilder labelTag = new TagBuilder("label");
            labelTag.MergeAttributes(htmlAttributes);
            labelTag.Attributes.Add("for", html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(htmlFieldName));

            TagBuilder spanTag = new TagBuilder("span");
            spanTag.SetInnerText(labelText.ToString());

            StringBuilder labelString = new StringBuilder();

            if (metadata.IsRequired)
            {
                TagBuilder requiredSpanTag = new TagBuilder("span");
                requiredSpanTag.AddCssClass("label-required");
                requiredSpanTag.SetInnerText("*");

                labelTag.InnerHtml = string.Format("{0} {1}", spanTag.ToString(TagRenderMode.Normal), requiredSpanTag.ToString(TagRenderMode.Normal));
            }
            else
            {
                labelTag.InnerHtml = spanTag.ToString(TagRenderMode.Normal);
            }

            return MvcHtmlString.Create(labelTag.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString SpanFor<TModel, TProperty>(this HtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, object htmlAttributes = null)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);

            var valueGetter = expression.Compile();
            var value = valueGetter(html.ViewData.Model);

            var span = new TagBuilder("span");
            span.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            if (value != null)
            {
                span.SetInnerText(metadata.DisplayName);
            }

            return MvcHtmlString.Create(span.ToString());
        }
    }
}