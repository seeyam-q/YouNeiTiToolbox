
namespace MS.Reflection{
    using System;
    using System.Collections.Generic;



    /// <summary>
    /// For AOT of il2cpp, if you create cacheCall for method that has ValueType arguements, you must use CacheCallArguementsTypeRegister.EnsureCall<> to generate AOT code.
    /// https://stackoverflow.com/questions/56183606/invoke-generic-method-via-reflection-in-c-sharp-il2cpp-on-ios
    /// </summary>
    public static class CachedCallArguementsTypeRegister{
        private static HashSet<System.Type> _supportCacheCallType = new HashSet<Type>();
        private static void Ensure ( Action action )
        {
            if ( IsFalse ( ) )
            {
                try
                {
                    action ( );
                }
                catch ( Exception e )
                {
                    throw new InvalidOperationException ( "", e );
                }
            }
        }
        public static void EnsureCall<T>(){
            Ensure(()=>{
                var a = new CachedCall<T>(null,null);
            });
            var tp = typeof(CachedCall<T>);
            if(!_supportCacheCallType.Contains(tp)){
                _supportCacheCallType.Add(tp);
            }
        }

        public static void EnsureCall<T0,T1>(){
            Ensure(()=>{
                var a = new CachedCall<T0,T1>(null,null);
            });
            var tp = typeof(CachedCall<T0,T1>);
            if(!_supportCacheCallType.Contains(tp)){
                _supportCacheCallType.Add(tp);
            }
        }
        public static void EnsureCall<T0,T1,T2>(){
            Ensure(()=>{
                var a = new CachedCall<T0,T1,T2>(null,null);
            });
            var tp = typeof(CachedCall<T0,T1,T2>);
            if(!_supportCacheCallType.Contains(tp)){
                _supportCacheCallType.Add(tp);
            }
        }

        public static void EnsureCall<T0,T1,T2,T3>(){
            Ensure(()=>{
                var a = new CachedCall<T0,T1,T2,T3>(null,null);
            });
            var tp = typeof(CachedCall<T0,T1,T2,T3>);
            if(!_supportCacheCallType.Contains(tp)){
                _supportCacheCallType.Add(tp);
            }
        }


        public static bool IsCallSupport(params System.Type[] types){
            if(types.Length > 4){
                return false;
            }
            var hasValueType = false;
            foreach(var tp in types){
                if(tp.IsValueType){
                    hasValueType = true;
                }
            }
            if(!hasValueType){
                return true;
            }
            var cachedCallType = CachedCallFactory.MakeCachedCallGenericType(types);
            return _supportCacheCallType.Contains(cachedCallType);
        }


        public static void EnsureFunc<TReturn>(){
            Ensure(()=>{
                var a = new CachedFunc<TReturn>(null,null);
            });
            var tp = typeof(CachedFunc<TReturn>);
            if(!_supportCacheCallType.Contains(tp)){
                _supportCacheCallType.Add(tp);
            }
        }


        public static void EnsureFunc<T0,TReturn>(){
            Ensure(()=>{
                var a = new CachedFunc<T0,TReturn>(null,null);
            });
            var tp = typeof(CachedFunc<T0,TReturn>);
            if(!_supportCacheCallType.Contains(tp)){
                _supportCacheCallType.Add(tp);
            }
        }

        public static void EnsureFunc<T0,T1,TReturn>(){
            Ensure(()=>{
                var a = new CachedFunc<T0,T1,TReturn>(null,null);
            });
            var tp = typeof(CachedFunc<T0,T1,TReturn>);
            if(!_supportCacheCallType.Contains(tp)){
                _supportCacheCallType.Add(tp);
            }
        }

        public static void EnsureFunc<T0,T1,T2,TReturn>(){
            Ensure(()=>{
                var a = new CachedFunc<T0,T1,T2,TReturn>(null,null);
            });
            var tp = typeof(CachedFunc<T0,T1,T2,TReturn>);
            if(!_supportCacheCallType.Contains(tp)){
                _supportCacheCallType.Add(tp);
            }
        }

        public static void EnsureFunc<T0,T1,T2,T3,TReturn>(){
            Ensure(()=>{
                var a = new CachedFunc<T0,T1,T2,T3,TReturn>(null,null);
            });
            var tp = typeof(CachedFunc<T0,T1,T2,T3,TReturn>);
            if(!_supportCacheCallType.Contains(tp)){
                _supportCacheCallType.Add(tp);
            }
        }

        public static bool IsFuncSupport(System.Type returnType, params System.Type[] types){
            if(types.Length > 4){
                return false;
            }
            var hasValueType = returnType.IsValueType;
            foreach(var tp in types){
                if(tp.IsValueType){
                    hasValueType = true;
                }
            }
            if(!hasValueType){
                return true;
            }
            var cachedFuncType = CachedCallFactory.MakeCachedFuncGenericType(types,returnType);
            return _supportCacheCallType.Contains(cachedFuncType);
        }



        private static bool s_alwaysFalse = DateTime.UtcNow.Year < 0;

        /// <summary>
        /// Always return false but compiler doesn't know it.
        /// </summary>
        /// <returns>False</returns>
        private static bool IsFalse ( )
        {
            return s_alwaysFalse;
        }
    }
}
