using System;

namespace HandyControl.Controls
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NecessaryAttribute : Attribute
    {
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
            return obj.GetType() == this.GetType() && Equals((NecessaryAttribute) obj);
        }

        public override int GetHashCode() => base.GetHashCode();

        protected bool Equals(NecessaryAttribute other) => base.Equals(other);
    }
}
