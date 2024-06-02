using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MS.Reflection{
    using System;

    public class CachedCallBenchmark : MonoBehaviour
    {

        public class TestObject{

            public void Run(){
            }

            public void Run(int value){

            }

            public void Run(int v1,bool v2){

            }

            public void Run(int v1,bool v2,string v3){

            }

            public void Run(int v1,float v2,string v3,UnityEngine.Object v4){

            }
        }

        static CachedCallBenchmark(){
            CachedCallArguementsTypeRegister.EnsureCall<int>();
            CachedCallArguementsTypeRegister.EnsureCall<int,bool>();
            CachedCallArguementsTypeRegister.EnsureCall<int,bool,string>();
            CachedCallArguementsTypeRegister.EnsureCall<int,float,string,UnityEngine.Object>();
        }

        private static int runCounts = 10000;

        private static double TestRunVoidDirectly(){
            var instance = new TestObject();
            var start = System.DateTime.Now;
            for(var i = 0; i < runCounts; i ++){
                instance.Run();
            }
            var cost = (System.DateTime.Now - start).TotalMilliseconds;
            return cost;
        }

        private static double TestRunVoidWithDelegate(){
            var instance = new TestObject();
            System.Action action = instance.Run;
            var start = System.DateTime.Now;
            for(var i = 0; i < runCounts; i ++){
                action();
            }
            var cost = (System.DateTime.Now - start).TotalMilliseconds;
            return cost;
        }  

        private static double TestRunVoidWithCachedCall(){
            var instance = new TestObject();
            var cachedCall = CachedCallFactory.Create(instance,"Run",new Type[]{});
            var start = System.DateTime.Now;
            for(var i = 0; i < runCounts; i ++){
                cachedCall.Invoke(null);
            }
            var cost = (System.DateTime.Now - start).TotalMilliseconds;
            return cost;
        }  

        private static double TestRunVoidWithReflection(){
            var instance = new TestObject();
            var method = typeof(TestObject).GetMethod("Run",new Type[]{});
            var start = System.DateTime.Now;
            for(var i = 0; i < runCounts; i ++){
                method.Invoke(instance,null);
            }
            var cost = (System.DateTime.Now - start).TotalMilliseconds;
            return cost;
        }  

        private static void RunVoidBenchMark(){
            var c1 = TestRunVoidDirectly();
            var c2 = TestRunVoidWithDelegate();
            var c3 = TestRunVoidWithCachedCall();
            var c4 = TestRunVoidWithReflection();
            Debug.LogFormat("Directly = {0}, Delegate = {1}, CachedCall = {2},Reflection = {3}",c1,c2,c3,c4);
        }


        private static double RunOneArgDirectly(){
            var instance = new TestObject();
            var start = System.DateTime.Now;
            for(var i = 0; i < runCounts; i ++){
                instance.Run(1);
            }
            var cost = (System.DateTime.Now - start).TotalMilliseconds;
            return cost;
        }

        private static double RunOneArgWithDelegate(){
            var instance = new TestObject();
            System.Action<int> action = instance.Run;
            var start = System.DateTime.Now;
            for(var i = 0; i < runCounts; i ++){
                action(1);
            }
            var cost = (System.DateTime.Now - start).TotalMilliseconds;
            return cost;
        }  

        private static double RunOneArgWithCachedCall(){
            var instance = new TestObject();
            var cachedCall = CachedCallFactory.Create(instance,"Run",new Type[]{typeof(int)});
            var args = new object[]{1};
            var start = System.DateTime.Now;
            for(var i = 0; i < runCounts; i ++){
                cachedCall.Invoke(args);
            }
            var cost = (System.DateTime.Now - start).TotalMilliseconds;
            return cost;
        }  

        private static double RunOneArgWithReflection(){
            var instance = new TestObject();
            var method = typeof(TestObject).GetMethod("Run",new Type[]{typeof(int)});
            var args = new object[]{1};
            var start = System.DateTime.Now;
            for(var i = 0; i < runCounts; i ++){
                method.Invoke(instance,args);
            }
            var cost = (System.DateTime.Now - start).TotalMilliseconds;
            return cost;
        }  

        private static void RunOneArgBenchmark(){
            var c1 = RunOneArgDirectly();
            var c2 = RunOneArgWithDelegate();
            var c3 = RunOneArgWithCachedCall();
            var c4 = RunOneArgWithReflection();
            Debug.LogFormat("Directly = {0}, Delegate = {1}, CachedCall = {2},Reflection = {3}",c1,c2,c3,c4);
        }


        public Button testButton;

        void Start(){
            testButton.onClick.AddListener(()=>{
                RunVoidBenchMark();
                RunOneArgBenchmark();
            });
        } 

    }
}
