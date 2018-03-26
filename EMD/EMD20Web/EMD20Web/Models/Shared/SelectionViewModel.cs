using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.Models.Shared
{
    public class SelectionViewModel
    {
        public SelectionViewModel()
        {
            this.GridHeight = "450px";
            this.ParentFormControlWidth = "100%";
            this.ControlHeight = "";
            this.ControlWidth = "400px";
        }

        public string RenderGridStyle()
        {
            return "height: " + this.GridHeight;
        }

        public bool IsDisabled { get; set; }

        private string identifier;
        public string Identifier
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.identifier))
                {
                    this.identifier = Guid.NewGuid().ToString().Replace("-", "_");
                }
                return this.identifier;
            }
        }

        public string JavaScriptIdPopupDiv
        {
            get
            {
                return string.Format("popup_selection_{0}", this.Identifier);
            }
        }

        public string JavaScriptIdLabel
        {
            get
            {
                return string.Format("idLabel_{0}", this.Identifier);
            }
        }

        public string JavaScriptMethodNameSelectedItem
        {
            get
            {
                return string.Format("on{0}Selected", this.Identifier);
            }
        }

        //public string JavaScriptMethodNameReadOnly
        //{
        //    get
        //    {
        //        return string.Format("on{0}ReadOnly", this.Identifier);
        //    }
        //}

        public string JavaScriptMethodNameOkButtonClicked
        {
            get
            {
                return string.Format("onOkButtonClicked{0}", this.Identifier);
            }
        }

        public string JavaScriptMethodNameDeleteButtonClicked
        {
            get
            {
                return string.Format("onDeleteButtonClicked{0}", this.Identifier);
            }
        }

        public string JavaScriptMethodNameOpenPopup
        {
            get
            {
                return string.Format("on{0}OpenPopup", this.Identifier);
            }
        }

        public string JavaScriptOkButtonId
        {
            get
            {
                return string.Format("okButton{0}", this.Identifier);
            }
        }

        public string SelectionEvent { get; set; }

        public string ObjectLabel { get; set; }
        public string ObjectValue { get; set; }

        public string ClientTemplate { get; set; }

        public string ControlWidth { get; set; }
        public string ParentFormControlWidth { get; set; }

        public string ControlHeight { get; set; }

        public string GridHeight { get; set; }

        public bool HideDeleteButton { get; set; }

        public string TargetControllerName { get; set; }
        public string TargetControllerMethodName { get; set; }

        public string TargetOptionalMethodParameters { get; set; }
        public string ObjectText { get; internal set; }
        public string ReferencePropertyName { get; internal set; }

        [Obsolete("not needed if you set the SelectionViewModel to a property.new() in the default constructor")]
        public string PropertyName { get; internal set; }
    }
}