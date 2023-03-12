#define NETCORE

namespace Assemblers;

internal abstract class ShallowObjectCloner
{
    protected abstract object DoCloneObject(object obj);

    private static readonly ShallowObjectCloner _unsafeInstance;

    private static ShallowObjectCloner _instance;

    public static object CloneObject(object obj) => _instance.DoCloneObject(obj);

    internal static bool IsSafeVariant() => _instance is ShallowSafeObjectCloner;

    static ShallowObjectCloner()
    {
#if !NETCORE
			_unsafeInstance = GenerateUnsafeCloner();
			_instance = _unsafeInstance;
			try
			{
				_instance.DoCloneObject(new object());
			}
			catch (Exception)
			{
				// switching to safe
				_instance = new ShallowSafeObjectCloner();
			}
#else
        _instance = new ShallowSafeObjectCloner();
        _unsafeInstance = _instance;
#endif
    }

    internal static void SwitchTo(bool isSafe)
    {
        DeepClonerCache.ClearCache();
        if (isSafe) _instance = new ShallowSafeObjectCloner();
        else _instance = _unsafeInstance;
    }

#if !NETCORE
		private static ShallowObjectCloner GenerateUnsafeCloner()
		{
			var mb = TypeCreationHelper.GetModuleBuilder();

			var builder = mb.DefineType("ShallowSafeObjectClonerImpl", TypeAttributes.Public, typeof(ShallowObjectCloner));
			var ctorBuilder = builder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis | CallingConventions.HasThis, Type.EmptyTypes);

			var cil = ctorBuilder.GetILGenerator();
			cil.Emit(OpCodes.Ldarg_0);
			// ReSharper disable AssignNullToNotNullAttribute
			cil.Emit(OpCodes.Call, typeof(ShallowObjectCloner).GetPrivateConstructors()[0]);
			// ReSharper restore AssignNullToNotNullAttribute
			cil.Emit(OpCodes.Ret);

			var methodBuilder = builder.DefineMethod(
				"DoCloneObject",
				MethodAttributes.Public | MethodAttributes.Virtual,
				CallingConventions.HasThis,
				typeof(object),
				new[] { typeof(object) });

			var il = methodBuilder.GetILGenerator();
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Call, typeof(object).GetPrivateMethod("MemberwiseClone"));
			il.Emit(OpCodes.Ret);
			var type = builder.CreateType();
			return (ShallowObjectCloner)Activator.CreateInstance(type);
		}
#endif

    private class ShallowSafeObjectCloner : ShallowObjectCloner
    {
        private static readonly Func<object, object> _cloneFunc;

        static ShallowSafeObjectCloner()
        {
            var methodInfo = typeof(object).GetPrivateMethod("MemberwiseClone");
            var p = Expression.Parameter(typeof(object));
            var mce = Expression.Call(p, methodInfo);
            _cloneFunc = Expression.Lambda<Func<object, object>>(mce, p).Compile();
        }

        protected override object DoCloneObject(object obj) => _cloneFunc(obj);
    }
}