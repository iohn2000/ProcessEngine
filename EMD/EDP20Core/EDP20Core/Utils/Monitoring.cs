using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Utils
{
    /// <summary>
    /// class for monitoring the access to the EMD database. 
    /// </summary>
    public class Monitoring
    {
        /// <summary>
        /// For monitoring purpose this method creates a contact in the database and afterwards deletes the contact permanent.
        /// </summary>
        /// <param name="emplGuid"></param>
        /// <returns></returns>
        public static MonitoringResult CreateAndDeleteContactForEmployment(string emplGuid)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            MonitoringResult result = new MonitoringResult();
            try
            {
                ContactManager contactManager = new ContactManager();
                EMDContact newContact = new Entities.EMDContact();
                newContact.EP_Guid = emplGuid;
                newContact.Text = "TESTRAUM";
                newContact.Note = "Testeintrag kmp";
                newContact.ModifyComment = "Testeintrag kmp";
                newContact = contactManager.CreateContactRoom(newContact);
                contactManager.WriteOrModifyContact(newContact);
                ContactHandler contactHandler = new ContactHandler();
                contactHandler.DeleteObject(newContact, false);
                sw.Stop();
                if (sw.ElapsedMilliseconds > 10000)
                    result.Status = EnumMonitoringStatus.WARNING;
                else
                    result.Status = EnumMonitoringStatus.UP;
            }
            catch (Exception ex)
            {
                Util.Logging.IISLogger logger = Util.Logging.ISLogger.GetLogger("KMP", "Test");
                string errorText = string.Format("Exception in test method for kmp call: Message: {0}, StackTrace: {1}", ex.Message, ex.StackTrace);
                result.Error = errorText;
                logger.Error(errorText);
                result.Status = EnumMonitoringStatus.CRITICAL;
            }
            return result;
        }
      
        
    }
}
