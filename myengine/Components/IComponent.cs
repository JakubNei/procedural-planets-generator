using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine.Components
{
    public interface IComponent
    {
		void OnAddedToEntity(Entity entity);
        Entity Entity { get; }
    }
}
