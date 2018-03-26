using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    [Serializable]
    public class TextValueModel
    {
        public string Text { get; set; }
        public string Value { get; set; }

        public object Data { get; set; }

        public TextValueModel()
        {
            Text = String.Empty;
            Value = String.Empty;
        }

        public TextValueModel(string text, string value)
        {
            this.Text = text;
            this.Value = value;
        }

        public TextValueModel(string text, string value, object data)
        {
            this.Text = text;
            this.Value = value;
            this.Data = data;
        }
    }


}