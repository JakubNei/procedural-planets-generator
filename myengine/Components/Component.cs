using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using MyEngine;

namespace MyEngine.Components
{
    public class Component
    {
        public Component(Entity entity)
        {
            this.m_entity = entity;
        }

        /// <summary>
        /// The Entity that this Component is attached to
        /// </summary>
        public Entity Entity
        {
            get
            {
                if (m_entity == null) throw new NullReferenceException(typeof(Entity) + " is null");
                return m_entity;
            }
        }
        private Entity m_entity;

        
    }
}
