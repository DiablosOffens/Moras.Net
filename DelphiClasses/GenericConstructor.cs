using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Security;
using System.Security.Permissions;

namespace DelphiClasses
{
    internal static class ConstructorHelper
    {
        private readonly static MethodInfo s_miConstructValueType = typeof(ConstructorHelper).GetMethod("ConstructValueType", BindingFlags.NonPublic | BindingFlags.Static);
        //private delegate void CtorDelegate(object instance);
        //private readonly static PermissionSet s_delegateCreatePermissions = new PermissionSet(PermissionState.None);
        //private readonly static ConstructorInfo s_delegateCtorInfo = typeof(CtorDelegate).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) });

        //static ConstructorHelper()
        //{
        //    s_delegateCreatePermissions.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.MemberAccess));
        //    s_delegateCreatePermissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.UnmanagedCode));
        //}

        internal static GenericConstructor<T> CreateDefault<T>() where T : new()
        {
            Type t = typeof(T);
            if (t.IsValueType)
            {
                MethodInfo miCtor = s_miConstructValueType.MakeGenericMethod(t);
                return new GenericConstructor<T>((Func<T>)Delegate.CreateDelegate(typeof(Func<T>), miCtor));
            }

            ConstructorInfo defaultCtor = t.GetConstructor(Type.EmptyTypes);
            return new GenericConstructor<T>((Func<T>)CreateCtorDelegate(defaultCtor));
        }

        private static Delegate CreateCtorDelegate(ConstructorInfo ctor)
        {
            Type t = ctor.DeclaringType;
            // Can not use static implementation as long as RuntimeTypeHandle.Allocate is internal, because there is no benefit in using reflection
            // when we try to avoid it. Not because of performance (reflected method info could be turned into a delegate) but because of dependency
            // on undocumented internals.
            //s_delegateCreatePermissions.Assert();
            //CtorDelegate ctordel = (CtorDelegate)s_delegateCtorInfo.Invoke(new object[2] { null, ctor.MethodHandle.GetFunctionPointer() });
            //return (Func<object>)(() =>
            //{
            //    object obj = RuntimeTypeHandle.Allocate(t);
            //    ctordel(obj);
            //    return obj;
            //});
            DynamicMethod dynamic = new DynamicMethod(t.FullName + ConstructorInfo.ConstructorName, t, Type.EmptyTypes, t);
            ILGenerator il = dynamic.GetILGenerator();

            //il.DeclareLocal(type);
            il.Emit(OpCodes.Newobj, ctor);
            //il.Emit(OpCodes.Stloc_0);
            //il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

            return dynamic.CreateDelegate(typeof(Func<>).MakeGenericType(t));
        }

        private static T ConstructValueType<T>() where T : struct
        {
            return default(T);
        }
    }

    /// <summary>
    /// Holds a delegate of the default constructor of a generic type parameter in a singleton.
    /// </summary>
    /// <remarks>The implementation of the new() constraint uses Activator.CreateInstance&lt;T&gt; for reference types.
    /// This can be realy slow, if the implementation in the Framework decides to not cache the constructor in a delegate.</remarks>
    /// <typeparam name="T">The type of objects to construct.</typeparam>
    public class GenericConstructor<T> where T : new()
    {
        /// <summary>
        /// Returns a default generic constructor for the type specified by the generic argument.
        /// </summary>
        public readonly static GenericConstructor<T> Default = ConstructorHelper.CreateDefault<T>();

        private Func<T> m_funcCtor;

        internal GenericConstructor(Func<T> ctor)
        {
            m_funcCtor = ctor;
        }

        /// <summary>
        /// Returns the delegate which can be used to construct object for the type specified by the generic argument.
        /// </summary>
        public Func<T> CreateInstance { get { return m_funcCtor; } }
    }
}
