using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NewSR2MP.Networking.Data
{
    public partial class NetBuffer
    {
        /// <summary>
		/// Writes all public and private declared instance fields of the object in alphabetical order using reflection
		/// </summary>
		public void WriteAllFields(object ob)
        {
            WriteAllFields(ob, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        /// <summary>
        /// Writes all fields with specified binding in alphabetical order using reflection
        /// </summary>
        public void WriteAllFields(object ob, BindingFlags flags)
        {
            if (ob == null)
                return;
            Type tp = ob.GetType();

            FieldInfo[] fields = tp.GetFields(flags);
            NetUtility.SortMembersList(fields);

            foreach (FieldInfo fi in fields)
            {
                object value = fi.GetValue(ob);

                // find the appropriate Write method
                MethodInfo writeMethod;
                if (s_writeMethods.TryGetValue(fi.FieldType, out writeMethod))
                    writeMethod.Invoke(this, new object[] { value });
                else
                    throw new NetException("Failed to find write method for type " + fi.FieldType);
            }
        }

        /// <summary>
        /// Writes all public and private declared instance properties of the object in alphabetical order using reflection
        /// </summary>
        public void WriteAllProperties(object ob)
        {
            WriteAllProperties(ob, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        /// <summary>
        /// Writes all properties with specified binding in alphabetical order using reflection
        /// </summary>
        public void WriteAllProperties(object ob, BindingFlags flags)
        {
            if (ob == null)
                return;
            Type tp = ob.GetType();

            PropertyInfo[] fields = tp.GetProperties(flags);
            NetUtility.SortMembersList(fields);

            foreach (PropertyInfo fi in fields)
            {
                MethodInfo getMethod = fi.GetGetMethod();
                if (getMethod != null)
                {
                    object value = getMethod.Invoke(ob, null);

                    // find the appropriate Write method
                    MethodInfo writeMethod;
                    if (s_writeMethods.TryGetValue(fi.PropertyType, out writeMethod))
                        writeMethod.Invoke(this, new object[] { value });
                }
            }
        }
    }
}
