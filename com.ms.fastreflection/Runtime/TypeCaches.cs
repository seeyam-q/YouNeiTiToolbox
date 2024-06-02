using System.Collections.Generic;

namespace MS.Reflection{
    public static class TypeCaches
    {
        private static Dictionary<string,System.Type> _cachedTypes = new Dictionary<string, System.Type>();

		public static System.Type GetType(string assemblyQualifiedName){
			if(string.IsNullOrEmpty(assemblyQualifiedName)){
				return null;
			}
			System.Type resultType = null;
			if(_cachedTypes.TryGetValue(assemblyQualifiedName,out resultType)){
				return resultType;
			}
			resultType =  System.Type.GetType (assemblyQualifiedName);
			_cachedTypes.Add(assemblyQualifiedName,resultType);
			return resultType;
		}        
    }
}
