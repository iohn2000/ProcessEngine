using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.ProcessEngine.Variables
{
    public abstract class Replacer<T> where T : Replacer<T>
    {
        public string BeginBracket { get; private set; }
        public string EndBracket { get; private set; }
        public string Value { get; private set; }
        public string ProcessedValue { get; set; }

        public abstract T Replace();

        public Replacer(string value)
        {
            this.Value = value;
            this.BeginBracket = "{{";
            this.EndBracket = "}}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="beginbracket"></param>
        /// <param name="endbracket"></param>
        /// <returns></returns>
        public virtual T SetBrackets(string beginbracket, string endbracket)
        {
            this.BeginBracket = beginbracket;
            this.EndBracket = endbracket;
            return (T)this;
        }

    }
}
