using System.Linq;
using Mono.Cecil;

public partial class ModuleWeaver
{

    public MethodReference GetTypeFromHandle;
    public TypeReference ExceptionType;
    public ArrayType ObjectArray;
    public MethodReference ConcatMethod;
    public MethodReference FormatMethod;
    public MethodReference FuncInvokeMethod;
    public TypeReference GenericFunc;

    public void LoadSystemTypes()
    {
        var mscorlib = AssemblyResolver.Resolve(new AssemblyNameReference("mscorlib", null));
        var typeType = mscorlib?.MainModule.Types.FirstOrDefault(x => x.Name == "Type");
        if (typeType == null)
        {
            var runtime = AssemblyResolver.Resolve(new AssemblyNameReference("System.Runtime",null));
            typeType = runtime.MainModule.Types.First(x => x.Name == "Type");
        }
        var funcDefinition = typeType.Module.Types.FirstOrDefault(x => x.Name == "Func`1");
        if (funcDefinition == null)
        {
            var core = AssemblyResolver.Resolve(new AssemblyNameReference("System.Core", null));
            funcDefinition = core.MainModule.Types.First(x => x.Name == "Func`1");
        }
        var genericInstanceType = new GenericInstanceType(funcDefinition);
        genericInstanceType.GenericArguments.Add(ModuleDefinition.TypeSystem.String);
        GenericFunc = ModuleDefinition.ImportReference(genericInstanceType);

        var methodReference = new MethodReference("Invoke", funcDefinition.FindMethod("Invoke").ReturnType, genericInstanceType) { HasThis = true };
        FuncInvokeMethod = ModuleDefinition.ImportReference(methodReference);

        GetTypeFromHandle = typeType.Methods
            .First(x => x.Name == "GetTypeFromHandle" &&
                        x.Parameters.Count == 1 &&
                        x.Parameters[0].ParameterType.Name == "RuntimeTypeHandle");
        GetTypeFromHandle = ModuleDefinition.ImportReference(GetTypeFromHandle);


        var stringType = mscorlib?.MainModule.Types.FirstOrDefault(x => x.Name == "String");
        if (stringType == null)
        {
            var runtime = AssemblyResolver.Resolve(new AssemblyNameReference("System.Runtime", null));
            stringType = runtime.MainModule.Types.First(x => x.Name == "String");
        }
        ConcatMethod = ModuleDefinition.ImportReference(stringType.FindMethod("Concat", "String", "String"));
        FormatMethod = ModuleDefinition.ImportReference(stringType.FindMethod("Format", "String", "Object[]"));
        ObjectArray = new ArrayType(ModuleDefinition.TypeSystem.Object);

        var exceptionType = mscorlib?.MainModule.Types.FirstOrDefault(x => x.Name == "Exception");
        if (exceptionType == null)
        {
            var runtime = AssemblyResolver.Resolve(new AssemblyNameReference("System.Runtime",null));
            exceptionType = runtime.MainModule.Types.First(x => x.Name == "Exception");
        }
        ExceptionType = ModuleDefinition.ImportReference(exceptionType);

    }

}