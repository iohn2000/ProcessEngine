using System;

namespace Kapsch.IS.EDP.Core.Entities.Operator.Enum
{
    /// <summary>
    /// Enum to define what data-records are in the history of an object.
    /// </summary>
    public enum HistorySearchTypeEnum
    {
        /// <summary>
        /// History contains all historical, future and the currently valid data-records.
        /// </summary>
        Complete = 0,

        /// <summary>
        /// History contains all data-records that were valid.
        /// </summary>
        Historical = 1,

        /// <summary>
        /// History contains all data-records that are going to be valid.
        /// </summary>
        Future = 2
    }

    public static class HistorySearchTypeExtensions
    {
        /// <summary>
        /// Returns the validity-span (from, to) of a <see cref="HistorySearchTypeEnum"/>.
        /// </summary>
        /// <param name="type">The type to get the dates for</param>
        /// <param name="WorkingDate">Used to mock the <see cref="DateTime.Now"/>. If <see langword="null"/> <see cref="DateTime.Now"/> is used.</param>
        /// <returns>A Tuple where Item1 is the from-date and Item2 the to-date.</returns>
        public static Tuple<DateTime, DateTime> GetValiditySpan(this HistorySearchTypeEnum type, DateTime? WorkingDate = null)
        {
            DateTime working;
            if (WorkingDate == null)
            {
                working = DateTime.Now;
            }
            else
            {
                working = WorkingDate.Value;
            }

            switch (type)
            {
                case HistorySearchTypeEnum.Historical:
                    return Tuple.Create(DateTime.MinValue, working);

                case HistorySearchTypeEnum.Future:
                    return Tuple.Create(working, DateTime.MaxValue);

                case HistorySearchTypeEnum.Complete:
                default:
                    return Tuple.Create(DateTime.MinValue, DateTime.MaxValue);
            }
        }
    }
}
