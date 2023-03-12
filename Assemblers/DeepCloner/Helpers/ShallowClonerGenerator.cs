namespace Assemblers;

internal static class ShallowClonerGenerator
{
    public static T CloneObject<T>(T obj)
    {
        if (obj is ValueType)
        {
            if (typeof(T) == obj.GetType()) return obj;
            return (T)ShallowObjectCloner.CloneObject(obj);
        }

        if (ReferenceEquals(obj, null)) return (T)(object)null;
        if (DeepClonerSafeTypes.CanReturnSameObject(obj.GetType())) return obj;
        return (T)ShallowObjectCloner.CloneObject(obj);
    }
}