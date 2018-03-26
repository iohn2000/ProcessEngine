using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kapsch.IS.EDP.Core.DB;
using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.EDP.Core.Logic.Filter;
using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.EDP.Core.Framework;

namespace Kapsch.IS.EDP.Core.Logic
{
    public class ObjectFlagManager
        : BaseManager
    {
        #region Constructors

        public ObjectFlagManager()
            : base()
        {
        }

        public ObjectFlagManager(CoreTransaction transaction)
            : base(transaction)
        {
        }

        public ObjectFlagManager(string guid_ModifiedBy, string modifyComment = null)
            : base(guid_ModifiedBy, modifyComment)
        {
        }

        public ObjectFlagManager(CoreTransaction transaction, string guid_ModifiedBy, string modifyComment = null)
            : base(transaction, guid_ModifiedBy, modifyComment)
        {
        }

        #endregion Constructors

        public bool IsMainEmployment(string empl_guid)
        {
            bool isMainEmployment = false;
            ObjectFlagHandler ofh = new ObjectFlagHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<IEMDObject<EMDObjectFlag>> mainEmploymentFlags = ofh.GetObjects<EMDObjectFlag, ObjectFlag>("Obj_Guid=\"" + empl_guid + "\" && FlagType=\"" + EnumObjectFlagType.MainEmployment.ToString() + "\"");
            if (mainEmploymentFlags.Count > 1)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "There is more than one active main-employment configured for empl: " + empl_guid.ToString());
            }
            else if (mainEmploymentFlags.Count == 1)
            {
                isMainEmployment = true;
            }

