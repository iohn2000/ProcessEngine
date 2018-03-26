using Kapsch.IS.EDP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Logic
{
    public class EmploymentChangeTypeHelper
    {

        public int EntitiesToChange;

        public EmploymentChangeTypeHelper(EnumEmploymentChangeType changeType)
        {
            var etc = new int[100];

            etc[(int)EnumEmploymentChangeType.Enterprise] =
                (int)Math.Pow(2, Convert.ToInt32(EnumChangeValueType.NewEmpl)) +
                (int)Math.Pow(2, Convert.ToInt32(EnumChangeValueType.Enterprise)) +
                (int)Math.Pow(2, Convert.ToInt32(EnumChangeValueType.EmploymentType)) +
                (int)Math.Pow(2, Convert.ToInt32(EnumChangeValueType.OrgUnit)) +
                (int)Math.Pow(2, Convert.ToInt32(EnumChangeValueType.Costcenter)) +
                (int)Math.Pow(2, Convert.ToInt32(EnumChangeValueType.Location)) +
                (int)Math.Pow(2, Convert.ToInt32(EnumChangeValueType.EquipmentProc));

            etc[(int)EnumEmploymentChangeType.Organisation] =
                (int)Math.Pow(2, Convert.ToInt32(EnumChangeValueType.OrgUnit)) +
                (int)Math.Pow(2, Convert.ToInt32(EnumChangeValueType.Costcenter)) +
                (int)Math.Pow(2, Convert.ToInt32(EnumChangeValueType.Location));

            etc[(int)EnumEmploymentChangeType.EmploymentType] = //1 + 4 + 64 + 128 = 69
                (int)Math.Pow(2, Convert.ToInt32(EnumChangeValueType.NewEmpl)) + //1
                (int)Math.Pow(2, Convert.ToInt32(EnumChangeValueType.EmploymentType)) + //1+4 =5
                                                                                         //(int)Math.Pow(2, Convert.ToInt32(EnumChangeValueTypes.Pause)) + //1+
                (int)Math.Pow(2, Convert.ToInt32(EnumChangeValueType.EquipmentProc));

            EntitiesToChange = etc[(int)changeType];
        }

        public bool newEmployment;

        public bool isNeeded(EnumChangeValueType entity)
        {
            return IsBitSet((byte)EntitiesToChange, (int)entity);
        }

        public bool IsBitSet(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

    }




}
