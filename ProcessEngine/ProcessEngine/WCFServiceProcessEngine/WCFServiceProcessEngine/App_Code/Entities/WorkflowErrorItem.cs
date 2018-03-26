using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

/// <summary>
/// Summary description for WorkflowErrorItem
/// </summary>
[DataContract]
public class WorkflowErrorItem
{
    [DataMember]
    public string Message { get; set; }
    [DataMember]
    public int LineNumber { get; set; }
    [DataMember]
    public int LinePosition { get; set; }

    public WorkflowErrorItem()
    {

    }

    public WorkflowErrorItem(string message, int lineNumber, int linePosition)
    {
        this.Message = message;
        this.LineNumber = lineNumber;
        this.LinePosition = linePosition;

    }
}