using Kapsch.IS.EDP.Core.ServiceReferenceProcessEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.Models.Workflow
{
    public class ActivityResultMessageModel
    {
        public DateTime Created { get; set; }
        public string Text { get; set; }

        public string ActivityInstance { get; set; }

        public static ActivityResultMessageModel Map(ActivityResultMessageItem activityResultMessageItem)
        {
            ActivityResultMessageModel activityResultMessageModel = new ActivityResultMessageModel()
            {
                Created = activityResultMessageItem.ARM_Created,
                Text = activityResultMessageItem.ARM_ResultMessage,
                ActivityInstance = activityResultMessageItem.ARM_ActivityInstanceId
            };


            return activityResultMessageModel;
        }
    }
}