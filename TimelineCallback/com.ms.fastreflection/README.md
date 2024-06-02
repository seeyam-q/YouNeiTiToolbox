# Unity FastReflection

Convert `MethodInfo` to `Delegate` to reduce the invoke cost.


# How to use


1. Create a cached call with target & funcName & parameterTypes .
``` csharp
static CachedCallFactory.Create(object target,string func,System.Type[] arguementTypes) => BaseCachedCall

```

2. Call Invoke of BaseCachedCall

```csharp

BaseCachedCall.Invoke(object[] args) => void

```




Examples:


```csharp

public class TestObject{

    public void Run(){
    }
}

//....

var obj = new TestObject();
//Create a cached call of obj.Run(parameters)
var cachedCall = CachedCallFactory.Create(obj,"Run",new Type[]{});

//Invoke the method
cachedCall.Invoke(null);

```


# Benchmark

Call instance void method 10000 times

CALL MODE|Directly|Delegate|CachedCall|Reflection
|---|---|---|---|---
COST(ms)| 0.121|0.144|0.193|22.191

CachedCall has simular performance with directly call and must faster than reflection.

# IL2CPP AOT Issue

If you create a cachedCall with ValueType parameters, it may caused AOT Issue. 

More Info: 
https://stackoverflow.com/questions/56183606/invoke-generic-method-via-reflection-in-c-sharp-il2cpp-on-ios

To resolve this issue, we must use `CachedCallArguementsTypeRegister.EnsureCall` to let compiler generate AOT code.

for example:

```csharp

//These codes can be put anywhere of your project, alse no need to be ran. It is only for compiler.
private static void Register(){
    //Support create cachedCalls with one int paramster
    CachedCallArguementsTypeRegister.EnsureCall<int>();

    //Support create cachedCalls with bool & int parameters
    CachedCallArguementsTypeRegister.EnsureCall<bool,int>();
}


```


