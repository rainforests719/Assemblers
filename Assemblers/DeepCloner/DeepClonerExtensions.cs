namespace Assemblers;

/// <summary>
/// `DeepCloner` 对象克隆的扩展
/// </summary>
public static class DeepClonerExtensions
{
    static DeepClonerExtensions()
    {
        if (!PermissionCheck()) throw new SecurityException("DeepCloner should have enough permissions to run. Grant FullTrust or Reflection permission");

        static bool PermissionCheck()
        {
            try
            {
                new object().ShallowClone();
            }
            catch (VerificationException)
            {
                return false;
            }
            catch (MemberAccessException)
            {
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// `DeepClone` 深克隆
    /// </summary>
    public static T DeepClone<T>(this T obj) => DeepClonerGenerator.CloneObject(obj);

    /// <summary>
    /// `DeepCloneTo` 深克隆到现有对象
    /// </summary>
    public static TTo DeepCloneTo<TFrom, TTo>(this TFrom objFrom, TTo objTo) where TTo : class, TFrom => (TTo)DeepClonerGenerator.CloneObjectTo(objFrom, objTo, true);

    /// <summary>
    /// `ShallowClone` 浅拷贝
    /// </summary>
    public static T ShallowClone<T>(this T obj) => ShallowClonerGenerator.CloneObject(obj);

    /// <summary>
    /// `ShallowCloneTo` 浅拷贝到现有对象
    /// </summary>
    public static TTo ShallowCloneTo<TFrom, TTo>(this TFrom objFrom, TTo objTo) where TTo : class, TFrom => (TTo)DeepClonerGenerator.CloneObjectTo(objFrom, objTo, false);
}