using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine.Components
{
    public class ComponentSettingAttribute : Attribute
    {
        public bool allowMultiple = true;
        public ComponentSettingAttribute()
        {

        }
    }
}
