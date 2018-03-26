using Kapsch.IS.Util.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.WFActivity
{
    public class WorkflowLogger : IEDPLogger
    {
        public bool IsDebugEnabled
        {
            get
            {
                return this.logger.IsDebugEnabled;
            }
        }
        public bool IsErrorEnabled
        {
            get
            {
                return this.logger.IsErrorEnabled;
            }
        }
        public bool IsFatalEnabled
        {
            get
            {
                return this.logger.IsFatalEnabled;
            }
        }
        public bool IsInfoEnabled
        {
            get
            {
                return this.logger.IsInfoEnabled;
            }
        }
        public bool IsWarnEnabled
        {
            get
            {
                return this.logger.IsWarnEnabled;
            }
        }

        public string LoggingContext { get; set; }

        private IEDPLogger logger;

        public WorkflowLogger(IEDPLogger theLogger, string loggingContext)
        {
            this.logger = theLogger;
            this.LoggingContext = loggingContext;
        }

        public void Debug(object message)
        {
            this.logger.Debug(string.Format("{0}{1}", this.LoggingContext, message));
        }

        public void Debug(object message, Exception exception)
        {
            this.logger.Debug(string.Format("{0}{1}", this.LoggingContext, message), exception);
        }

        public void Error(object message)
        {
            this.logger.Error(string.Format("{0}{1}", this.LoggingContext, message));
        }

        public void Error(object message, Exception exception)
        {
            this.logger.Error(string.Format("{0}{1}", this.LoggingContext, message), exception);
        }

        public void Fatal(object message)
        {
            this.logger.Fatal(string.Format("{0}{1}", this.LoggingContext, message));
        }

        public void Fatal(object message, Exception exception)
        {
            this.logger.Fatal(string.Format("{0}{1}", this.LoggingContext, message), exception);
        }

        public void Info(object message)
        {
            this.logger.Info(string.Format("{0}{1}", this.LoggingContext, message));
        }

        public void Info(object message, Exception exception)
        {
            this.logger.Info(string.Format("{0}{1}", this.LoggingContext, message), exception);
        }

        public void Warn(object message)
        {
            this.logger.Warn(string.Format("{0}{1}", this.LoggingContext, message));
        }

        public void Warn(object message, Exception exception)
        {
            this.logger.Warn(string.Format("{0}{1}", this.LoggingContext, message), exception);
        }
    }
}
