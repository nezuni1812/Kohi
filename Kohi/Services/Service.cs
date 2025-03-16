using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Services
{
    public class Service
    {
        static Dictionary<string, object> _singletons = new Dictionary<string, object>();
        public static void AddKeyedSingleton<IParent, Child>()
        {
            Type child = typeof(Child);
            Type parent = typeof(IParent);
            _singletons[parent.Name] = Activator.CreateInstance(child)!;
        }
        public static IParent GetKeyedSingleton<IParent>()
        {
            Type parent = typeof(IParent);
            return (IParent)_singletons[parent.Name];
        }
    }
}
