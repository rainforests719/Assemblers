#define NETCORE

namespace Assemblers;

internal static class DeepClonerSafeTypes
{
    internal static readonly ConcurrentDictionary<Type, bool> KnownTypes = new();

    static DeepClonerSafeTypes()
    {
        foreach (
            var x in
                new[]
                    {
                        typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong),
                        typeof(float), typeof(double), typeof(decimal), typeof(char), typeof(string), typeof(bool), typeof(DateTime),
                        typeof(IntPtr), typeof(UIntPtr), typeof(Guid),
                        Type.GetType("System.RuntimeType"),
                        Type.GetType("System.RuntimeTypeHandle"),
#if !NETCORE
						typeof(DBNull)
#endif
					}) KnownTypes.TryAdd(x, true);
    }

    private static bool CanReturnSameType(Type type, HashSet<Type> processingTypes)
    {
        bool isSafe;
        if (KnownTypes.TryGetValue(type, out isSafe)) return isSafe;
        if (type.IsEnum() || type.IsPointer)
        {
            KnownTypes.TryAdd(type, true);
            return true;
        }

#if !NETCORE
		// do not do anything with remoting. it is very dangerous to clone, bcs it relate to deep core of framework
		if (type.FullName.StartsWith("System.Runtime.Remoting.")
			&& type.Assembly == typeof(System.Runtime.Remoting.CustomErrorsModes).Assembly)
		{
			KnownTypes.TryAdd(type, true);
			return true;
		}

		if (type.FullName.StartsWith("System.Reflection.") && type.Assembly == typeof(PropertyInfo).Assembly)
		{
			KnownTypes.TryAdd(type, true);
			return true;
		}

		// catched by previous condition
		/*if (type.FullName.StartsWith("System.Reflection.Emit") && type.Assembly == typeof(System.Reflection.Emit.OpCode).Assembly)
		{
			KnownTypes.TryAdd(type, true);
			return true;
		}*/

		// this types are serious native resources, it is better not to clone it
		if (type.IsSubclassOf(typeof(System.Runtime.ConstrainedExecution.CriticalFinalizerObject)))
		{
			KnownTypes.TryAdd(type, true);
			return true;
		}

		// Better not to do anything with COM
		if (type.IsCOMObject)
		{
			KnownTypes.TryAdd(type, true);
			return true;
		}
#else
        if (type.FullName.StartsWith("System.DBNull"))
        {
            KnownTypes.TryAdd(type, true);
            return true;
        }

        if (type.FullName.StartsWith("System.RuntimeType"))
        {
            KnownTypes.TryAdd(type, true);
            return true;
        }

        if (type.FullName.StartsWith("System.Reflection.") && Equals(type.GetTypeInfo().Assembly, typeof(PropertyInfo).GetTypeInfo().Assembly))
        {
            KnownTypes.TryAdd(type, true);
            return true;
        }

        if (type.IsSubclassOfTypeByName("CriticalFinalizerObject"))
        {
            KnownTypes.TryAdd(type, true);
            return true;
        }

        // better not to touch ms dependency injection
        if (type.FullName.StartsWith("Microsoft.Extensions.DependencyInjection."))
        {
            KnownTypes.TryAdd(type, true);
            return true;
        }

        if (type.FullName == "Microsoft.EntityFrameworkCore.Internal.ConcurrencyDetector")
        {
            KnownTypes.TryAdd(type, true);
            return true;
        }
#endif
        if (!type.IsValueType())
        {
            KnownTypes.TryAdd(type, false);
            return false;
        }

        if (processingTypes == null)
            processingTypes = new HashSet<Type>();

        processingTypes.Add(type);

        List<FieldInfo> fi = new List<FieldInfo>();
        var tp = type;
        do
        {
            fi.AddRange(tp.GetAllFields());
            tp = tp.BaseType();
        }
        while (tp != null);

        foreach (var fieldInfo in fi)
        {
            // type loop
            var fieldType = fieldInfo.FieldType;
            if (processingTypes.Contains(fieldType)) continue;
            if (!CanReturnSameType(fieldType, processingTypes))
            {
                KnownTypes.TryAdd(type, false);
                return false;
            }
        }

        KnownTypes.TryAdd(type, true);
        return true;
    }

    public static bool CanReturnSameObject(Type type) => CanReturnSameType(type, null);
}