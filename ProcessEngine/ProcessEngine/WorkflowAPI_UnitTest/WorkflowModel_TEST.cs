using Kapsch.IS.ProcessEngine;
using Kapsch.IS.ProcessEngine.DataLayer;
using Kapsch.IS.ProcessEngine.Shared.Enums;
using Kapsch.IS.ProcessEngine.Shared.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace TESTING
{
    [TestClass]
    public class WorkflowModel_TEST
    {
        [TestMethod]
        public void UpdateVariables_TEST()
        {
            WorkflowModel m = new WorkflowModel();
            m.LoadModelXml(File.ReadAllText("UpdateVariables_TEST.xml"));
            m.UpdateWorkflowVariableInXmlModel("theKing", "johannes", EnumVariablesDataType.stringType, EnumVariableDirection.both);
        }

        [TestMethod]
        [ExpectedException(typeof(WorkflowException))]
        public void VariableSectionDoesntExist()
        {
            WorkflowModel m = new WorkflowModel();
            m.LoadModelXml(File.ReadAllText("VariableSectionDoesntExist.xml"));
            m.UpdateWorkflowVariableInXmlModel("theKing", "johannes", EnumVariablesDataType.stringType, EnumVariableDirection.both);
        }

        [TestMethod]
        public void InitVariables_ProcessFromStart()
        {

        }

        [TestMethod]
        public void AddActivity()
        {
            WorkflowModel m = new WorkflowModel();
            m.LoadModelXml(File.ReadAllText("TestData/MobileWorkflow.xml"));

            ProcessDefinition processDefinition = new ProcessDefinition();
            // take the first activkty key
            string firstkey = processDefinition.GetAllActivities().First().WFAD_ID;

            Activity activity = new Activity(processDefinition.GetActivityDefinitionTemplate(firstkey));

            m.AddActivity(activity);

            string xml = m.GetWorkflowXml();
        }

        [TestMethod]
        [ExpectedException(typeof(WorkflowException))]
        public void WorkflowException_TEST()
        {
            try
            {
                WorkflowModel m = new WorkflowModel();
                m.LoadModelXml(File.ReadAllText(@"TestData/InvalidWF.xml"));
            }
            catch (WorkflowException ex)
            {

                throw ex;
            }



        }


        [TestMethod]
        [ExpectedException(typeof(WorkflowException))]
        public void WorkflowException2_TEST()
        {
            try
            {
                WorkflowModel m = new WorkflowModel();
                m.LoadModelXml(File.ReadAllText(@"TestData/SchemaFailed.xml"));
            }
            catch (WorkflowException ex)
            {

                throw ex;
            }



        }

    }
}
