using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System;
using System.Diagnostics;

using BEPUphysics;
using BEPUphysics.Threading;
using BEPUutilities;
using BEPUphysics;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseSystems;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.Settings;
using BEPUutilities;

using MyEngine.Components;

namespace MyEngine.PhysicsUsage
{
    internal class PhysicsManager
    {
        private int accumulatedPhysicsFrames;
        private double accumulatedPhysicsTime;
        private double previousTimeMeasurement;
        private ParallelLooper parallelLooper;


        static internal PhysicsManager instance;

        CollisionGroup firstStackGroup;
        CollisionGroup secondStackGroup;

        internal PhysicsManager()
        {
            instance = this;

            parallelLooper = new ParallelLooper();
            //This section lets the engine know that it can make use of multithreaded systems
            //by adding threads to its thread pool.
#if XBOX360
            parallelLooper.AddThread(delegate { Thread.CurrentThread.SetProcessorAffinity(new[] { 1 }); });
            parallelLooper.AddThread(delegate { Thread.CurrentThread.SetProcessorAffinity(new[] { 3 }); });
            parallelLooper.AddThread(delegate { Thread.CurrentThread.SetProcessorAffinity(new[] { 4 }); });
            parallelLooper.AddThread(delegate { Thread.CurrentThread.SetProcessorAffinity(new[] { 5 }); });

#else
            if (Environment.ProcessorCount > 1)
            {
                for (int i = 0; i < Environment.ProcessorCount; i++)
                {
                    parallelLooper.AddThread();
                }
            }
#endif
            
            Space = new BEPUphysics.Space(parallelLooper);
            Space.ForceUpdater.Gravity = new Vector3(0, -9.81f, 0f);

            Space.TimeStepSettings.MaximumTimeStepsPerFrame = 100;


            //Set up two stacks which go through each other
            firstStackGroup = new CollisionGroup();
            secondStackGroup = new CollisionGroup();
            //Adding this rule to the space's collision group rules will prevent entities belong to these two groups from generating collision pairs with each other.
            var groupPair = new CollisionGroupPair(firstStackGroup, secondStackGroup);
            //CollisionRules.CollisionGroupRules.Add(groupPair, CollisionRule.NoBroadPhase);


        }

        /// <summary>
        /// Gets the average time spent per frame in the physics simulation.
        /// </summary>
        public double PhysicsTime { get; private set; }


        /// <summary>
        /// Gets or sets the space this simulation runs in.
        /// </summary>
        internal BEPUphysics.Space Space { get; set; }

        List<ISpaceObject> allSpaceObjects = new List<ISpaceObject>(1000);
        public void Add(ISpaceObject spaceObject)
        {
            if (!allSpaceObjects.Contains(spaceObject))
            {
                allSpaceObjects.Add(spaceObject);
                Space.Add(spaceObject);
            }
        }
        public void Remove(ISpaceObject spaceObject)
        {
            if (allSpaceObjects.Contains(spaceObject))
            {
                allSpaceObjects.Remove(spaceObject);
                Space.Remove(spaceObject);
            }
        }

        public void IgnoreCollision(Collider collider1, Collider collider2, bool ignore)
        {
            if (ignore)
            {
                CollisionRules.AddRule(collider1.collisionEntity_generic.CollisionInformation, collider2.collisionEntity_generic.CollisionInformation, CollisionRule.NoBroadPhase);
                //collider1.collisionEntity_generic.CollisionInformation.CollisionRules.Group = firstStackGroup;
                //collider2.collisionEntity_generic.CollisionInformation.CollisionRules.Group = secondStackGroup;
            }
            else
            {
                CollisionRules.RemoveRule(collider1.collisionEntity_generic.CollisionInformation, collider2.collisionEntity_generic.CollisionInformation);
                //collider1.collisionEntity_generic.CollisionInformation.CollisionRules.Group = null;
                //collider2.collisionEntity_generic.CollisionInformation.CollisionRules.Group = null;
            }
        }


    
        /// <summary>
        /// Updates the game.
        /// </summary>
        /// <param name="dt">Game timestep.</param>
        public void Update(float dt)
        {
            long startTime = Stopwatch.GetTimestamp();

            //Update the simulation.
            //Pass in dt to the function to use internal timestepping, if desired.
            //Using internal time stepping usually works best when the interpolation is also used.
            //Check out the asynchronous updating documentation for an example (though you don't have to use a separate thread to use interpolation).

#if WINDOWS
            if (Game.MouseInput.XButton1 == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                //Interpolation isn't used in the demos by default, so passing in a really short time adds a lot of time between discretely visible time steps.
                //Using a Space.TimeStepSettings.TimeStepDuration of 1/60f (the default), this will perform one time step every 20 frames (about three per second at the usual game update rate).
                //This can make it easier to examine behavior frame-by-frame.
                Space.Update(1 / 1200f); 
            }
            else
#endif

            
            // maybe better ? not sure
            //for (int i = 0; i < 100; i++)
            //{
            //    Space.Update(dt/100);
            //}
            Space.Update(dt);
            

            long endTime = Stopwatch.GetTimestamp();
            accumulatedPhysicsTime += (endTime - startTime) / (double)Stopwatch.Frequency;
            accumulatedPhysicsFrames++;
            previousTimeMeasurement += dt;
            if (previousTimeMeasurement > .3f)
            {
                previousTimeMeasurement -= .3f;
                PhysicsTime = accumulatedPhysicsTime / accumulatedPhysicsFrames;
                accumulatedPhysicsTime = 0;
                accumulatedPhysicsFrames = 0;
            }

        }

        public virtual void CleanUp()
        {
            //Undo any in-demo configuration.
            ConfigurationHelper.ApplyDefaultSettings(Space);
            parallelLooper.Dispose();
        }
    }
}
