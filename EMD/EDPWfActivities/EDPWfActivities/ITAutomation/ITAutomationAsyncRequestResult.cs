using Kapsch.IS.EDP.WFActivity.ITAutomationWebService;
using Kapsch.IS.ProcessEngine.Shared.DataClasses;

namespace Kapsch.IS.EDP.WFActivity.ITAutomation
{
    public class ITAutomationAsyncRequestResult: BaseAsyncRequestResult
    {
        public ResultObjectItem ServiceResult { get; internal set; }
    }
}
