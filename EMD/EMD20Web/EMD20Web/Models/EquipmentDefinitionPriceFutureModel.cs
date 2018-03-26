using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kapsch.IS.EDP.Core.Framework;
using System.ComponentModel.DataAnnotations;
using Kapsch.IS.EDP.Core.Logic;
using Kapsch.IS.EDP.Core.Entities;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class EquipmentDefinitionPriceFutureModel : BaseModel
    {
        private DateTime activeFromFuture;

        public bool IsOwnerOfEquipment { get; set; }

        /// <summary>
        /// workaround, because Telerik Javascript can't find element with FutureContact.ActiveFrom
        /// </summary>
        [Required]
        public DateTime ActiveFromFuture
        {
            get { return activeFromFuture; }
            set
            {
                this.activeFromFuture = value;
                if (this.FutureEquipmentDefinitionPrice != null)
                {
                    this.FutureEquipmentDefinitionPrice.ActiveFrom = value;
                }
            }
        }


        private EquipmentDefinitionPriceModel futureEquipmentDefinitionPrice;

        public EquipmentDefinitionPriceModel CurrentEquipmentDefinitionPrice { get; set; }

        public EquipmentDefinitionPriceModel FutureEquipmentDefinitionPrice
        {
            get { return this.futureEquipmentDefinitionPrice; }
            set
            {
                this.ActiveFromFuture = value.ActiveFrom;
                this.futureEquipmentDefinitionPrice = value;
            }
        }

        private bool? isFutureChecked;

        public bool IsFutureChecked
        {
            get
            {
                return this.isFutureChecked.HasValue ? this.isFutureChecked.Value : false;
            }
            set
            {
                this.isFutureChecked = value;
            }
        }

        public bool HasFutureEquipmentDefinitionPrice
        {
            get
            {
                bool hasFuture = !string.IsNullOrEmpty(FutureEquipmentDefinitionPrice.Guid);

                if (this.isFutureChecked == null)
                {
                    IsFutureChecked = hasFuture;
                }

                return hasFuture;
            }
        }

        public EquipmentDefinitionPriceFutureModel()
        {
            CurrentEquipmentDefinitionPrice = new EquipmentDefinitionPriceModel();
            FutureEquipmentDefinitionPrice = new EquipmentDefinitionPriceModel();
        }

        public void Init(EquipmentDefinitionPriceModel current, EquipmentDefinitionPriceModel future)
        {
            CurrentEquipmentDefinitionPrice = current;
            FutureEquipmentDefinitionPrice = future;
        }

        public override void InitializeSecurity(SecurityUser securityUser)
        {
            base.InitializeBaseSecurity(securityUser);
            EquipmentDefinitionOwnerManager equipmentDefinitionOwnerManager = new EquipmentDefinitionOwnerManager();
            PersonManager persManager = new PersonManager();
            EMDPerson person = persManager.GetPersonByUserId(securityUser.UserId);

            this.IsOwnerOfEquipment = equipmentDefinitionOwnerManager.IsPersonOwnerOfEquipment(this.CurrentEquipmentDefinitionPrice.EQDE_Guid, person.Guid);
        }
    }

    
}