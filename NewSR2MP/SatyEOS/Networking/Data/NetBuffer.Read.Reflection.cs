using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NewSR2MP.Data
{
    public partial class NetBuffer
    {
        /// <summary>
		/// Reads all public and private declared instance fields of the object in alphabetical order using reflection
		/// </summary>
		public void ReadAllFields(object target)
        {
            ReadAllFields(target, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        /// <summary>
        /// Reads all fields with the specified binding of the object in alphabetical order using reflection
        /// </summary>
        public void ReadAllFields(object target, BindingFlags flags)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            Type tp = target.GetType();

            FieldInfo[] fields = tp.GetFields(flags);
            NetUtility.SortMembersList(fields);

            foreach (FieldInfo fi in fields)
            {
                object value;

                // find read method
                MethodInfo readMethod;
                if (s_readMethods.TryGetValue(fi.FieldType, out readMethod))
                {
                    // read value
                    value = readMethod.Invoke(this, null);

                    // set the value
                    fi.SetValue(target, value);
                }
            }
        }

        /// <summary>
        /// Reads all public and private declared instance fields of the object in alphabetical order using reflection
        /// </summary>
        public void ReadAllProperties(object target)
        {
            ReadAllProperties(target, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        /// <summary>
        /// Reads all fields with the specified binding of the object in alphabetical order using reflection
        /// </summary>
        public void ReadAllProperties(object target, BindingFlags flags)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            Type tp = target.GetType();

            PropertyInfo[] fields = tp.GetProperties(flags);
            NetUtility.SortMembersList(fields);
            foreach (PropertyInfo fi in fields)
            {
                object value;

                // find read method
                MethodInfo readMethod;
                if (s_readMethods.TryGetValue(fi.PropertyType, out readMethod))
                {
                    // read value
                    value = readMethod.Invoke(this, null);

                    // set the value
                    var setMethod = fi.GetSetMethod();
                    if (setMethod != null)
                        setMethod.Invoke(target, new object[] { value });
                }
            }
        }
    }
}
