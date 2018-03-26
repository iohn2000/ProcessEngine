using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;

using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using Kendo.Mvc;

using Kapsch.IS.EDP.Core.Utils;
using Kapsch.IS.Util.Logging;

namespace Kapsch.IS.EMD.EMD20Web.Core
{
    public class EMDPagingUtil
    {
        static IISLogger logger = ISLogger.GetLogger("EMDPagingUtil");

        public static DatabasePaging GetEDPDatabasePaging([DataSourceRequest]DataSourceRequest request) {

            DatabasePaging dp = new DatabasePaging();
            dp.PageSize = request.PageSize;
            dp.Page = request.Page;

            if (request.Sorts!=null && request.Sorts.Count > 0)
            {
                dp.SortField = request.Sorts[0].Member;
                //TODO Set correct Directions
                //dp.Direction = Int32.Parse(request.Sorts[0].SortDirection.ToString());
                dp.Descending = false;
            }

            IEnumerable<IFilterDescriptor> reqfilters = request.Filters;

            if (reqfilters.Any())
            {
                dp.Filterlist = new List<DatabasePaging.Filter>();
                foreach (var filter in request.Filters)
                {
                    var descriptor = filter as FilterDescriptor;
                    Type filtertype;
                    if (descriptor != null)
                    {
                        if (descriptor.MemberType == null) filtertype = typeof(String);
                        else filtertype = descriptor.MemberType;
                        //ParameterExpression paramExpression = Expression.Parameter(filtertype, descriptor.Member);
                        dp.Filterlist.Add(new DatabasePaging.Filter(
                            descriptor.Member,
                            descriptor.Operator.ToString(),
                            descriptor.Value.ToString(),
                            //filter.CreateFilterExpression(paramExpression)));
                            null));
                    }
                    else if (filter is CompositeFilterDescriptor)
                    {
                        //int todo = 1; TODO implement Composit filter!!
                        //ModifyFilters(((CompositeFilterDescriptor) filter).FilterDescriptors);
                        logger.Info("EMDPagingUtil.GetEDPDatabasePaging(DataSourceRequest request) => TODO implement Composit filter!!");
                    }
                }
                
            };
            //TODO Set correct Directions
            //dp.Direction = Int32.Parse(request.Sorts[0].SortDirection.ToString());
            dp.Descending = false;
            return dp;
        
        }



    }
}
