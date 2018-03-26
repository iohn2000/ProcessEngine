using Kapsch.IS.Util.ReflectionHelper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kapsch.IS.EDP.Core.Utils
{
    /// <summary>
    /// Provides instances of <see cref="IPropertyCopier{S, T}"/> for the given types.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public static class PropertyCopierProvider
    {

        private static ConcurrentDictionary<Tuple<Type, Type>, dynamic> copiers = new ConcurrentDictionary<Tuple<Type, Type>, dynamic>();

        /// <summary>
        /// Provides an instance of <see cref="IPropertyCopier{S, T}"/> for the given types <typeparamref name="S"/> and <typeparamref name="T"/>. If the type-combination was already requested
        /// once reuses the instance, else creates a new one.
        /// </summary>
        /// <typeparam name="S">Source-type of for the copier.</typeparam>
        /// <typeparam name="T">Target-type of for the copier.</typeparam>
        /// <returns>An instance of <see cref="IPropertyCopier{S, T}"/> with the given types. Either new or reused.</returns>
        public static IPropertyCopier<S, T> Request<S, T>()
        {
            Tuple<Type, Type> key = Tuple.Create(typeof(S), typeof(T));
            if (!copiers.ContainsKey(key))
            {
                IPropertyCopier<S, T> copier = new FastPropertyCopier<S, T>();
                return copiers.GetOrAdd(key, copier);
            }
            return copiers[key];
        }

        /// <summary>
        /// Quick way to copy all shared (have the same name) properties of an object to another object.
        /// </summary>
        /// <typeparam name="S">Type of the source-object.</typeparam>
        /// <typeparam name="T">Type of the target-object.</typeparam>
        /// <param name="source">Source-object. Mustn't be <see langword="null"/>.</param>
        /// <param name="target">Target-object. Mustn't be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Is thrown if one of the arguments is <see langword="null"/>.</exception>
        /// <overloads>A quick and convenient way to copy properties via reflection to another object.</overloads>
        public static void QuickCopy<S, T>(S source, ref T target)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (target == null) throw new ArgumentNullException("target");

            IPropertyCopier<S, T> cp = Request<S, T>();
            cp.CopyAll(source, ref target);
        }

        /// <summary>
        /// Quick way to copy the property with the given name <paramref name="propName"/> from the <paramref name="source"/> to the <paramref name="target"/>.
        /// </summary>
        /// <typeparam name="S">Type of the source-object.</typeparam>
        /// <typeparam name="T">Type of the target-object.</typeparam>
        /// <param name="source">Source-object. Mustn't be <see langword="null"/>.</param>
        /// <param name="target">Target-object. Mustn't be <see langword="null"/>.</param>
        /// <param name="propName">Name of the property to copy. Mustn't be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Is thrown if one of the arguments is <see langword="null"/>.</exception>
        public static void QuickCopy<S, T>(S source, ref T target, string propName)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (target == null) throw new ArgumentNullException("target");
            if (propName == null) throw new ArgumentNullException("propName");

            IPropertyCopier<S, T> cp = Request<S, T>();
            cp.CopyByName(source, ref target, propName);
        }
    }
}
