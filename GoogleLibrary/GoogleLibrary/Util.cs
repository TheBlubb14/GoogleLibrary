using System;
using System.Collections.Generic;

namespace GoogleLibrary
{
    public static class Util
    {
        public static void CheckNull(this object obj, string caller = "")
        {
            if (obj == null)
                throw new ArgumentException("value cannot be null", caller);

            if (obj is string s)
                if (string.IsNullOrEmpty(s))
                    throw new ArgumentException("value cannot be empty", caller);

            // TODO: generic type?
            if (obj is ICollection<string> l)
                if (l.Count < 1)
                    throw new ArgumentException("value cannot be empty", caller);
        }
    }
}
