using Neitri;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyEngine.Components
{
	public abstract class ComponentWithShortcuts : Component
	{
		public InputSystem Input => Entity.Input;
		public MyDebug Debug => Entity.Debug;
		public Factory Factory => Entity.Factory;
		public ILog Log => Entity.Log;
		public FileSystem FileSystem => Entity.FileSystem;
		public Events.EventSystem EventSystem => Entity.EventSystem;



		public SceneSystem Scene => Entity.Scene;
		public Transform Transform => Entity.Transform;

	}
}