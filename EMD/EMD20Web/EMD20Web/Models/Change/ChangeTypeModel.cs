using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.Models.Change
{
    public class ChangeTypeModel
    {
        public EnumChangeType ChangeType { get; set; }

        public ChangeTypeModel()
        {

        }

        public ChangeTypeModel(EnumChangeType enumChangeType)
        {
            this.ChangeType = enumChangeType;
        }

        public string Name
        {
            get
            {
                return GetDisplayName(ChangeType);
            }
        }

        public static List<ChangeTypeModel> GetChangeTypeModelList()
        {
            List<ChangeTypeModel> changeTypeModelList = new List<ChangeTypeModel>();
            List<EnumChangeType> changeTypeList = System.Enum.GetValues(typeof(EnumChangeType)).Cast<EnumChangeType>().ToList();

            foreach (EnumChangeType userStatus in changeTypeList)
            {
                changeTypeModelList.Add(new ChangeTypeModel(userStatus));
            }


            return changeTypeModelList;
        }


        public static string GetDisplayName(EnumChangeType changeType)
        {
            string name = "No changetype found";

            switch (changeType)
            {
                case EnumChangeType.Enterprise:
                    name = "Enterprise";
                    break;
                case EnumChangeType.EmploymentType:
                    name = "Employment Type";
                    break;
                case EnumChangeType.Organisation:
                    name = "Organisational Change";
                    break;
            }

            return name;
        }
    }
}