using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MyEngine
{
    public class DataBinder
    {
        public string targetPropertyName;
        public object targetObject;
        public string sourcePropertyName;
        public object sourceObject;

		public IPropertyDescriptor targetProperty;
		public IPropertyDescriptor sourceProperty;

		public bool canUpdate;

		public void Start()
        {
            sourceProperty = PropertyDescriptorUtils.GetOne(sourceObject.GetType(), sourcePropertyName);
            targetProperty = PropertyDescriptorUtils.GetOne(targetObject.GetType(), targetPropertyName);

            canUpdate = sourceProperty != null && targetProperty != null && sourceProperty.Type == targetProperty.Type;
        }

		public void Update()
        {
            if (canUpdate)
            {
                targetProperty.Write(targetObject, sourceProperty.Read(sourceObject));
            }
        }
    }
}
