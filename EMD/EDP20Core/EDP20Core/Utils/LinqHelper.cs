using System.Linq;
using System.Linq.Expressions;
using System;
using System.Linq.Dynamic;
using System.Data.Linq.SqlClient;

using Kapsch.IS.EDP.Core.DB;


namespace Kapsch.IS.EDP.Core.Utils
{
    /// <summary>
    /// http://stackoverflow.com/questions/16013807/unable-to-sort-with-property-name-in-linq-orderby
    /// </summary>
    public static class LinqHelper
    {
        private static IOrderedQueryable<T> OrderingHelper<T>(IQueryable<T> source, string propertyName, bool descending, bool anotherLevel)
        {
            var param = Expression.Parameter(typeof(T), "p");
            var property = Expression.PropertyOrField(param, propertyName);
            var sort = Expression.Lambda(property, param);

            var call = Expression.Call(
                typeof(Queryable),
                (!anotherLevel ? "OrderBy" : "ThenBy") + (descending ? "Descending" : string.Empty),
                new[] { typeof(T), property.Type },
                source.Expression,
                Expression.Quote(sort));

            return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(call);
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName)
        {
            return OrderingHelper(source, propertyName, false, false);
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName, bool descending)
        {
            return OrderingHelper(source, propertyName, descending, false);
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string propertyName)
        {
            return OrderingHelper(source, propertyName, false, true);
        }

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/bb882637.aspx
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="paging"></param>
        /// <returns></returns>
        internal static IOrderedQueryable<T> pageQuery<T>(this IQueryable<T> source, DatabasePaging paging)
        {
            //Sorting
            if (!String.IsNullOrEmpty(paging.SortField))
            {
                source = LinqHelper.OrderBy(source, paging.SortField,paging.Descending);
            }
            else {
                source = LinqHelper.OrderBy(source, "guid");
            }
            source = source.Skip(paging.GetSkip());
            source = source.Take(paging.GetTake());
            return (IOrderedQueryable<T>)source;
        }

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/bb882637.aspx
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="paging"></param>
        /// <returns></returns>
        internal static IOrderedQueryable<T> filteryQuery<T>(this IQueryable<T> source, DatabasePaging paging)
        {
            //Filtering
            if (paging.Filterlist.Any())
            {
                foreach (DatabasePaging.Filter filter in paging.Filterlist)
                {
                    String queryString;
                    if (filter.FilterOperator == "contains")
                    {
                        //.Where("MyColumn.Contains(@0)", myArray)
                        queryString = filter.FilterField + ".Contains(\"" + filter.FilterValue + "\")";

                    }
                    else {
                        queryString = filter.FilterField + "." + filter.FilterOperator + "(\"" + filter.FilterValue + "\")";
                    }
                    source = source.Where(queryString);
                    string query = source.ToString();
                }
            }

            return (IOrderedQueryable<T>) source;
        }



    }
}
