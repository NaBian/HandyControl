#nullable enable
using System;

namespace HandyControl.Controls
{
    [AttributeUsage(AttributeTargets.All)]
    public class HierarchyLevelAttribute : Attribute
    {
        /// <summary>
        /// This is the hierarchy level value.
        /// </summary>
        private int? _value;

        /// <summary>
        /// Initializes a new instance of the <see cref='HierarchyLevelAttribute'/>
        /// class using a Unicode character.
        /// </summary>
        public HierarchyLevelAttribute(char value)
        {
            _value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref='HierarchyLevelAttribute'/>
        /// class using an 8-bit unsigned integer.
        /// </summary>
        public HierarchyLevelAttribute(byte value)
        {
            _value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref='HierarchyLevelAttribute'/>
        /// class using a 16-bit signed integer.
        /// </summary>
        public HierarchyLevelAttribute(short value)
        {
            _value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref='HierarchyLevelAttribute'/>
        /// class using a 32-bit signed integer.
        /// </summary>
        public HierarchyLevelAttribute(int value)
        {
            _value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref='HierarchyLevelAttribute'/>
        /// class using a <see cref='System.String'/>.
        /// </summary>
        public HierarchyLevelAttribute(string? value)
        {
            if (value is null || !int.TryParse(value, out var val))
            {
                return;
            }
            _value = val;
        }

        /// <summary>
        /// Gets the default value of the property this attribute is bound to.
        /// </summary>
        public virtual int? Value => _value;

        public override bool Equals(object? obj)
        {
            if (obj == this)
            {
                return true;
            }
            if (!(obj is HierarchyLevelAttribute other))
            {
                return false;
            }

            if (Value == null)
            {
                return other.Value == null;
            }

            return Value.Equals(other.Value);
        }

        public override int GetHashCode() => base.GetHashCode();

        protected void SetValue(int? value) => _value = value;
    }
}
