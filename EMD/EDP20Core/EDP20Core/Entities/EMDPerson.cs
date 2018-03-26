using Kapsch.IS.EDP.Core.Entities;
using System.Collections.Generic;
using System;
using Kapsch.IS.Util.Strings;
using Kapsch.IS.EDP.Core.DB;

namespace Kapsch.IS.EDP.Core.Entities
{

    public class EMDPerson : EMDObject<EMDPerson>
    {
        public int P_ID { get; set; }

        public string FamilyName { get; set; }

        public string FirstName { get; set; }

        public string Synonyms { get; set; }

        public string Sex { get; set; }

        public string DegreePrefix { get; set; }

        public string DegreeSuffix { get; set; }

        public string C128_FamilyName { get; set; }

        public string C128_FirstName { get; set; }

        public string C128_DegreePrefix { get; set; }

        public string C128_DegreeSuffix { get; set; }

        /// <summary>
        /// Fills out the fields C128_FamilyName, C128_FirstName, C128_DegreePrefix and C128_DegreeSuffix with reduced versions of 
        /// FamilyName, FirstName, DegreePrefix and DegreeSuffix. (These mustn't be null)
        /// </summary>
        /// <param name="pers"></param>
        /// <returns></returns>
        public void generateC128Strings()
        {
            try
            {
                //Reduce Familyname
                string temp = StringUtils.TranslateGermanMutationCharacters(this.FamilyName);
                this.C128_FamilyName = StringUtils.TranslateSpecialCharacters(temp);

                //Reduce Firstname
                temp = StringUtils.TranslateGermanMutationCharacters(this.FirstName);
                this.C128_FirstName = StringUtils.TranslateSpecialCharacters(temp);

                //Reduce DegreePrefix
                temp = StringUtils.TranslateGermanMutationCharacters(this.DegreePrefix);
                this.C128_DegreePrefix = StringUtils.TranslateSpecialCharacters(temp);

                //Reduce DegreeSuffix
                temp = StringUtils.TranslateGermanMutationCharacters(this.DegreeSuffix);
                this.C128_DegreeSuffix = StringUtils.TranslateSpecialCharacters(temp);
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("One ore more of the following fields is null: FamilyName, FirstName, DegreePrefix or DegreeSuffix.", ex);
            }
        }

        public string UserID { get; set; }

        public string MainMail { get; set; }

        public string UnixID { get; set; }

        //public bool VisiblePhone { get; set; }

        public string InsBY { get; set; }

        public DateTime? UpdDT { get; set; }

        public string UpdBY { get; set; }

        public DateTime? DelDT { get; set; }

        public string DelBY { get; set; }

        public string Language { get; set; }

        //public bool PictureVisible { get; set; }

        public string Display_FamilyName { get; set; }

        public string Display_FirstName { get; set; }

        //public bool AD_Picture { get; set; }

        public DateTime? AD_Picture_UpdDT { get; set; }

        public string USER_GUID { get; set; }

        public override String Prefix { get { return "PERS"; } }

        public EMDPerson(string guid, DateTime created, DateTime? modified) :
            base(guid, created, modified)
        { }

        public EMDPerson()
        { }

        public static string GetDisplayFullName(EMDPerson person)
        {
            return string.Format("{0} {1}", person.Display_FamilyName, person.Display_FirstName);
        }
    }

    public class PersonSex
    {
        public static string MALE = "M";
        public static string FEMALE = "F";
        public static string NEUTRAL = "N";
    }


}