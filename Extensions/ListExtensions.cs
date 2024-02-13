using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Debricked.Extensions
{
    internal static class ListExtensions
    {
        public static bool EqualTo(this List<string> self, List<string> other)
        {
            if(self.Count != other.Count) return false;
            for(int i = 0; i < self.Count; i++)
            {
                if (other[i] != self[i]) return false;
            }
            return true;
        }
    }
}
