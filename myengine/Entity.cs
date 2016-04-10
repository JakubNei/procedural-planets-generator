using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using MyEngine.Events;
using MyEngine.Components;

namespace MyEngine
{
    public class Entity : IDisposable
    {
        public EventSystem EventSystem { get; private set; }

        public Transform Transform { get; private set; }

        public SceneSystem Scene { get; private set; }
        public InputSystem Input { get; private set; }

        public IReadOnlyList<Component> Components
        {
            get
            {
                return components.AsReadOnly();
            }
        }


        List<Component> components = new List<Component>();


        public Entity(SceneSystem scene, string name = "")
        {
            this.Transform = this.AddComponent<Transform>();
            this.Scene = scene;
            this.Input = Scene.Input;
            EventSystem = new EventSystem();
        }

        public T GetComponent<T>() where T : Component
        {
            lock(components)
            {
                foreach (var c in components)
                {
                    if (c is T)
                    {
                        return c as T;
                    }
                }
            }
            return null;
        }

        public T[] GetComponents<T>() where T : Component
        {
            List<T> ret = new List<T>();
            lock(components)
            {
                foreach (var c in components)
                {
                    if (c is T)
                    {
                        ret.Add(c as T);
                    }
                }
            }
            return ret.ToArray();
        }

        public T AddComponent<T>() where T : Component
        {
            if (typeof(T) == typeof(Transform))
            {
                if (GetComponent<T>() != null)
                {
                    Debug.Warning("Attempting to add " + typeof(Transform) + " component, but one is already on this " + typeof(Entity));
                    return null;
                }
            }
            T c = System.Activator.CreateInstance(typeof(T), this) as T;
            lock (components)
            {
                components.Add(c);
            }
            return c;
        }

        public void DestroyComponent(Component component)
        {
            lock(components)
            {
                if (components.Contains(component))
                {
                    components.Remove(component);
                } else
                {
                    return;
                }
            }
            if (component is IDisposable)
            {
                (component as IDisposable).Dispose();
            }
        }



        public Action<ChangedFlags> OnChanged;

        internal void RaiseOnChanged(ChangedFlags flags)
        {
            if (OnChanged != null) OnChanged(flags);
        }

        public void Dispose()
        {
            foreach(var c in components)
            {
                DestroyComponent(c);
            }
            Scene.Remove(this);
        }
    }

    public enum ChangedFlags
    {
        Position = 1<<0,
        Rotation = 1 << 1,
        Scale = 1 << 2,
        Bounds = 1 << 3,
        VisualRepresentation = 1 << 4,
        PhysicsSettings = 1 << 5,
        PhysicalShape = 1 << 6,
        All = 0xffff
    }
}
