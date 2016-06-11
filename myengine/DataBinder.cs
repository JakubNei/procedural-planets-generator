using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MyEngine
{
    class DataBinder
    {
        public string targetPropertyName;
        public object targetObject;
        public string sourcePropertyName;
        public object sourceObject;

        IPropertyDescriptor targetProperty;
        IPropertyDescriptor sourceProperty;

        bool canUpdate;

        void Start()
        {
            sourceProperty = PropertyDescriptorUtils.GetOne(sourceObject.GetType(), sourcePropertyName);
            targetProperty = PropertyDescriptorUtils.GetOne(targetObject.GetType(), targetPropertyName);

            canUpdate = sourceProperty != null && targetProperty != null && sourceProperty.Type == targetProperty.Type;
        }

        void Update()
        {
            if (canUpdate)
            {
                targetProperty.Write(targetObject, sourceProperty.Read(sourceObject));
            }
        }
    }
}
