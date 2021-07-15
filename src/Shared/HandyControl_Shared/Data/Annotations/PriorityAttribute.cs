using System;

namespace HandyControl.Controls
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PriorityAttribute : Attribute
    {
        private int mValue = 0;

        public PriorityAttribute(int value)
        {
            mValue = value;
        }

        /// <summary>
        /// Gets the default value of the property this attribute is bound to.
        /// </summary>
        public virtual int Value => mValue;

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj))
            {
                return false;
            }
            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }
            return obj.GetType() == this.GetType() && Equals((PriorityAttribute) obj);
        }

        public override int GetHashCode() => base.GetHashCode();

        protected void SetValue(int value) => mValue = value;

        protected bool Equals(PriorityAttribute other) => base.Equals(other) && mValue == other.mValue;
    }
}
