using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kapsch.IS.EDP.Core.Entities;
using Kapsch.IS.Util.ErrorHandling;


namespace Kapsch.IS.EDP.Core.Framework
{
    public sealed class EntityPrefix
    {
        private static volatile EntityPrefix instance = new EntityPrefix();
        private static object syncRoot = new Object();

        private Hashtable PrefixList;
        private String entityNamespace = "Kapsch.IS.EDP.Core.Entities";

        private EntityPrefix()
        {
            PrefixList = new Hashtable();
            List<Type> typelist = GetCoreTypeList();
            foreach (Type t in typelist)
            {
                try
                {
                    if (!t.IsAbstract && !t.ContainsGenericParameters && t.BaseType.FullName != "System.Attribute")
                    {
                        var entity = Activator.CreateInstance(t);
                        String prefix = entity.GetType().GetProperty("Prefix")?.GetValue(entity)?.ToString();
                        if (!string.IsNullOrEmpty(prefix))
                        {
                            PrefixList.Add(prefix, t);
                        }
                    }
                }
                catch (Exception)
                {
                    // do nothing; wrong kind of Object
                }

            }

        }

        public static List<Type> GetCoreTypeList()
        {
            return GetTypesInNamespace(Assembly.GetExecutingAssembly(), "Kapsch.IS.EDP.Core.Entities").ToList();
        }

        public static EntityPrefix Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new EntityPrefix();
                    }
                }

                return instance;
            }
        }

        internal bool isPrefix(string innerresult)
        {
            //TODO implementation
            return true;
        }

        public object GetPropertyByNameandGuid(string guid, string propertyName, out Type propType)
        {
            Type entityType = GetTypeFromGuid(guid);

            Type genericType = typeof(IEMDObject<>);
            Type specificType = genericType.MakeGenericType(entityType);

            MethodInfo method = this.GetType().GetMethod("GetInstanceFromGuid");
            MethodInfo generic = method.MakeGenericMethod(entityType);

            var obj = generic.Invoke(this, new object[] { guid });

            Type objType = obj.GetType();
            PropertyInfo pidProperty = objType.GetProperty(propertyName);
            propType = pidProperty.PropertyType;

            var result = pidProperty.GetValue(obj);
            return result;
        }

        /// <summary>
        /// invokes EntityFunction
        /// </summary>
        /// <param name="function"></param>
        /// <param name="param"></param>
        /// <param name="entityGuid"></param>
        /// <returns></returns>
        public String GetGuidByFunctionCall(String function, String param, String entityGuid)
        {
            Type entityType = GetTypeFromGuid(entityGuid);
             
            String functionClassName = entityNamespace+".EntityFunctions." + function;
            Type functionType = GetTypeFromAssemblyByName(functionClassName);

            object entityFunctionObject = Activator.CreateInstance(functionType);

            MethodInfo method = functionType.GetMethod("Call");

            object resultObject = method.Invoke(entityFunctionObject, new object[] { entityGuid, param });

            String result = (String)resultObject.GetType().GetRuntimeProperty("Result").GetValue(resultObject);
            return result;
        }

        public Type GetTypeFromGuid(String guid)
        {
            return (Type)PrefixList[GetPrefixFromGuid(guid)];
        }

        public Type GetTypeFromPrefix(String prefix)
        {
            return (Type)PrefixList[prefix];
        }

        public String CallHandlerMethodFromGuid(String guid, String methodName, String methodParam)
        {

            Type t = (Type)PrefixList[GetPrefixFromGuid(guid)];
            String typeName = t.ToString();
            String handlerName = entityNamespace + "." + typeName.Substring(entityNamespace.Length + 4) + "Handler";

            Type handler = GetTypeFromAssemblyByName(handlerName);

            EMDBaseObjectHandler handlerInstance = (EMDBaseObjectHandler)Activator.CreateInstance(handler);

            Type genericType = typeof(IEMDObject<>);
            Type specificType = genericType.MakeGenericType(t);

            try
            {
                MethodInfo method = handlerInstance.GetType().GetMethod(methodName);

                //MethodInfo generic = method.MakeGenericMethod(t);
                var result = method.Invoke(handlerInstance, new object[] { guid, methodParam });
                return result.ToString();

            }
            catch (Exception exc)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Error while trying to invoke given Method " + methodName + "(" + guid + ", " + methodParam + ")", exc);
            }
        }

        public T GetInstanceFromGuid<T>(String guid)
        {
            String methodName = "GetObject";

            Type t = (Type)PrefixList[GetPrefixFromGuid(guid)];
            String typeName = t.ToString();
            String handlerName = entityNamespace + "." + typeName.Substring(entityNamespace.Length + 4) + "Handler";

            Type handler = GetTypeFromAssemblyByName(handlerName);

            EMDBaseObjectHandler handlerInstance = (EMDBaseObjectHandler)Activator.CreateInstance(handler);

            Type genericType = typeof(IEMDObject<>);
            Type specificType = genericType.MakeGenericType(t);

            try
            {
                MethodInfo method = handlerInstance.GetType().GetMethod(methodName);

                MethodInfo generic = method.MakeGenericMethod(t);
                var result = generic.Invoke(handlerInstance, new object[] { guid });
                return (T)result;
            }
            catch (Exception exc)
            {
                throw new BaseException(ErrorCodeHandler.E_EDP_BUSINESS_LOGIK, "Error while trying to invoke GetObject for guid " + guid, exc);
            }
        }

        public Type GetTypeFromAssemblyByName(String name)
        {
            Type t = Assembly.GetExecutingAssembly().GetType(name);
            return t;
        }

        public string GetPrefixFromGuid(String guid)
        {
            return guid.Substring(0, 4);
        }

        private static Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return assembly.GetTypes().Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)).ToArray();
        }

    }
}
