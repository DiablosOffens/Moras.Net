using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace DelphiClasses
{
    public interface INeedsInitialization
    {
        void Init();
    }

    // Converts reference type "values" to "pointer values", so that DynamicArray can store reference types as nullable "values"
    /*public struct Pointer<T> where T : class
    {
        public T Value;

        public static implicit operator T(Pointer<T> p)
        {
            return p.Value;
        }

        public static implicit operator Pointer<T>(T v)
        {
            return new Pointer<T> { Value = v };
        }
    }*/

    // Thats an extension from above, it stores these reference type "values" with always initialized default values by using the default constructor or
    // empty string.
    //HINT: if structs could have a user-defined default ctor, we could avoid the lazy initialized property.
    public struct ObjectAsValue<T> where T : class
    {
        private readonly static MethodInfo genDefaultCtor = typeof(ObjectAsValue<T>).GetMethod("DefaultCtor", BindingFlags.Static | BindingFlags.NonPublic);
        private static Func<T> defaultCtor;

        private static Func<T2> DefaultCtor<T2>() where T2 : new()
        {
            if (!typeof(T2).ImplementsInterface(typeof(INeedsInitialization)))
                return () => new T2();
            else
                return () =>
                {
                    T2 obj = new T2();
                    ((INeedsInitialization)obj).Init();
                    return obj;
                };
        }

        static ObjectAsValue()
        {
            Type elemType = typeof(T);
            if (elemType.IsGenericType && elemType.GetGenericTypeDefinition() == typeof(ValueRef<>))
                throw new ArgumentException("The generic parameter musst not be of the ValueRef<T> type. Instead use the Pointer<T> type.", "T");
            else if (elemType == typeof(string))
                defaultCtor = () => (T)(object)string.Empty;
            else if (elemType.GetConstructor(Type.EmptyTypes) != null)
            {
                MethodInfo genCtor = genDefaultCtor.MakeGenericMethod(elemType);
                defaultCtor = (Func<T>)genCtor.Invoke(null, null);
            }
            else
                throw new NotSupportedException(string.Format("The class \"{0}\" doesn't provide a default constructor.", elemType));
        }

        private T value;
        public T Value { get { return value ?? (value = defaultCtor()); } }

        public static implicit operator T(ObjectAsValue<T> p)
        {
            return p.Value;
        }

        public static implicit operator ObjectAsValue<T>(T v)
        {
            if (v == null) throw new ArgumentNullException("v");
            return new ObjectAsValue<T> { value = v };
        }
    }

    // Thats the complete opposite from above. It's used to store value types as "references", so it can be null, which saves the space for complex structs.
    public class ValueRef<T> where T : struct
    {
        public T Value;

        public ValueRef() { }

        public ValueRef(T value)
        {
            Value = value;
        }

        public static implicit operator T?(ValueRef<T> p)
        {
            if (p != null)
                return p.Value;
            return null;
        }

        public static implicit operator ValueRef<T>(T? v)
        {
            if (v.HasValue)
                return new ValueRef<T>(v.Value);
            return null;
        }
    }

    //HINT: if ref-return-values are possible, we can remove any restrictions, exposing the array for structs and instead use the indexing property.
    // https://github.com/dotnet/roslyn/issues/118
    public struct DynamicArray<T>
    {
        private static Dictionary<MemberInfo, Action<T[], int, object>> mapArraySetter = new Dictionary<MemberInfo, Action<T[], int, object>>();
        private static readonly bool elementIsValueType = typeof(T).IsValueType;
        private static readonly bool elementNeedsInit = typeof(T).ImplementsInterface(typeof(INeedsInitialization));
        private delegate void InitItemDelegate<T2>(ref T2 item);
        private readonly static MethodInfo genInitItemHelper = typeof(DynamicArray<T>).GetMethod("InitItemHelper", BindingFlags.Static | BindingFlags.NonPublic);
        private static InitItemDelegate<T> initItem;

        static DynamicArray()
        {
            if (elementIsValueType && elementNeedsInit)
            {
                MethodInfo geninitItem = genInitItemHelper.MakeGenericMethod(typeof(T));
                initItem = (InitItemDelegate<T>)geninitItem.Invoke(null, null);
            }
        }

        //HACK: Trick with generic constraint, so Init can be called on reference instead of boxed value,
        // because casting to interface every time involves boxing.
        private static InitItemDelegate<TInit> InitItemHelper<TInit>() where TInit : struct,T, INeedsInitialization
        {
            return (ref TInit item) => item.Init();
        }

        private T[] array;

        public T this[int index]
        {
            get
            {
                if (array == null)
                    throw new IndexOutOfRangeException();
                return array[index];
            }
            set
            {
                if (array == null)
                    throw new IndexOutOfRangeException();
                array[index] = value;
            }
        }

        public object this[int index, Expression<Func<T, object>> exprFieldOrProp]
        {
            set
            {
                if (array == null)
                    throw new IndexOutOfRangeException();
                GetArraySetter(exprFieldOrProp)(array, index, value);
            }
        }

        private static Action<T[], int, object> GetArraySetter(Expression<Func<T, object>> exprFieldOrProp)
        {
            if (exprFieldOrProp == null)
                throw new ArgumentNullException("exprFieldOrProp");

            MemberExpression memberexp = exprFieldOrProp.Body as MemberExpression;
            if (memberexp.Member.MemberType != MemberTypes.Property &&
                memberexp.Member.MemberType != MemberTypes.Field)
                throw new ArgumentException("Lambda is no property or field access.", "exprFieldOrProp");

            Action<T[], int, object> setter;
            if (mapArraySetter.TryGetValue(memberexp.Member, out setter))
                return setter;

            ParameterExpression arrayparam = Expression.Parameter(typeof(T[]), "array");
            ParameterExpression indexparam = Expression.Parameter(typeof(int), "index");
            ParameterExpression valueparam = Expression.Parameter(typeof(object), "value");
            IndexExpression arrayaccess = Expression.ArrayAccess(arrayparam, indexparam);
            if (memberexp.Member.MemberType == MemberTypes.Field)
            {
                memberexp = Expression.Field(arrayaccess, (FieldInfo)memberexp.Member);
            }
            else
            {
                memberexp = Expression.Property(arrayaccess, (PropertyInfo)memberexp.Member);
            }
            var setterExpr = Expression.Lambda<Action<T[], int, object>>(Expression.Assign(memberexp, valueparam), arrayparam, indexparam, valueparam);
            setter = setterExpr.Compile();
            mapArraySetter.Add(memberexp.Member, setter);
            return setter;
        }

        public T[] Array { get { return array; } }

        public static implicit operator DynamicArray<T>(T[] a)
        {
            return new DynamicArray<T> { array = a };
        }

        public int High
        {
            get { return array != null ? array.GetUpperBound(0) : 0; }
        }

        public int Low
        {
            get { return array != null ? array.GetLowerBound(0) : 0; }
        }

        public int Length
        {
            get { return array != null ? array.Length : 0; }
            set { SetLength(value); }
        }

        private void SetLength(int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException("length", length, "");

            if (length == 0)
            {
                if (array != null)
                    array = null;
            }
            else
            {
                int oldlen = Length;
                System.Array.Resize(ref array, length);
                if (elementNeedsInit && elementIsValueType && length > oldlen)
                    for (int i = oldlen; i < length; i++)
                        initItem(ref array[i]);
            }
        }

        public DynamicArray<T> Copy()
        {
            DynamicArray<T> temp;
            temp.array = new T[array.Length];
            System.Array.Copy(array, temp.array, array.Length);
            return temp;
        }

        public DynamicArray<T> CopyRange(int startIndex, int count)
        {
            DynamicArray<T> temp;
            temp.array = new T[count];
            System.Array.Copy(array, startIndex, temp.array, 0, count);
            return temp;
        }

        //protected void AllocData(int count)
        //{
        //    array = new T[count];
        //}
    }
}