            return isMainEmployment;
        }
        
        /// <summary>
        /// Sets/Remove the flag IsMainDeployment for an employment
        /// </summary>
        /// <param name="emplGuid"></param>
        /// <param name="isMain"></param>
        public void SetMainEmployment(string emplGuid, bool isMain)
        {
            bool hasTransaction = this.Transaction != null;
            if (!hasTransaction)
            {
                this.Transaction = new CoreTransaction();
            }

            if (!hasTransaction)
            {
                this.Transaction.Begin();
            }

            ObjectFlagHandler ofh = new ObjectFlagHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<IEMDObject<EMDObjectFlag>> mainEmploymentFlags = ofh.GetObjects<EMDObjectFlag, ObjectFlag>("Obj_Guid=\"" + emplGuid + "\" && FlagType=\"" + EnumObjectFlagType.MainEmployment.ToString() + "\"");

            PersonManager persManager = new PersonManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            EmploymentManager emplManager = new EmploymentManager(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);


            EMDPerson pers = persManager.GetPersonByEmployment(emplGuid);

            if (isMain)
            {
                //delete Main-Employment-Flags on other Employments
                List<EMDEmployment> empls = emplManager.GetEmploymentsForPerson(pers.Guid,true).Cast<EMDEmployment>().ToList();
                foreach (EMDEmployment empl in empls)
                {
                    if (empl.Guid != emplGuid)
                    {
                        List<EMDObjectFlag> otherMainEmploymentFlags = ofh.GetObjects<EMDObjectFlag, ObjectFlag>("Obj_Guid=\"" + empl.Guid + "\" && FlagType=\"" + EnumObjectFlagType.MainEmployment.ToString() + "\"").Cast<EMDObjectFlag>().ToList();
                        if (otherMainEmploymentFlags.Count > 0)
                        {
                            foreach (EMDObjectFlag of in otherMainEmploymentFlags)
                            {
                                ofh.DeleteDBObject(of);
                            }
                        }
                    }
                }
            }
            
            //if (mainEmploymentFlags.Count > 1)
            //{
            //    if (!hasTransaction)
            //    {
            //        this.Transaction.Rollback();
            //    }
            //    throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "There is more than one active main-employment configured for empl: " + emplGuid.ToString());
            //}

            if (!isMain && mainEmploymentFlags.Count == 1)
            {
                ofh.DeleteDBObject(mainEmploymentFlags[0]);
            }

            if (isMain && mainEmploymentFlags.Count == 0)
            {
                EMDObjectFlag flag = new EMDObjectFlag();
                flag.Obj_Guid = emplGuid;
                flag.FlagType = EnumObjectFlagType.MainEmployment.ToString();
                ofh.CreateObject(flag);
            }

            if (!hasTransaction)
            {
                this.Transaction.Commit();
            }
        }


        public bool IsEmploymentVisibleInPhonebook(string empl_guid)
        {
            bool isEmploymentVisibleInPhonebook = false;
            ObjectFlagHandler ofh = new ObjectFlagHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<IEMDObject<EMDObjectFlag>> isEmploymentVisibleInPhonebookFlags = ofh.GetObjects<EMDObjectFlag, ObjectFlag>("Obj_Guid=\"" + empl_guid + "\" && FlagType=\"" + EnumObjectFlagType.Visible.ToString() + "\"");
            if (isEmploymentVisibleInPhonebookFlags.Count > 1)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "There is more than one active employment configured for Visible Phonebook: " + empl_guid.ToString());
            }
            else if (isEmploymentVisibleInPhonebookFlags.Count == 1)
            {
                isEmploymentVisibleInPhonebook = true;
            }

            return isEmploymentVisibleInPhonebook;
        }

        public bool UpdateIsEmploymentVisibleInPhonebook(string employmentGuid, bool state, CoreTransaction transaction = null)
        {
            bool updateAD = false;
            ObjectFlagHandler ofh = new ObjectFlagHandler(transaction ?? this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<IEMDObject<EMDObjectFlag>> updateADFlags = ofh.GetObjects<EMDObjectFlag, ObjectFlag>("Obj_Guid=\"" + employmentGuid + "\" && FlagType=\"" + EnumObjectFlagType.Visible.ToString() + "\"");
            if (updateADFlags.Count > 1)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "More than one employment that should be updated to the UpdateIsEmploymentVisibleInPhonebook - employmentGuid: " + employmentGuid.ToString());
            }

            if (!state && updateADFlags.Count == 1)
            {
                ofh.DeleteDBObject(updateADFlags[0]);
            }

            if (state && updateADFlags.Count == 0)
            {
                EMDObjectFlag flag = new EMDObjectFlag();
                flag.Obj_Guid = employmentGuid;
                flag.FlagType = EnumObjectFlagType.Visible.ToString();
                ofh.CreateObject(flag);
            }

            return updateAD;
        }

        public bool IsPersonVisibleInPhonebook(string pers_guid)
        {
            bool isPersonVisibleInPhonebook = false;
            ObjectFlagHandler ofh = new ObjectFlagHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<IEMDObject<EMDObjectFlag>> isPersonVisibleInPhonebookFlags = ofh.GetObjects<EMDObjectFlag, ObjectFlag>("Obj_Guid=\"" + pers_guid + "\" && FlagType=\"" + EnumObjectFlagType.VisiblePhone.ToString() + "\"");
            if (isPersonVisibleInPhonebookFlags.Count > 1)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "More than one active person with pers_guid: " + pers_guid.ToString());
            }
            else if (isPersonVisibleInPhonebookFlags.Count == 1)
            {
                isPersonVisibleInPhonebook = true;
            }

            return isPersonVisibleInPhonebook;
        }

        public bool UpdateIsPersonVisibleInPhonebook(string pers_guid, bool isVisible, CoreTransaction transaction = null)
        {
            bool updateAD = false;
            ObjectFlagHandler ofh = new ObjectFlagHandler(transaction ?? this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<IEMDObject<EMDObjectFlag>> updateADFlags = ofh.GetObjects<EMDObjectFlag, ObjectFlag>("Obj_Guid=\"" + pers_guid + "\" && FlagType=\"" + EnumObjectFlagType.VisiblePhone.ToString() + "\"");
            if (updateADFlags.Count > 1)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "More than one active person that should be updated to the VisibleInPhonebook - pers_guid: " + pers_guid.ToString());
            }
  
            if (!isVisible && updateADFlags.Count == 1)
            {
                ofh.DeleteDBObject(updateADFlags[0]);
            }

            if (isVisible && updateADFlags.Count == 0)
            {
                EMDObjectFlag flag = new EMDObjectFlag();
                flag.Obj_Guid = pers_guid;
                flag.FlagType = EnumObjectFlagType.VisiblePhone.ToString();
                ofh.CreateObject(flag);
            }

            return updateAD;
        }

        public bool UpdateDNA(string empl_guid)
        {
            bool updateDNA = false;
            ObjectFlagHandler ofh = new ObjectFlagHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<IEMDObject<EMDObjectFlag>> updateDNAFlags = ofh.GetObjects<EMDObjectFlag, ObjectFlag>("Obj_Guid=\"" + empl_guid + "\" && FlagType=\"" + EnumObjectFlagType.UpdateDNA.ToString() + "\"");
            if (updateDNAFlags.Count > 1)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "More than one active employment that should be updated to the DNA - empl_guid: " + empl_guid.ToString());
            }
            else if (updateDNAFlags.Count == 1)
            {
                updateDNA = true;
            }

            return updateDNA;
        }

        public bool UpdateIsDNA(string employmentGuid, bool state, CoreTransaction transaction = null)
        {
            bool updateAD = false;
            ObjectFlagHandler ofh = new ObjectFlagHandler(transaction ?? this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<IEMDObject<EMDObjectFlag>> updateADFlags = ofh.GetObjects<EMDObjectFlag, ObjectFlag>("Obj_Guid=\"" + employmentGuid + "\" && FlagType=\"" + EnumObjectFlagType.UpdateDNA.ToString() + "\"");
            if (updateADFlags.Count > 1)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "More than one employment that should be updated to the UpdateIsDNA - employmentGuid: " + employmentGuid.ToString());
            }

            if (!state && updateADFlags.Count == 1)
            {
                ofh.DeleteDBObject(updateADFlags[0]);
            }

            if (state && updateADFlags.Count == 0)
            {
                EMDObjectFlag flag = new EMDObjectFlag();
                flag.Obj_Guid = employmentGuid;
                flag.FlagType = EnumObjectFlagType.UpdateDNA.ToString();
                ofh.CreateObject(flag);
            }

            return updateAD;
        }

        public bool UpdateAD(string empl_guid)
        {
            bool updateAD = false;
            ObjectFlagHandler ofh = new ObjectFlagHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<IEMDObject<EMDObjectFlag>> updateADFlags = ofh.GetObjects<EMDObjectFlag, ObjectFlag>("Obj_Guid=\"" + empl_guid + "\" && FlagType=\"" + EnumObjectFlagType.UpdateAD.ToString() + "\"");
            if (updateADFlags.Count > 1)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "More than one active employment that should be updated to the AD - empl_guid: " + empl_guid.ToString());
            }
            else if (updateADFlags.Count == 1)
            {
                updateAD = true;
            }

            return updateAD;
        }

        public bool UpdateIsAD(string employmentGuid, bool state, CoreTransaction transaction = null)
        {
            bool updateAD = false;
            ObjectFlagHandler ofh = new ObjectFlagHandler(transaction ?? this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<IEMDObject<EMDObjectFlag>> updateADFlags = ofh.GetObjects<EMDObjectFlag, ObjectFlag>("Obj_Guid=\"" + employmentGuid + "\" && FlagType=\"" + EnumObjectFlagType.UpdateAD.ToString() + "\"");

            PersonManager persManager = new PersonManager(transaction ?? this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            EmploymentManager emplManager = new EmploymentManager(transaction ?? this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            EMDPerson pers = persManager.GetPersonByEmployment(employmentGuid);

            if (state)
            {
                //delete AD-Flags on other Employments
                List<EMDEmployment> empls = emplManager.GetEmploymentsForPerson(pers.Guid).Cast<EMDEmployment>().ToList();
                foreach (EMDEmployment empl in empls)
                {
                    if (empl.Guid != employmentGuid)
                    {
                        List<EMDObjectFlag> otherMainEmploymentFlags = ofh.GetObjects<EMDObjectFlag, ObjectFlag>("Obj_Guid=\"" + empl.Guid + "\" && FlagType=\"" + EnumObjectFlagType.UpdateAD.ToString() + "\"").Cast<EMDObjectFlag>().ToList();
                        if (otherMainEmploymentFlags.Count > 0)
                        {
                            foreach (EMDObjectFlag of in otherMainEmploymentFlags)
                            {
                                ofh.DeleteDBObject(of);
                            }
                        }
                    }
                }
            }


            if (updateADFlags.Count > 1)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Update to AD is set more than once for this employment - employmentGuid: " + employmentGuid.ToString());
            }

            if (!state && updateADFlags.Count == 1)
            {
                ofh.DeleteDBObject(updateADFlags[0]);
            }

            if (state && updateADFlags.Count == 0)
            {
                EMDObjectFlag flag = new EMDObjectFlag();
                flag.Obj_Guid = employmentGuid;
                flag.FlagType = EnumObjectFlagType.UpdateAD.ToString();
                ofh.CreateObject(flag);
            }

            return updateAD;
        }

        public bool IsMainAccount(string emac_guid)
        {
            bool isMainAccount = false;
            ObjectFlagHandler ofh = new ObjectFlagHandler(this.Transaction);
            List<IEMDObject<EMDObjectFlag>> isMainAccountFlags = ofh.GetObjects<EMDObjectFlag, ObjectFlag>("Obj_Guid=\"" + emac_guid + "\" && FlagType=\"" + EnumObjectFlagType.MainAccount.ToString() + "\"");
            if (isMainAccountFlags.Count > 1)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "More than one active employmentaccount is set as main - emac_guid: " + emac_guid.ToString());
            }
            else if (isMainAccountFlags.Count == 1)
            {
                isMainAccount = true;
            }

            return isMainAccount;
        }

        public bool UpdateIsMainAccount(string employmentAccountGuid, bool state, CoreTransaction transaction = null)
        {
            bool updateAD = false;
            ObjectFlagHandler ofh = new ObjectFlagHandler(transaction ?? this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<IEMDObject<EMDObjectFlag>> updateFlags = ofh.GetObjects<EMDObjectFlag, ObjectFlag>("Obj_Guid=\"" + employmentAccountGuid + "\" && FlagType=\"" + EnumObjectFlagType.MainAccount.ToString() + "\"");
            if (updateFlags.Count > 1)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "More than one employment that should be updated to the UpdateIsMainAccount - employmentGuid: " + employmentAccountGuid.ToString());
            }

            if (!state && updateFlags.Count == 1)
            {
                ofh.DeleteDBObject(updateFlags[0]);
            }

            if (state && updateFlags.Count == 0)
            {
                EMDObjectFlag flag = new EMDObjectFlag();
                flag.Obj_Guid = employmentAccountGuid;
                flag.FlagType = EnumObjectFlagType.MainAccount.ToString();
                ofh.CreateObject(flag);
            }

            return updateAD;
        }

        public bool IsPictureVisible(string pers_guid)
        {
            bool isPictureVisible = false;
            ObjectFlagHandler ofh = new ObjectFlagHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<IEMDObject<EMDObjectFlag>> isPictureVisibleFlags = ofh.GetObjects<EMDObjectFlag, ObjectFlag>("Obj_Guid=\"" + pers_guid + "\" && FlagType=\"" + EnumObjectFlagType.PictureVisible.ToString() + "\"");
            if (isPictureVisibleFlags.Count > 1)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "More than one active person found for pers_guid: " + pers_guid.ToString());
            }
            else if (isPictureVisibleFlags.Count == 1)
            {
                isPictureVisible = true;
            }

            return isPictureVisible;
        }

        public bool UpdateIsPictureVisible(string pers_guid, bool isVisible, CoreTransaction transaction = null)
        {
            bool updateAD = false;
            ObjectFlagHandler ofh = new ObjectFlagHandler(transaction ?? this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<IEMDObject<EMDObjectFlag>> updateADFlags = ofh.GetObjects<EMDObjectFlag, ObjectFlag>("Obj_Guid=\"" + pers_guid + "\" && FlagType=\"" + EnumObjectFlagType.PictureVisible.ToString() + "\"");
            if (updateADFlags.Count > 1)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "More than one active person that should be updated to the PictureVisible - pers_guid: " + pers_guid.ToString());
            }

            if (!isVisible && updateADFlags.Count == 1)
            {
                ofh.DeleteDBObject(updateADFlags[0]);
            }

            if (isVisible && updateADFlags.Count == 0)
            {
                EMDObjectFlag flag = new EMDObjectFlag();
                flag.Obj_Guid = pers_guid;
                flag.FlagType = EnumObjectFlagType.PictureVisible.ToString();
                ofh.CreateObject(flag);
            }

            return updateAD;
        }

        public bool IsPictureVisibleAD(string pers_guid)
        {
            bool isPictureVisibleAD = false;
            ObjectFlagHandler ofh = new ObjectFlagHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<IEMDObject<EMDObjectFlag>> isPictureVisibleADFlags = ofh.GetObjects<EMDObjectFlag, ObjectFlag>("Obj_Guid=\"" + pers_guid + "\" && FlagType=\"" + EnumObjectFlagType.PictureVisibleAD.ToString() + "\"");
            if (isPictureVisibleADFlags.Count > 1)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "More than one active person found for pers_guid: " + pers_guid.ToString());
            }
            else if (isPictureVisibleADFlags.Count == 1)
            {
                isPictureVisibleAD = true;
            }

            return isPictureVisibleAD;
        }

        public bool UpdateIsPictureVisibleAD(string pers_guid, bool isVisible, CoreTransaction transaction = null)
        {
            bool updateAD = false;
            ObjectFlagHandler ofh = new ObjectFlagHandler(transaction ?? this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<IEMDObject<EMDObjectFlag>> updateADFlags = ofh.GetObjects<EMDObjectFlag, ObjectFlag>("Obj_Guid=\"" + pers_guid + "\" && FlagType=\"" + EnumObjectFlagType.PictureVisibleAD.ToString() + "\"");
            if (updateADFlags.Count > 1)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "More than one active person that should be updated to the PictureVisibleAD - pers_guid: " + pers_guid.ToString());
            }

            if (!isVisible && updateADFlags.Count == 1)
            {
                ofh.DeleteDBObject(updateADFlags[0]);
            }

            if (isVisible && updateADFlags.Count == 0)
            {
                EMDObjectFlag flag = new EMDObjectFlag();
                flag.Obj_Guid = pers_guid;
                flag.FlagType = EnumObjectFlagType.PictureVisibleAD.ToString();
                ofh.CreateObject(flag);
            }

            return updateAD;
        }

        public bool SyncPictureForEnterprise(string ente_guid)
        {
            bool syncPictureForEnterprise = false;
            ObjectFlagHandler ofh = new ObjectFlagHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<IEMDObject<EMDObjectFlag>> syncPictureForEnterpriseFlags = ofh.GetObjects<EMDObjectFlag, ObjectFlag>("Obj_Guid=\"" + ente_guid + "\" && FlagType=\"" + EnumObjectFlagType.AdPictureEnterprise.ToString() + "\"");
            if (syncPictureForEnterpriseFlags.Count > 1)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "More than one active enterprise found for ente_guid: " + ente_guid.ToString());
            }
            else if (syncPictureForEnterpriseFlags.Count == 1)
            {
                syncPictureForEnterprise = true;
            }

            return syncPictureForEnterprise;
        }

        public bool EnterpriseHasEmployees(string ente_guid)
        {
            bool enterpriseHasEmployees = false;
            ObjectFlagHandler ofh = new ObjectFlagHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<IEMDObject<EMDObjectFlag>> enterpriseHasEmployeesFlags = ofh.GetObjects<EMDObjectFlag, ObjectFlag>("Obj_Guid=\"" + ente_guid + "\" && FlagType=\"" + EnumObjectFlagType.HasEmployees.ToString() + "\"");
            if (enterpriseHasEmployeesFlags.Count > 1)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "More than one active enterprise found for ente_guid: " + ente_guid.ToString());
            }
            else if (enterpriseHasEmployeesFlags.Count == 1)
            {
                enterpriseHasEmployees = true;
            }

            return enterpriseHasEmployees;
        }


        public List<EMDObjectFlag> ObjectFlagsByType(EnumObjectFlagType objectFlagType)
        {
            ObjectFlagHandler ofh = new ObjectFlagHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            
            List<EMDObjectFlag> objFlags = ofh.GetObjects<EMDObjectFlag, ObjectFlag>("FlagType=\"" + Enum.GetName(typeof(EnumObjectFlagType), objectFlagType) + "\"").Cast<EMDObjectFlag>().ToList();
            return objFlags;
        }

        public List<EMDObjectFlag> ObjectFlagsByTypeAndObjectGuid(EnumObjectFlagType ObjectFlagType, string ObjectGuid, bool DeliverInActive)
        {
            ObjectFlagHandler ofh = new ObjectFlagHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            ofh.DeliverInActive = DeliverInActive;
            List<EMDObjectFlag> objFlags = ofh.GetObjects<EMDObjectFlag, ObjectFlag>("Obj_Guid=\"" + ObjectGuid + "\" && FlagType=\"" + Enum.GetName(typeof(EnumObjectFlagType), ObjectFlagType) + "\"").Cast<EMDObjectFlag>().ToList();
            return objFlags;
        }

        public bool IsMainLocation(string enlo_guid)
        {
            bool isMainLocation = false;
            ObjectFlagHandler ofh = new ObjectFlagHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<IEMDObject<EMDObjectFlag>> isMainLocationFlags = ofh.GetObjects<EMDObjectFlag, ObjectFlag>("Obj_Guid=\"" + enlo_guid + "\" && FlagType=\"" + EnumObjectFlagType.MainLocation.ToString() + "\"");
            if (isMainLocationFlags.Count > 1)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "More than one active locations found for enlo_guid: " + enlo_guid.ToString());
            }
            else if (isMainLocationFlags.Count == 1)
            {
                isMainLocation = true;
            }

            return isMainLocation;
        }

        public bool UpdateEFaxForEmploymentContactsOnLocation(string enlo_guid)
        {
            bool updateEFaxForEmploymentContactsOnLocation = false;
            ObjectFlagHandler ofh = new ObjectFlagHandler(this.Transaction, this.Guid_ModifiedBy, this.ModifyComment);
            List<IEMDObject<EMDObjectFlag>> updateEFaxForEmploymentContactsOnLocationFlags = ofh.GetObjects<EMDObjectFlag, ObjectFlag>("Obj_Guid=\"" + enlo_guid + "\" && FlagType=\"" + EnumObjectFlagType.EFaxUpdate.ToString() + "\"");
            if (updateEFaxForEmploymentContactsOnLocationFlags.Count > 1)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "More than one active locations found for enlo_guid: " + enlo_guid.ToString());
            }
            else if (updateEFaxForEmploymentContactsOnLocationFlags.Count == 1)
            {
                updateEFaxForEmploymentContactsOnLocation = true;
            }

            return updateEFaxForEmploymentContactsOnLocation;
        }

        //public bool IsMainEmployment(string empl_guid)
        //{

        //}
    }
}
