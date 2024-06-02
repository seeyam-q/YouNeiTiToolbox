using System.Reflection;
using System;

namespace MS.Reflection{

    public class CachedCallFactory{

        internal static System.Type MakeCachedCallGenericType(System.Type[] arguementTypes){
            if(arguementTypes == null || arguementTypes.Length == 0){
                return typeof(CachedCall);
            }else if(arguementTypes.Length == 1){
                return typeof (CachedCall<>).MakeGenericType(arguementTypes);
            }else if(arguementTypes.Length == 2){
                return typeof (CachedCall<,>).MakeGenericType(arguementTypes);
            }else if(arguementTypes.Length == 3){
                return typeof (CachedCall<,,>).MakeGenericType(arguementTypes);
            }else if(arguementTypes.Length == 4){
                return typeof (CachedCall<,,,>).MakeGenericType(arguementTypes);
            }else{
                throw new System.ArgumentOutOfRangeException("max supported arguements lenght is 4");
            }
        }

        internal static System.Type MakeCachedFuncGenericType(System.Type[] arguementTypes,System.Type returnType){
            System.Type[] genericTypes = new System.Type[1 + (arguementTypes == null?0:arguementTypes.Length)];
            var index = 0;
            if(arguementTypes != null){
                foreach(var tp in arguementTypes){
                    genericTypes[index] = tp;
                    index ++;
                }
            }
            genericTypes[index] = returnType;
            if(genericTypes.Length == 1){
                return typeof(CachedFunc<>).MakeGenericType(genericTypes);
            }else if(genericTypes.Length == 2){
                return typeof (CachedFunc<,>).MakeGenericType(genericTypes);
            }else if(genericTypes.Length == 3){
                return typeof (CachedFunc<,,>).MakeGenericType(genericTypes);
            }else if(genericTypes.Length == 4){
                return typeof (CachedFunc<,,,>).MakeGenericType(genericTypes);
            }else if(genericTypes.Length == 5){
                return typeof (CachedFunc<,,,,>).MakeGenericType(genericTypes);
            }else{
                throw new System.ArgumentOutOfRangeException("max supported arguements lenght is 4");
            }
        }

        public static BaseCachedCall Create(object target,string func,System.Type[] arguementTypes,bool throwExceptionsIfNotFound = true){
            var methodInfo = target.GetType().GetMethod(func,arguementTypes);
            if(methodInfo == null){
                if(throwExceptionsIfNotFound){
                    throw new System.MissingMethodException($"{target.GetType()}:{func}");
                }
                return null;
            }

            System.Type cachedCallType = null;
            if(IsVoidReturnType(methodInfo.ReturnType)){
                cachedCallType = MakeCachedCallGenericType(arguementTypes);
            }else{
                cachedCallType = MakeCachedFuncGenericType(arguementTypes,methodInfo.ReturnType);
            }
            var constructor = cachedCallType.GetConstructor(new System.Type[2]{ typeof (object), typeof (MethodInfo)});
            var cachedCall = constructor.Invoke(new object[]{target,methodInfo});
            return (BaseCachedCall)cachedCall;   
        }

        public static BaseCachedCall Create(object target,MethodInfo methodInfo){
            if(methodInfo == null){
                throw new System.ArgumentNullException("methodInfo");
            }
            var parameterInfos = methodInfo.GetParameters();
            System.Type[] arguementTypes = new Type[parameterInfos.Length];
            for(var i = 0; i < arguementTypes.Length;i++){
                arguementTypes[i] = parameterInfos[i].ParameterType;
            }
            System.Type cachedCallType = null;
            if(IsVoidReturnType(methodInfo.ReturnType)){
                cachedCallType = MakeCachedCallGenericType(arguementTypes);
            }else{
                cachedCallType = MakeCachedFuncGenericType(arguementTypes,methodInfo.ReturnType);
            }
            var constructor = cachedCallType.GetConstructor(new System.Type[2]{ typeof (object), typeof (MethodInfo)});
            var cachedCall = constructor.Invoke(new object[]{target,methodInfo});
            return (BaseCachedCall)cachedCall;   
        }

        public static BaseCachedCall CreateStatic(System.Type type,string func,System.Type[] arguementTypes){
            var methodInfo = type.GetMethod(func,arguementTypes);
            if(methodInfo == null){
                throw new System.MissingMethodException($"{type}:{func}");
            }
            System.Type cachedCallType = null;
            if(IsVoidReturnType(methodInfo.ReturnType)){
                cachedCallType = MakeCachedCallGenericType(arguementTypes);
            }else{
                cachedCallType = MakeCachedFuncGenericType(arguementTypes,methodInfo.ReturnType);
            }
            var constructor = cachedCallType.GetConstructor(new System.Type[2]{ typeof (object), typeof (MethodInfo)});
            var cachedCall = constructor.Invoke(new object[]{null,methodInfo});
            return (BaseCachedCall)cachedCall;    
        }

