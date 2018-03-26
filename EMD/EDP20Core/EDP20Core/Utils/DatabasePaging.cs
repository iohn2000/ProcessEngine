using System;
using System.Collections.Generic;

using System.Linq.Expressions;

namespace Kapsch.IS.EDP.Core.Utils
{
    public class DatabasePaging
    {
        public int PageSize { get; set; }
        public int Page { get; set; }
        public string SortField { get; set; }
        public bool Descending { get; set; }



        public List<Filter> Filterlist { get; set; }

        /// <summary>
        /// This field contains nr of all Elements AFTER queying
        /// </summary>
        public int NrOfAllElements { get; set; }
        public int NrOfPages
        {
            get
            {
                if (PageSize == 0 || NrOfAllElements == 0) { return 0; }
                int pages = NrOfAllElements / PageSize;
                if ((NrOfAllElements % PageSize) > 0) pages += 1;
                return pages;
            }

        }
        public int LastPageSize
        {
            get
            {
                if (PageSize == 0 || NrOfAllElements == 0) { return 0; }
                int pages = NrOfAllElements % PageSize;
                return pages;
            }

        }

        public int GetSkip()
        {
            return (Page - 1) * PageSize;
        }

        public int GetTake()
        {
            return PageSize;
        }

        public DatabasePaging()
        {

        }

        public class Filter
        {
            public string FilterField { get; set; }
            public string FilterOperator { get; set; }
            public string FilterValue { get; set; }
            public Expression FilterExpression { get; set; }

            public Filter(string filterField, string filterOperator, string filterValue, Expression filterExpression)
            {
                this.FilterField = filterField;
                this.FilterOperator = filterOperator;
                this.FilterValue = filterValue;
                this.FilterExpression = filterExpression;
            }

            public override string ToString()
            {
                return string.Format("{0}-{1}-{2}-{3}", FilterField, FilterOperator, FilterValue, FilterExpression.ToString());
            }
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}-{2}-{3}-{4}", PageSize, Page, SortField, Descending, Filterlist.ToString());
        }
    }
}
