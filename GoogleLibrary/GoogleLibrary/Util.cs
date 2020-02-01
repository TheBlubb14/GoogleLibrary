using System;
using System.Collections.Generic;
using System.Linq;

namespace GoogleLibrary
{
    public static class Util
    {
        public static void CheckNull(this object obj, string caller = "")
        {
            if (obj is null)
                throw new ArgumentException("value cannot be null", caller);

            if (obj is string s && string.IsNullOrEmpty(s))
                throw new ArgumentException("value cannot be empty", caller);

            // TODO: generic type?
            if (obj is IEnumerable<string> l && !l.Any())
                throw new ArgumentException("value cannot be empty", caller);
        }
    }
}