        private static bool IsVoidReturnType(System.Type type){
            return type == null || type == typeof(void);
        }
    }

    public abstract class BaseCachedCall{
        public abstract object Invoke(object[] args);
    }

    public class CachedCall:BaseCachedCall{

        private Action _action;
        public CachedCall(object target,MethodInfo method){
            _action = (Action)method.CreateDelegate(typeof(Action),target);
        }
        public override object Invoke(object[] args){
            _action();
            return null;
        }
    }

    public class CachedCall<T>:BaseCachedCall{
        private Action<T> _action;
        public CachedCall(object target,MethodInfo method){
            _action = (Action<T>)method.CreateDelegate(typeof(Action<T>),target);
        }
   
        public override object Invoke(object[] args){
            _action((T)args[0]);
            return null;
        }
    }

    public class CachedCall<T0,T1>:BaseCachedCall{
        private Action<T0,T1> _action;
        public CachedCall(object target,MethodInfo method){
            _action = (Action<T0,T1>)method.CreateDelegate(typeof(Action<T0,T1>),target);
        }
        public override object Invoke(object[] args){
            _action((T0)args[0],(T1)args[1]);
            return null;
        }
    }


    public class CachedCall<T0,T1,T2>:BaseCachedCall{
        private Action<T0,T1,T2> _action;
        public CachedCall(object target,MethodInfo method){
            _action = (Action<T0,T1,T2>)method.CreateDelegate(typeof(Action<T0,T1,T2>),target);
        }
        public override object Invoke(object[] args){
            _action((T0)args[0],(T1)args[1],(T2)args[2]);
            return null;
        }
    }

    public class CachedCall<T0,T1,T2,T3>:BaseCachedCall{
        private Action<T0,T1,T2,T3> _action;
        public CachedCall(object target,MethodInfo method){
            _action = (Action<T0,T1,T2,T3>)method.CreateDelegate(typeof(Action<T0,T1,T2,T3>),target);
        }
        public override object Invoke(object[] args){
            _action((T0)args[0],(T1)args[1],(T2)args[2],(T3)args[3]);
            return null;
        }
    }




    public class CachedFunc<TReturn>:BaseCachedCall{

        private Func<TReturn> _action;
        public CachedFunc(object target,MethodInfo method){
            _action = (Func<TReturn>)method.CreateDelegate(typeof(Func<TReturn>),target);
        }
        public override object Invoke(object[] args){
            return _action();
        }
    }

    public class CachedFunc<T,TReturn>:BaseCachedCall{
        private Func<T,TReturn> _action;
        public CachedFunc(object target,MethodInfo method){
            _action = (Func<T,TReturn>)method.CreateDelegate(typeof(Func<T,TReturn>),target);
        }
   
        public override object Invoke(object[] args){
            return _action((T)args[0]);
        }
    }

    public class CachedFunc<T0,T1,TReturn>:BaseCachedCall{
        private Func<T0,T1,TReturn> _action;
        public CachedFunc(object target,MethodInfo method){
            _action = (Func<T0,T1,TReturn>)method.CreateDelegate(typeof(Func<T0,T1,TReturn>),target);
        }
        public override object Invoke(object[] args){
            return _action((T0)args[0],(T1)args[1]);
        }
    }


    public class CachedFunc<T0,T1,T2,TReturn>:BaseCachedCall{
        private Func<T0,T1,T2,TReturn> _action;
        public CachedFunc(object target,MethodInfo method){
            _action = (Func<T0,T1,T2,TReturn>)method.CreateDelegate(typeof(Func<T0,T1,T2,TReturn>),target);
        }
        public override object Invoke(object[] args){
            return _action((T0)args[0],(T1)args[1],(T2)args[2]);
        }
    }

    public class CachedFunc<T0,T1,T2,T3,TReturn>:BaseCachedCall{
        private Func<T0,T1,T2,T3,TReturn> _action;
        public CachedFunc(object target,MethodInfo method){
            _action = (Func<T0,T1,T2,T3,TReturn>)method.CreateDelegate(typeof(Func<T0,T1,T2,T3,TReturn>),target);
        }
        public override object Invoke(object[] args){
            return _action((T0)args[0],(T1)args[1],(T2)args[2],(T3)args[3]);
        }
    }

}
