using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.HelperExtensions
{
    public static class LoggerExtension
    {
        public static void Debug(this IISLogger logger, object obj, HttpContextBase context)
        {
            StringBuilder message = new StringBuilder();
            message.AppendFormat("Message: {0};", obj);
            if (context != null)
            {
                message.AppendFormat(" Controller: '{0}';", (context?.Request?.RequestContext?.RouteData?.Values["controller"]?.ToString()) ?? "not set");
                message.AppendFormat(" Action: '{0}';", (context?.Request?.RequestContext?.RouteData?.Values["action"]?.ToString()) ?? "not set");
                message.AppendFormat(" User: '{0}';", (context?.User?.Identity?.Name) ?? "not set");
            }
            logger.Debug(message.ToString());
        }

        public static void Error(this IISLogger logger, Exception ex, HttpContextBase context = null)
        {
            logger.Error(ExceptionToText(ex, context));
        }

        public static void Error(this IISLogger logger, string text, Exception ex, HttpContextBase context)
        {
            logger.Error(string.Format("ErrorText: {0}; {1}", text, ExceptionToText(ex, context)));
        }

        private static string ExceptionToText(Exception ex, HttpContextBase context)
        {
            StringBuilder message = new StringBuilder();
            message.AppendFormat("Error: {0};", ex.GetType().FullName);
            if (ex is BaseException)
                message.AppendFormat(" ErrorCode: {0};", (ex as BaseException).ErrorCode);
            message.AppendFormat(" Message: '{0}';", ex.Message);
            if (context != null)
            {
                message.AppendFormat(" Controller: '{0}';", (context?.Request?.RequestContext?.RouteData?.Values["controller"]?.ToString()) ?? "not set");
                message.AppendFormat(" Action: '{0}';", (context?.Request?.RequestContext?.RouteData?.Values["action"]?.ToString()) ?? "not set");
                message.AppendFormat(" User: '{0}';", (context?.User?.Identity?.Name) ?? "not set");
            }
            message.AppendFormat(" StackTrace: {0};", ex.StackTrace);
            message.AppendFormat(" InnerException: {0};", (ex.InnerException == null) ? "none" :  ExceptionToText(ex.InnerException, null));
            return message.ToString();
        }
    }
}