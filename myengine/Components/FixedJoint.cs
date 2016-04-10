using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyEngine;

namespace MyEngine.Components
{
    public class FixedJoint : Joint, IDisposable
    {
        Rigidbody m_connectedTo;
        public Rigidbody connectedTo
        {
            set
            {
                if (physicsJoint!=null)
                {
                    PhysicsUsage.PhysicsManager.instance.Remove(physicsJoint);
                    physicsJoint = null;
                }

                m_connectedTo = value;

                if (value != null)
                {

                    var a = this.Entity.GetComponent<Collider>();
                    if (a == null) return;

                    var b = connectedTo.Entity.GetComponent<Collider>();
                    if (b == null) return;

                    physicsJoint = new BEPUphysics.Constraints.SolverGroups.WeldJoint(a.collisionEntity_generic, b.collisionEntity_generic);
                    physicsJoint.BallSocketJoint.SpringSettings.Stiffness = 10000000;
                    physicsJoint.NoRotationJoint.SpringSettings.Stiffness = physicsJoint.BallSocketJoint.SpringSettings.Stiffness;

                    PhysicsUsage.PhysicsManager.instance.Add(physicsJoint);
                }
            }
            get
            {
                return m_connectedTo;
            }
        }

        BEPUphysics.Constraints.SolverGroups.WeldJoint physicsJoint;

        public FixedJoint(Entity entity) : base(entity)
        {
        }
   

        public void Dispose()
        {            
            connectedTo = null;
        }
    }
}
