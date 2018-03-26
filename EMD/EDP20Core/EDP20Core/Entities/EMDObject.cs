using System;
using System.Text.RegularExpressions;

namespace Kapsch.IS.EDP.Core.Entities
{
    /// <summary>
    /// Base-Type of all database-objects declaring shared fields and methods.
    /// </summary>
    /// <remarks>
    /// The EMDObject holds values shared by all objects in the database. These are used for the data-management of EDP. <br/><br/>
    /// <h3>Validity System</h3>
    /// <para>EDP knows two kinds of validity, they are named Valid and Active. Valid refers to the validity of the stored data. E.g. when a Company gets a new phone number 
    /// the data-record with the old number is not valid anymore, but the company itself remains active. Active on the other hand refers to the 'real' object represented by the data.
    /// E.g. if the same company gets closed down it is not active anymore, but the data about the company remains valid.</para>
    /// <h3>Historization</h3>
    /// <para>EDP keeps the old version of updated data-records as invalid data. To track the changes the <see cref="HistoryGuid"/> holds the <see cref="Guid"/> of the latest record.
    /// When updating the <see cref="Guid"/> is copied to <see cref="HistoryGuid"/> and then gets replaced by a new generated GUID to keep the uniqueness. Furthermore the record is 
    /// invalidated by setting <see cref="ValidTo"/> to <see cref="DateTime.Now"/>. The updated record gets written to the database as a new record.
    /// One can find the whole history of an record by allowing invalid objects to be found in the Handlers (<see cref="EMDBaseObjectHandler"/>)
    /// and matching the <see cref="Guid"/> of the latest record with the <see cref="HistoryGuid"/>.</para>
    /// <h3>Modification</h3>
    /// <para>To keep track of who modified a record and what was modified each record holds the <see cref="EMDPerson">Person-Guid</see> of the one who modified the record
    /// and a <see cref="ModifyComment"/> describing the changes.</para>
    /// </remarks>
    /// <typeparam name="T">Type of the concrete EMD-object implementation</typeparam>
    public class EMDObject<T> : IEMDObject<T>
    {
        /// <summary>
        /// EMDObjects with this status are considered as valid.
        /// </summary>
        /// <value>"Valid"</value>
        public const string VALIDITY_STATUS_VALID = "Valid";

        /// <summary>
        /// EMDObjects with this status are not anymore considered as valid.
        /// </summary>
        /// <value>"Past"</value>
        public const string VALIDITY_STATUS_PAST = "Past";

        /// <summary>
        /// EMDObjects with this status are going to be considered as valid.
        /// </summary>
        /// <value>"Future"</value>
        public const string VALIDITY_STATUS_FUTURE = "Future";

        /// <summary>
        /// EMDObjects with this status hold no information about the validity.
        /// </summary>
        /// <value>"Undefined"</value>
        public const string VALIDITY_STATUS_UNDEFINED = "Undefined";

        /// <summary>
        /// This constant defines the shift when objects starts to be valid or invalid relative to <see cref="DateTime.Now"/>.
        /// </summary>
        /// <remarks>in This is due to a problem where DB comparisons very shortly after the update do not work properly.</remarks>
        /// <value>-2</value>
        public const int VALIDITY_SHIFT = -2;

        /// <summary>
        /// This DateTime is defined as infinity and is used to indicate 'endless' objects or objects with no foreseeable end.
        /// </summary>
        /// <value>2299-12-31T00:00:00</value>
        public static DateTime INFINITY = new DateTime(2299, 12, 31);

        /// <summary>
        /// Prefix of the EMDObject-GUID. Needs to be overridden by concrete implementations.
        /// </summary>
        public virtual String Prefix { get { return "____"; } }

        /// <summary>
        /// GUID of the last Person ("PERS") which modified this EMDObject.
        /// </summary>
        public string Guid_ModifiedBy { get; set; }

        /// <summary>
        /// Comment of the last modification to this EMDObject.
        /// </summary>
        public string ModifyComment { get; set; }

        /// <summary>
        /// Constructs a new EMDObject with just the <see cref="Created"/> set to <see cref="DateTime.Now"/>.
        /// </summary>
        public EMDObject()
        {
            this.Created = DateTime.Now;
        }

        /// <summary>
        /// Constructs a new EMDObject with the given parameters set and the <see cref="ValidityStatus"/> set to <see cref="VALIDITY_STATUS_UNDEFINED"/>.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="created"></param>
        /// <param name="modified"></param>
        public EMDObject(String guid, DateTime created, DateTime? modified)
        {
            this.Guid = guid;
            this.Created = created;
            this.Modified = modified;
            ValidityStatus = VALIDITY_STATUS_UNDEFINED;
        }

        /// <summary>
        /// Date when this entity was created.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Unique ID of this entity unrelated to history.
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// Date when this entity was modified last time.
        /// </summary>
        public DateTime? Modified { get; set; }

        /// <summary>
        /// Date when this entity starts to be valid in respect of the data, not the object.
        /// </summary>
        public DateTime ValidFrom { get; set; }

