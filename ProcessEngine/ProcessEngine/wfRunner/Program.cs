using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.DataClasses;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kapsch.IS.ProcessEngine.wfRunner
{
    static class Program
    {
        private static Kapsch.IS.ProcessEngine.Runtime runTime;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //TODO: Silent Button for run once with given WF ID
            // WFRunner -s -c -wfdefid="WODE_9d0db2cece6c41f184c1ee23b11f3b84"

            //var instID = CreateWorkflowInstance("WODE_c72eb2ccf95a47f9b8c4232c7e5bfc55"); // 3 test AC with error path
            // simple add eq


            //RunEngine();

            //List<WorkflowMessageVariable> vars = new List<WorkflowMessageVariable>()
            //{
            //    new WorkflowMessageVariable("RequestingPersonEmploymentGuid","xxx"),
            //    new WorkflowMessageVariable("EffectedPersonEmploymentGuid","xxx"),
            //    new WorkflowMessageVariable("ObreGuid","xxx")
            //};
            //CreateWorkflowInstance("WODE_7b96fdd7eb35430c92e7d50fe66108ac", vars); //test email

            //Wakup("WOIN_cd774fbe189b4c21ace941ebeb229b72");          

            RunEngine();

            //while (1 == 1)
            //{

            //    RunEngine();

            //    Console.WriteLine("Press S to STOP");
            //    string key = Console.ReadLine();

            //    if (key == "S" || key == "s")
            //    {
            //        break;
            //    }

            //    //if (info.KeyChar == 'S' || info.KeyChar == 's')
            //    //{
            //    //    break;
            //    //}
            //}


            //string stopme = "now";


            // CreateWorkflowInstance("WODE_7a53c256889346688a7adeca9a50b477"); //all ok test run
            //var instID = CreateWorkflowInstance("WODE_dd9d016b2f6e4d959582023519d2dca4"); // 3 test activites : Complete - Paused - Complete
            //if (instID != null)
            //{
            //RunEngine();
            //    WakeUp(instID);
            //}


            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            Console.ReadLine();
        }

        private static void WakeUp(string workflowInstanceID)
        {
            DatabaseAccess db = new DatabaseAccess();
            WFEWorkflowInstance wfi = db.GetWorkflowInstance(workflowInstanceID);
            db.CreateEngineAlert(wfi.WFI_CurrentActivity, workflowInstanceID, null, null, EnumAlertTypes.Normal);
        }

        private static void RunEngine()
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                runTime = new Runtime(buildDllCache: true);
                string returnMsg = runTime.RunEngine(runTime.dllCache);

                sw.Stop();

                string processingTime = "Processing time : " + sw.ElapsedMilliseconds.ToString() + " ms";
                if (returnMsg == null)
                    writeMessage("No EngineAlert to work on or no alert of type polling due to run. " + processingTime);
                else
                    writeMessage(returnMsg + " " + processingTime + Environment.NewLine);
            }
            catch (BaseException bEx)
            {
                throw bEx;
            }
            catch (Exception ex)
            {
                writeMessage("Exception caught: " + ex.Message + Environment.NewLine + ex.StackTrace);
            }

        }

        private static string CreateWorkflowInstance(string wfdefid, List<WorkflowMessageVariable> inputVars = null)
        {
            try
            {

                Stopwatch sw = Stopwatch.StartNew();


                WorkflowHandler wfHandler = new WorkflowHandler();
                WorkflowMessageData msgData = new WorkflowMessageData();
                msgData.WorkflowDefinitionID = wfdefid;
                msgData.WorkflowVariables = inputVars;
                if (inputVars != null)
                {

                }
                var instID = wfHandler.CreateNewWorkflowInstance(msgData);

                sw.Stop();
                writeMessage("CreateWorkFlow '" + wfdefid + "' Instance took : " + sw.ElapsedMilliseconds.ToString() + " ms.");
                return instID.InstanceID;
            }
            catch (Exception ex)
            {
                writeMessage("Exception caught: " + ex.Message + Environment.NewLine + ex.StackTrace);
            }
            return null;
        }

        private static void writeMessage(String msg)
        {
            System.Console.WriteLine(msg);
        }
    }
}

