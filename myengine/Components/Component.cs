using MyEngine;
using Neitri;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MyEngine.Components
{
	public class Component : IComponent
	{
		public Component(Entity entity)
		{
			this.entity = entity;
		}

		/// <summary>
		/// The Entity that this Component is attached to
		/// </summary>
		public Entity Entity
		{
			get
			{
				if (entity == null) throw new NullReferenceException(typeof(Entity) + " is null");
				return entity;
			}
		}

		private Entity entity;
	}
}