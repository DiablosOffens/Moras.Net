using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DelphiClasses
{
    [AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
    public class BitSetAttribute : FlagsAttribute
    {
        public Type SetEnumType { get; private set; }
        public object FirstBit { get; private set; }
        public object LastBit { get; private set; }

        public BitSetAttribute(Type setEnumType, object firstBit, object lastBit)
        {
            if (!setEnumType.IsEnum)
                throw new ArgumentException("Type must be an enum.", "setEnumType");
            SetEnumType = setEnumType;
            SetFirstBit(firstBit);
            SetLastBit(lastBit);
        }

        private void SetFirstBit(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            Type valueType = value.GetType();
            if (valueType != SetEnumType && valueType != Enum.GetUnderlyingType(SetEnumType))
                throw new ArgumentException("Set this property a value of the enum type or its underlying integral type.", "value");

            FirstBit = value;
        }

        private void SetLastBit(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            Type valueType = value.GetType();
            if (valueType != SetEnumType && valueType != Enum.GetUnderlyingType(SetEnumType))
                throw new ArgumentException("Set this property a value of the enum type or its underlying integral type.", "value");

            LastBit = value;
        }

        public bool IsInrange<T>(T value) where T : struct, IComparable
        {
            Type valueType = typeof(T);
            if (valueType != SetEnumType && valueType != Enum.GetUnderlyingType(SetEnumType))
                throw new ArgumentException("The value must be of the enum type or its underlying integral type.", "value");

            if (value.CompareTo(FirstBit) < 0 || value.CompareTo(LastBit) > 0)
                return false;
            return true;
        }
    }
}
