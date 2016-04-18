/*

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine.Components
{
    public class SpringJoint : Joint
    {
        Rigidbody m_connectedTo;
        public Rigidbody connectedTo
        {
            set
            {
                m_connectedTo = value;

                var a = this.GetComponent<Collider>();
                if(!a) return;

                var b = connectedTo.GetComponent<Collider>();
                if(!b) return;

                physicsJoint = new BEPUphysics.Constraints.TwoEntity.Joints.DistanceJoint(a.collisionEntity_generic, b.collisionEntity_generic, a.transform.position, b.transform.position);

                PhysicsUsage.PhysicsManager.instance.Add(physicsJoint);
            }
            get
            {
                return m_connectedTo;
            }
        }

        BEPUphysics.Constraints.TwoEntity.Joints.DistanceJoint physicsJoint;

        internal override void OnCreated(Entity entity)
        {
            base.OnCreated(entity);            
        }
        public SpringJoint(Entity entity) : base(entity)
        {
        }
    }
}

*/