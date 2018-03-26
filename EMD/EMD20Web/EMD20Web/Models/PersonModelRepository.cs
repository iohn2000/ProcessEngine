using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class PersonModelRepository
    {
        public static PersonModelRepository Repository = new PersonModelRepository();

        public List<PersonModel> Persons
        {
            get
            {
                List<PersonModel> result = (List<PersonModel>)HttpContext.Current.Session["Persons"];

                if (result == null)
                {
                    result = new List<PersonModel>();
                    HttpContext.Current.Session["Persons"] = result;
                }

                return result;
            }
        }

        public List<PersonModel> GetAll()
        {
            return Persons;
        }

        public PersonModel GetById(int Id)
        {
            return (from per in Persons where per.Guid == Id.ToString() select per).FirstOrDefault();
        }

        public void Insert(PersonModel person)
        {
            var last = Persons.LastOrDefault();
            if (last != null)
            {
                person.Guid = (Convert.ToInt32(last.Guid) + 1).ToString();
            }
            else
            {
                person.Guid = "1";
            }

            Persons.Add(person);

            HttpContext.Current.Session["Persons"] = Persons;
        }

        public void Update(PersonModel person)
        {
            var old = Persons.Where(a => a.Guid == person.Guid).FirstOrDefault();

            if (old != null)
            {
                //old.Created = person.Created;
                //old.FirstName = person.FirstName;
                //old.FirstnameDisplay = person.FirstnameDisplay;
                //old.FirstnameReduced = person.FirstnameReduced;
                //old.Gender = person.Gender;
                //old.FamilyName = person.FamilyName;
                //old.SurnameDisplay = person.SurnameDisplay;
                //old.SurnameReduced = person.SurnameReduced;
                //old.Title = person.Title;
                //old.TitleSuffix= person.TitleSuffix;
            }

            HttpContext.Current.Session["Persons"] = Persons;
        }

        public void Delete(PersonModel person)
        {
            Persons.Remove( Persons.Single(ai => ai.Guid == person.Guid));

            HttpContext.Current.Session["Persons"] = Persons;
        }
    }
}