        /// <summary>
        /// Date when this entity stops to be valid in respect of the data, not the object. If no end-date is known this value is set to <see cref="INFINITY"/>.
        /// </summary>
        public DateTime ValidTo { get; set; }

        /// <summary>
        /// Date when this entity starts to be active in respect of the object, not the data.
        /// </summary>
        public DateTime ActiveFrom { get; set; }

        /// <summary>
        /// Date when this entity stops to be active in respect of the object, not the data. If no end-date is known this value is set to <see cref="INFINITY"/>.
        /// </summary>
        public DateTime ActiveTo { get; set; }

        /// <summary>
        /// Actual validity-status as string in relation to valid-from / valid-to which can be:
        /// <ul>
        ///     <li><see cref="VALIDITY_STATUS_VALID" /></li>
        ///     <li><see cref="VALIDITY_STATUS_PAST"/></li>
        ///     <li><see cref="VALIDITY_STATUS_FUTURE"/></li>
        ///     <li><see cref="VALIDITY_STATUS_UNDEFINED"/></li>
        /// </ul>
        /// </summary>
        public String ValidityStatus { get; set; }

        /// <summary>
        /// Indicates whether an object is active or inactive in respect of the object, not the data.
        /// </summary>
        public bool IsActive
        {
            get
            {
                if (this.ActiveFrom != null && this.ActiveTo != null && this.ActiveFrom <= DateTime.Now && this.ActiveTo >= DateTime.Now)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// The <see cref="Guid"/> of the currently valid record of the record-history of this 'real' object. If <see cref="Guid"/> 
        /// equals <see cref="HistoryGuid"/> this is the currently valid and most recent record of the represented object.
        /// </summary>
        public string HistoryGuid { get; set; }

        /// <summary>
        /// Creates a new GUID with Entity-Prefix.
        /// </summary>
        /// <returns>New GUID with correct Entity-Prefix</returns>
        public string CreateDBGuid()
        {
            String myPrefix = Prefix + "_____";
            myPrefix = myPrefix.Substring(0, 5);
            return myPrefix + System.Guid.NewGuid().ToString().Replace("-", "");
        }

        /// <summary>
        /// Sets this EMDObject as new object. In the process <see cref="Created"/>, <see cref="Modified"/>, <see cref="ValidFrom"/>, <see cref="ValidTo"/>, <see cref="ActiveFrom"/> and <see cref="ActiveTo"/> are reset.
        /// </summary>
        /// <returns> The DateTime used as 'now'</returns>
        public virtual DateTime SetAsNew()
        {
            DateTime stamp = System.DateTime.Now;
            this.Created = stamp;
            this.Modified = stamp;
            this.ValidFrom = stamp.AddSeconds(VALIDITY_SHIFT);
            this.ValidTo = INFINITY;
            this.ActiveFrom = stamp.AddSeconds(VALIDITY_SHIFT);
            this.ActiveTo = INFINITY;
            return stamp;
        }

        /// <summary>
        /// Sets Modified-date to <see cref="DateTime.Now"><c>DateTime.Now</c></see>. Then checks following properties for null:<br />
        /// <ul>
        ///     <li><see cref="Created"/>: if null is set to <see cref="DateTime.Now"/></li>
        ///     <li><see cref="ValidFrom"/>: if null is set to <see cref="DateTime.Now"/></li>
        ///     <li><see cref="ValidTo"/>: if null is set to <see cref="INFINITY"/></li>
        ///     <li><see cref="ActiveFrom"/>: if null is set to <see cref="DateTime.Now"/></li>
        ///     <li><see cref="ActiveTo"/>: if null is set to <see cref="INFINITY"/></li>
        /// </ul>
        /// </summary>
        /// <returns>The number of properties set. Minimum 1 (Modified) and maximum 6</returns>
        public virtual int FillEmptyDates(DateTime? effectDate = null)
        {
            int fillCount = 1;
            DateTime stamp = (effectDate != null ? effectDate.Value : DateTime.Now);
            stamp.AddSeconds(VALIDITY_SHIFT);

            this.Modified = stamp;

            if (this.Created == null)
            {
                this.Created = stamp;
                fillCount++;
            }
            if (this.ValidFrom == default(DateTime))
            {
                this.ValidFrom = stamp;
                fillCount++;
            }
            if (this.ValidTo == default(DateTime))
            {
                this.ValidTo = INFINITY;
                fillCount++;
            }
            if (this.ActiveFrom == default(DateTime))
            {
                this.ActiveFrom = stamp;
                fillCount++;
            }
            if (this.ActiveTo == default(DateTime))
            {
                this.ActiveTo = INFINITY;
                fillCount++;
            }

            return fillCount;
        }

        /// <summary>
        ///  Sets the Dates of this EMDObject to the respective values in <paramref name="oldVersion"/> except for the validity (<see cref="ValidFrom"/> and <see cref="ValidTo"/>) and <see cref="Modified"/>. 
        /// </summary>
        /// <param name="oldVersion">EMDObject representing the old version of this EMDObject.</param>
        /// <param name="touchActiveTo"> If set to false do not let SetAsUpdated function change the ActiveTo date.</param>
        /// <returns>DateTime of the update.</returns>
        public virtual DateTime SetAsUpdated(IEMDObject<T> oldVersion, bool touchActiveTo = true, bool touchActiveFrom = true)
        {
            DateTime stamp = DateTime.Now;
            this.Created = oldVersion.Created;
            this.Modified = stamp;
            this.ValidFrom = stamp.AddSeconds(VALIDITY_SHIFT);
            this.ValidTo = INFINITY;
            if (touchActiveFrom) this.ActiveFrom = oldVersion.ActiveFrom;
            if (touchActiveTo) this.ActiveTo = oldVersion.ActiveTo;

            return stamp;
        }

        /// <summary>
        /// Sets the <see cref="Modified"/> to <see cref="DateTime.Now"/>.
        /// </summary>
        public void SetModified()
        {
            Modified = System.DateTime.Now;
        }

        /// <summary>
        /// Sets the right <see cref="ValidityStatus"/> according to <see cref="ValidFrom"/> and <see cref="ValidTo"/>.
        /// </summary>
        public void SetValidityStatus()
        {
            if (this.ValidFrom == null && this.ValidTo == null)
                return;

            if (this.ValidTo < this.ValidFrom)
                this.ValidityStatus = EMDObject<T>.VALIDITY_STATUS_UNDEFINED;
            else
            {
                if (this.ValidFrom < DateTime.Now)
                {
                    if (this.ValidTo > DateTime.Now)
                        this.ValidityStatus = EMDObject<T>.VALIDITY_STATUS_VALID;
                    else
                        this.ValidityStatus = EMDObject<T>.VALIDITY_STATUS_PAST;
                }
                else
                {
                    if (this.ValidTo > DateTime.Now)
                        this.ValidityStatus = EMDObject<T>.VALIDITY_STATUS_FUTURE;
                    else
                        this.ValidityStatus = EMDObject<T>.VALIDITY_STATUS_UNDEFINED;
                }
            }
        }

        /// <summary>
        /// Invalidate this EMDObject (<see cref="ValidTo"/> is changed).
        /// </summary>
        public void Invalidate()
        {
            DateTime stamp = System.DateTime.Now;
            Modified = stamp;
            ValidTo = stamp.AddSeconds(VALIDITY_SHIFT);
        }

        /// <summary>
        /// Deactivate EMD-object 
        /// </summary>
        public void Deactivate()
        {
            DateTime stamp = System.DateTime.Now;
            ActiveTo = stamp.AddSeconds(VALIDITY_SHIFT);
        }

        /// <summary>
        /// Invalidate the object on the given date.
        /// </summary>
        /// <param name="t">Invalidation-Date. Can be in future or past.</param>
        public void InvalidateBy(DateTime t)
        {
            DateTime stamp = t;
            Modified = stamp;
            ValidTo = stamp.AddSeconds(VALIDITY_SHIFT);
        }

        /// <summary>
        /// Deactivate the object by the given Date.
        /// </summary>
        /// <param name="t">Deactivation-Date. Can be in future or past. Mustn't be <see langword="null"/>.</param>
        public void DeactivateBy(DateTime t)
        {
            ActiveTo = t.AddSeconds(VALIDITY_SHIFT);
        }

        /// <summary>
        /// Sets <see cref="ActiveTo"/> to <see cref="INFINITY"/> if and only if this EMDObject is valid.
        /// </summary>
        public void Activate()
        {
            if (this.ValidityStatus == EMDObject<T>.VALIDITY_STATUS_VALID)
            {
                this.ActiveTo = INFINITY;
            }
            else
            {
                // TODO do nothing? throw exception ?? bit much i think
            }
        }

        /// <summary>
        /// Set the information about the last modification of this EMDObject.
        /// </summary>
        /// <param name="guid_ModifiedBy">GUID of the Person ("PERS") which modified this EMDObject</param>
        /// <param name="modifyComment">Comment about the changes in the last modification</param>
        public void SetModifiedBy(string guid_ModifiedBy, string modifyComment)
        {
            this.Guid_ModifiedBy = guid_ModifiedBy;
            this.ModifyComment = modifyComment;
        }

        /// <summary>
        /// Returns the first 4 letters of the GUID (=Prefix) in lower case
        /// </summary>
        /// <param name="guid">GUID with prefix. Mustn't be <see langword="null"/>.</param>
        /// <returns>
        /// First four letters of the given <see cref="string"/> in lower case. The method does not guarantee that this is a valid prefix as it does not check whether the 
        /// given <see cref="string"/> is a valid GUID with prefix.
        /// </returns>
        public static string GetPrefix(string guid)
        {
            return guid.Substring(0, 4).ToLower();
        }

        private static Regex emdGuidRegex;

        /// <summary>
        /// Checks a Guid for validity
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static bool IsEMDGuid(string guid)
        {
            if (string.IsNullOrWhiteSpace(guid))
            {
                return false;
            }

            if (emdGuidRegex == null)
            {
                emdGuidRegex = new Regex("[A-Z]{4}_[0-9a-f]{32}");
            }

            return emdGuidRegex.IsMatch(guid);
        }
    }
}