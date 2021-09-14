using System;
using System.Linq;
using System.Reflection;

#if !NET_4_6
public static class FieldInfoExtensions
{
    public static T GetCustomAttribute<T>(this FieldInfo type, bool inherit = false) where T : Attribute
    {
        object[] attributes = type.GetCustomAttributes(inherit);
        return attributes.OfType<T>().FirstOrDefault();
    }
}
#endif