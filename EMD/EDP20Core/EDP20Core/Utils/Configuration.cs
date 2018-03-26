using Kapsch.IS.Util.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Utils
{
    public class Configuration
    {
        protected static internal IISLogger logger = ISLogger.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        private static string testSystemName;
        private static bool? isTestSystem;
        private static string testMailReceiver;
        private static string emailSenderAddress;

        private static bool? isCachingActive;
        private static int? cachingTimeInMinutes;
        private static int? cachingTimeInMinutes_Extended;
        private static List<string> cachingExtendedEntities;

        private static bool? doCacheSecurityUser;

        private static Guid? modificationID;

        public static string TESTSYSTEMNAME
        {
            get
            {
                if (testSystemName == null)
                {
                    testSystemName = System.Configuration.ConfigurationManager.AppSettings["EDP20Core.Optional.TestSystemName"];
                    if (testSystemName == null)
                    {
                        testSystemName = string.Empty;
                    }
                }
                return testSystemName;
            }
        }

        public static string EMAILSENDERADDRESS
        {
            get
            {
                if (emailSenderAddress == null)
                {
                    emailSenderAddress = System.Configuration.ConfigurationManager.AppSettings["EDP20Core.EMailSenderAddress"];

                }
                return emailSenderAddress;
            }
        }

        public static bool ISTESTSYSTEM
        {
            get
            {
                if (isTestSystem == null)
                {

                    isTestSystem = !string.IsNullOrEmpty(TESTSYSTEMNAME);
                }
                return isTestSystem.Value;
            }
        }

        public static string TESTMAILRECEIVER
        {
            get
            {
                if (testMailReceiver == null)
                {
                    testMailReceiver = System.Configuration.ConfigurationManager.AppSettings["EDP20Core.Optional.TestMailReceiver"];
                    if (testMailReceiver == null)
                    {
                        testMailReceiver = string.Empty;
                    }
                }
                return testMailReceiver;
            }
        }


        /// <summary>
        /// The modification ID is changed on any insert or update, to make it possible recognizing DB changes
        /// </summary>
        public static Guid MODIFICATION_ID
        {
            get
            {
                if (modificationID == null)
                {
                    modificationID = Guid.NewGuid();
                }

                return modificationID.Value;
            }
            set
            {
                modificationID = value;
            }
        }


        public static bool ISCACHINGACTIVE
        {
            get
            {
                if (isCachingActive == null)
                {
                    string cachingActiveString = System.Configuration.ConfigurationManager.AppSettings["EDP20Core.IsCachingActive"];
                    if (!string.IsNullOrEmpty(cachingActiveString))
                    {
                        try
                        {
                            isCachingActive = bool.Parse(cachingActiveString);
                        }
                        catch (Exception)
                        {
                            isCachingActive = false;
                        }

                        logger.Info(string.Format("is Caching active: {0}", isCachingActive));
                    }
                    else
                    {
                        isCachingActive = false;

                        logger.Info(string.Format("is Caching active: {0}", isCachingActive));
                    }
                }
                return isCachingActive.Value;
            }
        }

        public static int CACHINGTIMEINMINUTES
        {
            get
            {
                if (cachingTimeInMinutes == null)
                {
                    string cachingActiveString = System.Configuration.ConfigurationManager.AppSettings["EDP20Core.CachingTimeInMinutes"];
                    if (!string.IsNullOrEmpty(cachingActiveString))
                    {
                        try
                        {
                            cachingTimeInMinutes = int.Parse(cachingActiveString);
                        }
                        catch (Exception)
                        {
                            cachingTimeInMinutes = 5;
                        }
                        logger.Info(string.Format("Set Caching Time in minutes: {0}", cachingTimeInMinutes));
                    }
                    else
                    {
                        cachingTimeInMinutes = 5;
                        logger.Info(string.Format("Set Caching Time in minutes: {0}", cachingTimeInMinutes));
                    }
                }
                return cachingTimeInMinutes.Value;
            }
        }

        public static int CACHINGTIMEINMINUTES_EXTENDED
        {
            get
            {
                if (cachingTimeInMinutes_Extended == null)
                {
                    string cachingActiveString = System.Configuration.ConfigurationManager.AppSettings["EDP20Core.CachingTimeInMinutesExtended"];
                    if (!string.IsNullOrEmpty(cachingActiveString))
                    {
                        try
                        {
                            cachingTimeInMinutes_Extended = int.Parse(cachingActiveString);
                        }
                        catch (Exception)
                        {
                            cachingTimeInMinutes_Extended = 5;
                        }
                        logger.Info(string.Format("Set Caching Time in minutes: {0}", cachingTimeInMinutes_Extended));
                    }
                    else
                    {
                        cachingTimeInMinutes_Extended = 5;
                        logger.Info(string.Format("Set Caching Time in minutes: {0}", cachingTimeInMinutes_Extended));
                    }
                }
                return cachingTimeInMinutes_Extended.Value;
            }
        }

        public static List<string> CACHING_EXTENDED_ENTITIES
        {
            get
            {
                if (cachingExtendedEntities == null)
                {
                    string cachingExtendedEntitiesString = System.Configuration.ConfigurationManager.AppSettings["EDP20Core.CachingExtendedEntities"];
                    if (!string.IsNullOrEmpty(cachingExtendedEntitiesString))
                    {
                        try
                        {
                            cachingExtendedEntities = cachingExtendedEntitiesString.Split(',').ToList();


                        }
                        catch (Exception)
                        {
                            cachingExtendedEntities = new List<string>();
                        }
                        logger.Info(string.Format("Set Caching Time in minutes: {0}", cachingTimeInMinutes_Extended));
                    }
                    else
                    {
                        cachingExtendedEntities = new List<string>();
                        logger.Info(string.Format("Set Caching Time in minutes: {0}", cachingTimeInMinutes_Extended));
                    }
                }
                return cachingExtendedEntities;
            }
        }

        public static bool DOCACHESECURITYUSER
        {
            get
            {
                if (doCacheSecurityUser == null)
                {
                    string cachingActiveString = System.Configuration.ConfigurationManager.AppSettings["EDP20Core.DoCacheSecurityUser"];
                    if (!string.IsNullOrEmpty(cachingActiveString))
                    {
                        try
                        {
                            doCacheSecurityUser = bool.Parse(cachingActiveString);
                        }
                        catch (Exception)
                        {
                            doCacheSecurityUser = false;
                        }

                        logger.Info(string.Format("is Caching active: {0}", doCacheSecurityUser));
                    }
                    else
                    {
                        doCacheSecurityUser = false;

                        logger.Info(string.Format("is Caching active: {0}", doCacheSecurityUser));
                    }
                }
                return doCacheSecurityUser.Value;
            }
        }

        /// <summary>
        /// Checks whether an entity contains the extended fields.
        /// </summary>
        /// <param name="entityName">Name of the entity to check</param>
        /// <returns><see langword="true"/> if and only if the entity with the <paramref name="entityName"/> is an extended entity, otherwise returns <see langword="false"/>.</returns>
        public static bool IsExtendedEntity(string entityName)
        {
            bool isExtendedEntity = false;

            if (Configuration.CACHING_EXTENDED_ENTITIES.Count > 0)
            {
                if (Configuration.CACHING_EXTENDED_ENTITIES.Contains(entityName, StringComparer.InvariantCultureIgnoreCase))
                {
                    isExtendedEntity = true;
                }
            }

            return isExtendedEntity;
        }
    }
}
