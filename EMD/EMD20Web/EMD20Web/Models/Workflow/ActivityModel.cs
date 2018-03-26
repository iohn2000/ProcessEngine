using Kapsch.IS.EDP.Core.ServiceReferenceProcessEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.Models.Workflow
{
    public class ActivityModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public int ActivityType { get; set; }

        public static ActivityModel Map(ActivityItem activityItem)
        {
            return new ActivityModel() { Id = activityItem.Id, Name = activityItem.Name, ActivityType = (int)activityItem.ActivityType };
        }

        public static List<ActivityModel> Map(List<ActivityItem> activityItems)
        {
            List<ActivityModel> activityModels = new List<ActivityModel>();


            foreach (ActivityItem item in activityItems)
            {
                activityModels.Add(Map(item));
            }

            return activityModels;
        }
    }


}