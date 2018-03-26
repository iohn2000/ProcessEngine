using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using Kapsch.IS.Util.Serialiser;
using RabbitMQ.Client;
using System;
using System.Configuration;

namespace Kapsch.IS.ProcessEngine
{
    /// <summary>
    /// Used to create Queue messages for the workflow engine
    /// </summary>
    public class WorkflowMessageQueueProducer
    {
        private IEDPLogger logger = EDPLogger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string RabbitMQHostName = "";
        private string RabbitMQUserName = "";
        private string RabbitMQPassword = "";
        private string RabbitMQQueueName = "";

        public WorkflowMessageQueueProducer()
        {
            logger.Debug("ctor");

            this.RabbitMQHostName = ConfigurationManager.AppSettings["RabbitMQHostName"];
            if (string.IsNullOrWhiteSpace(this.RabbitMQHostName))
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, "RabbitMQ HostName cannot be null or empty");
            logger.Debug(string.Format("..RabbitMQHostName = '{0}'",this.RabbitMQHostName));

            this.RabbitMQUserName = ConfigurationManager.AppSettings["RabbitMQUserName"];
            if (string.IsNullOrWhiteSpace(this.RabbitMQUserName))
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, "RabbitMQ UserName cannot be null or empty");
            logger.Debug(string.Format("..RabbitMQUserName = '{0}'",this.RabbitMQUserName));

            this.RabbitMQPassword = ConfigurationManager.AppSettings["RabbitMQPassword"];
            if (string.IsNullOrWhiteSpace(this.RabbitMQPassword))
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, "RabbitMQ Password cannot be null or empty");
            logger.Debug(string.Format("..RabbitMQPassword = '{0}'",this.RabbitMQPassword));

            this.RabbitMQQueueName = ConfigurationManager.AppSettings["RabbitMQQueueName"];
            if (string.IsNullOrWhiteSpace(this.RabbitMQQueueName))
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL, "RabbitMQ QueueName cannot be null or empty");
            logger.Debug(string.Format("..RabbitMQQueueName = '{0}'",this.RabbitMQQueueName));

        }

        /// <summary>
        /// check if, for given method, all variables exist
        /// </summary>
        /// <returns></returns>
        public bool CheckNewWorkflowMessage()
        {
            throw new NotImplementedException();
        }

        public void CreateNewWorfklowMessage(WorkflowfMessageQueueData wfData)
        {
            if (wfData == null || wfData.WfVariables.Count < 1)
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL,"WorkflowfMessageQueueData cannot be null and must contain WfVariables.");

            try
            {
                var factory = new ConnectionFactory()
                {
                    /* Uri = "amqp://qtkouelg:8e5-7gyREIrGXVO9BhMVtEFeVCkQNdtH@hare.rmq.cloudamqp.com/qtkouelg" */
                    HostName = this.RabbitMQHostName,
                    UserName = this.RabbitMQUserName,
                    Password = this.RabbitMQPassword
                };
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: this.RabbitMQQueueName,
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);


                    IBasicProperties props = channel.CreateBasicProperties();
                    props.Persistent = true;
                    props.CorrelationId = "WFMSG_" + Guid.NewGuid().ToString();
                    
                    byte[] msgText = XmlSerialiserHelper.SerialiseIntoXml(wfData);

                    channel.BasicPublish(exchange: "",
                                         routingKey: this.RabbitMQQueueName /*"DureableQueue"*/,
                                         basicProperties: props,
                                         body: msgText);

                    logger.Info(string.Format("A workflow message was successfully published to {0}.{1}",this.RabbitMQHostName,this.RabbitMQQueueName));
                }
            }
            catch (Exception ex)
            {
                throw new BaseException(ErrorCodeHandler.E_WF_GENERAL,"Error added Rabbit MQ Message to Message Queue. Please retry!",ex);
            }
        }
    }
}
