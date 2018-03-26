using Kapsch.IS.EDP.Core.Framework.Exceptions;
using Kapsch.IS.EDP.Core.Logic.Interface;
using Kapsch.IS.EDP.Core.Logic.Mockup;
using Kapsch.IS.Util.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Logic
{
    public enum Managertype
    {
        NotSet,
        Database,
        Mockup
    }

    /// <summary>
    /// User the Manager to switch between concrete implementation of managers
    /// used for mockups
    /// </summary>
    public class Manager
    {
        /// <summary>
        /// never set this private static type directly
        /// use always the Method ManagerType
        /// </summary>
        private static Managertype managertype = Managertype.Database;

        /// <summary>
        /// The manager class must be initialized once before your can use it
        /// </summary>
        /// <param name="managerType"></param>
        /// <returns></returns>
        public static void Initialize(Managertype managerType)
        {
            Managertype = managertype;
        }


        internal static Managertype Managertype
        {
            get
            {
                if (managertype == Managertype.NotSet)
                {
                    throw new NotInitializedException(ErrorCodeHandler.E_CONFIG_GENERAL, "Call the Initialze Method ONCE before using the manager.");
                }
                return managertype;
            }
            set
            {
                managertype = value;
            }
        }

        public static IUserManager UserManager
        {
            get
            {
                IUserManager manager;

                switch (managertype)
                {
                    case Managertype.Mockup:
                        manager = new MockupUserManager();
                        break;
                    default:
                        manager = new UserManager();
                        break;
                }

                return manager;
            }
        }

        public static IUserDomainManager UserDomainManager
        {
            get
            {
                IUserDomainManager manager;

                switch (managertype)
                {
                    case Managertype.Mockup:
                        throw new NotImplementedException();

                    default:
                        manager = new UserDomainManager();
                        break;
                }

                return manager;
            }
        }

        public static IAccountManager AccountManager
        {
            get
            {
                IAccountManager manager;

                switch (managertype)
                {
                    case Managertype.Mockup:
                        throw new NotImplementedException();

                    default:
                        manager = new AccountManager();
                        break;
                }

                return manager;
            }
        }

        public static IEnterpriseManager EnterpriseManager
        {
            get
            {
                IEnterpriseManager manager;

                switch (managertype)
                {
                    case Managertype.Mockup:
                        throw new NotImplementedException();

                    default:
                        manager = new EnterpriseManager();
                        break;
                }

                return manager;
            }
        }

        public static IOrgUnitManager OrgUnitManager
        {
            get
            {
                IOrgUnitManager manager;

                switch (managertype)
                {
                    case Managertype.Mockup:
                        throw new NotImplementedException();

                    default:
                        manager = new OrgUnitManager();
                        break;
                }

                return manager;
            }
        }

        public static ILocationManager LocationManager
        {
            get
            {
                ILocationManager manager;

                switch (managertype)
                {
                    case Managertype.Mockup:
                        throw new NotImplementedException();

                    default:
                        manager = new LocationManager();
                        break;
                }

                return manager;
            }
        }

        public static ICountryManagercs CountryManager
        {
            get
            {
                ICountryManagercs manager;

                switch (managertype)
                {
                    case Managertype.Mockup:
                        throw new NotImplementedException();

                    default:
                        manager = new CountryManager();
                        break;
                }

                return manager;
            }
        }

        public static IPersonManager PersonManager
        {
            get
            {
                IPersonManager manager;

                switch (managertype)
                {
                    case Managertype.Mockup:
                        throw new NotImplementedException();

                    default:
                        manager = new PersonManager();
                        break;
                }

                return manager;
            }
        }

        public static ISecurityActionManager SecurityActionManager
        {
            get
            {
                ISecurityActionManager manager;

                switch (managertype)
                {
                    case Managertype.Mockup:
                        throw new NotImplementedException();

                    default:
                        manager = new SecurityActionManager();
                        break;
                }

                return manager;
            }
        }

        public static IRoleManager RoleManager
        {
            get
            {
                IRoleManager manager;

                switch (managertype)
                {
                    case Managertype.Mockup:
                        throw new NotImplementedException();

                    default:
                        manager = new RoleManager();
                        break;
                }

                return manager;
            }
        }


        public static IGroupManager GroupManager
        {
            get
            {
                IGroupManager manager;

                switch (managertype)
                {
                    case Managertype.Mockup:
                        throw new NotImplementedException();

                    default:
                        manager = new GroupManager();
                        break;
                }

                return manager;
            }
        }

        public static IProcessEntityManager ProcessEntityManager
        {
            get
            {
                IProcessEntityManager manager;

                switch (managertype)
                {
                    case Managertype.Mockup:
                        throw new NotImplementedException();

                    default:
                        manager = new ProcessEntityManager();
                        break;
                }

                return manager;
            }
        }
    }
}
