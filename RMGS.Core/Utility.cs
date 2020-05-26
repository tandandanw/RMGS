using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

namespace RMGS.Core
{
    public static class Utility
    {
        public static int Random(this double[] a, double r)
        {
            double sum = a.Sum();
            for (int j = 0; j < a.Length; j++) a[j] /= sum;

            int i = 0;
            double x = 0;

            while (i < a.Length)
            {
                x += a[i];
                if (r <= x) return i;
                i++;
            }

            return 0;
        }

        public static long ToPower(this int a, int n)
        {
            long product = 1;
            for (int i = 0; i < n; i++) product *= a;
            return product;
        }

        public static T Get<T>(this XElement xelem, string attribute, T defaultT = default(T))
        {
            XAttribute a = xelem.Attribute(attribute);
            return a == null ? defaultT : (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(a.Value);
        }

        public static IEnumerable<XElement> Elements(this XElement xelement, params string[] names) => xelement.Elements().Where(e => names.Any(n => n == e.Name));
    }
}
