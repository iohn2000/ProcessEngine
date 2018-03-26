using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kapsch.IS.EMD.EMD20Web.Models
{
    public class OldRoleModelRepository
    {
        public static OldRoleModelRepository Repository = new OldRoleModelRepository();

        public List<OldRoleModel> Roles
        {
            get
            {
                List<OldRoleModel> result = (List<OldRoleModel>)HttpContext.Current.Session["Roles"];

                if (result == null)
                {
                    List<OldRoleModel> listRoles = new List<OldRoleModel>();
                    for (int i = 1; i < 15; i++)
                    {
                        int enterprise = 1;
                        string color = "black";
                        if (i < 3 || i == 7 || i == 12)
                        { 
                            enterprise = 2;
                            color = "red";
                        }
                        else if (i < 9 || i == 14 || i == 15)
                        {
                            enterprise = 2;
                            color = "green";
                        }
                        int location = 1;

                        OldRoleModel role = new OldRoleModel(i.ToString(),"Role_" + i.ToString(),"RoleDescription_" + i.ToString(), enterprise,location,"01001110",color);
                        listRoles.Add(role);
                    }
                    listRoles = (from r in listRoles orderby r.Name ascending select r).ToList();
                    HttpContext.Current.Session["Roles"] = result = listRoles;
                }

                return result;
            }

            
        }

        public List<OldRoleModel> GetAll()
        {
            return Roles;
        }

        public void Insert(OldRoleModel role)
        {
            var last = Roles.LastOrDefault();
            if (last != null)
            {
                role.Id = (Convert.ToInt32(last.Id) + 1).ToString();
            }
            else
            {
                role.Id = "1";
            }

            Roles.Add(role);

            HttpContext.Current.Session["Roles"] = Roles;
        }

        public void Update(OldRoleModel role)
        {
            var old = Roles.Where(a => a.Id == role.Id).FirstOrDefault();

            if (old != null)
            {
                old.CompanyId = role.CompanyId;
                old.Description = role.Description;
                old.Flags = role.Flags;
                old.LocationId = role.LocationId;
                old.Name = role.Name;
            }

            HttpContext.Current.Session["Roles"] = Roles;
        }

        public void Delete(OldRoleModel role)
        {
            Roles.Remove( Roles.Single(ai => ai.Id == role.Id));

            HttpContext.Current.Session["Roles"] = Roles;
        }
    }
}