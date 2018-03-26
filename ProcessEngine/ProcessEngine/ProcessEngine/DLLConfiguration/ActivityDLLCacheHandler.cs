using Kapsch.IS.Util.ErrorHandling;
using Kapsch.IS.Util.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Kapsch.IS.ProcessEngine.DLLConfiguration
{
    /// <summary>
    /// loads all activity dlls into cache
    /// </summary>
    public class ActivityDLLCacheHandler
    {
        private IEDPLogger logger = EDPLogger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// fill the activity cache (dlls) with all activities from all DLLs
        /// throws a BaseException 
        /// </summary>
        /// <param name="dllConfig"></param>
        /// <returns></returns>
        public Tuple<Dictionary<string, object>, Dictionary<string, MethodInfo>> ReloadActivityCache(ActivityDLLConfigurationSection dllConfig)
        {
            Dictionary<string, object> dllCache = new Dictionary<string, object>();
            Dictionary<string, MethodInfo> methodCache = new Dictionary<string, MethodInfo>();

            object activityObject = null;
            MethodInfo activityMethod = null;

            string fullDLLPath = "";
            string baseDir = ConfigurationManager.AppSettings["DLLStepsBaseDir"];

            foreach (ActivityDLLElement item in dllConfig.ActivityDLLs)
            {
                // build path
                if (!Path.IsPathRooted(item.DLLPath))
                {
                    if (!string.IsNullOrWhiteSpace(baseDir))
                        fullDLLPath = Path.Combine(baseDir, item.DLLPath);
                    else
                    {
                        string exePath = Path.GetDirectoryName((new Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath);
                        fullDLLPath = Path.Combine(exePath, item.DLLPath);
                    }
                }


                // foreach DLL namespace is build from Configured base namespace + every class that implements IProcessStep interface
                // Kapsch.IS.EDP.WFActivity + EmailActivity

                try
                {
                    Assembly activityAssembly = Assembly.LoadFrom(fullDLLPath);
                    var it = typeof(IProcessStep);
                    foreach (Type acType in activityAssembly.GetTypes().Where(it.IsAssignableFrom).ToList())
                    {
                        try
                        {
                            if (!acType.IsAbstract)
                            {
                                activityObject = Activator.CreateInstance(acType);
                                activityMethod = acType.GetMethod("Execute");
                                dllCache[acType.FullName] = activityObject;
                                methodCache[acType.FullName] = activityMethod;
                            }
                        }
                        catch (MissingMethodException mme)
                        {
                            String msg = mme.Message;
                            logger.Error(msg, mme);
                            throw new Exception("Missing Method in :" + acType + "; " + msg);
                        }
                    }
                }
                catch (ReflectionTypeLoadException rtlex)
                {
                    String msg = "";
                    foreach (Exception e in rtlex.LoaderExceptions)
                    {
                        msg += (e.Message + "Inner ex:" + e.InnerException + " / ");
                    }
                    logger.Error(msg);
                    throw new Exception(msg);
                }
                catch (Exception ex)
                {
                    string errMsg = string.Format("Error loading activity DLL : '{0}' with exception message {1}", fullDLLPath, ex.Message);
                    logger.Error(errMsg, ex);
                    var bEx = new BaseException(ErrorCodeHandler.E_WF_PROCESSENGINE, errMsg, ex);
                    throw bEx;
                }
            }

            return Tuple.Create(dllCache, methodCache);
        }

    }
}
