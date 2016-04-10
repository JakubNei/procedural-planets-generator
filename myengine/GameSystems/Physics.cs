using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyEngine.Components;

namespace MyEngine
{
    public static class Physics
    {
        public static bool Raycast(Ray ray, out RaycastHit raycastHit, float maxDistance = float.MaxValue)
        {
            BEPUphysics.RayCastResult result;
            BEPUutilities.Ray r = new BEPUutilities.Ray()
            {
                Direction = ray.direction,
                Position = ray.origin,
            };
            bool ret = PhysicsUsage.PhysicsManager.instance.Space.RayCast(r, maxDistance, out result);
            if (ret)
            {
                var other = result.HitObject;
                var otherEntity = other as BEPUphysics.BroadPhaseEntries.MobileCollidables.EntityCollidable;
                var otherCollider = otherEntity.Entity.Tag as Collider;
                raycastHit = new RaycastHit()
                    {
                        m_Collider = otherCollider,
                        m_Point = result.HitData.Location,
                        m_Normal = result.HitData.Normal,
                        m_Distance = result.HitData.T,
                    };
            }
            else
            {
                raycastHit = new RaycastHit();
            }
            return ret;
        }

        public static void IgnoreCollision(Collider collider1, Collider collider2, bool ignore = true)
        {
            PhysicsUsage.PhysicsManager.instance.IgnoreCollision(collider1, collider2, ignore);
        }
    }
}
