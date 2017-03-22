using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace MyEngine.Components
{

	[ComponentSetting(allowMultiple = false)]
	public partial class Transform : Component
	{
		WorldPos position = WorldPos.Zero;
		Vector3 scale = Vector3.One;
		Quaternion rotation = Quaternion.Identity;

		public Transform(Entity entity) : base(entity)
		{
		}

		public WorldPos Position { set { position = value; } get { return position; } }
		public Vector3 Scale { set { scale = value; } get { return scale; } }
		public Quaternion Rotation { set { rotation = value; } get { return rotation; } }


		public Vector3 Right { get { return Constants.Vector3Right.RotateBy(Rotation); } }
		public Vector3 Up { get { return Constants.Vector3Up.RotateBy(Rotation); } }
		public Vector3 Forward { get { return Constants.Vector3Forward.RotateBy(Rotation); } set { this.Rotation = value.LookRot(); } }


		public Matrix4 GetLocalToWorldMatrix(WorldPos viewPointPos)
		{
			return
				Matrix4.CreateScale(Scale) *
				Matrix4.CreateFromQuaternion(Rotation) *
				Matrix4.CreateTranslation(viewPointPos.Towards(Position).ToVector3());
		}

		public Matrix4 GetWorldToLocalMatrix(WorldPos viewPointPos)
		{
			return Matrix4.Invert(GetLocalToWorldMatrix(viewPointPos));
		}

		public void Translate(Vector3 translation, Space relativeTo = Space.Self)
		{
			if (relativeTo == Space.Self)
			{
				var m = Matrix4.CreateTranslation(translation) * Matrix4.CreateFromQuaternion(Rotation);
				this.Position += m.ExtractTranslation();
			}
			else if (relativeTo == Space.World)
			{
				this.Position += translation;
			}
		}
		
		public void LookAt(WorldPos worldPosition, Vector3 worldUp)
		{
			this.Rotation = Matrix4.LookAt(Vector3.Zero, this.Position.Towards(worldPosition).ToVector3(), worldUp).ExtractRotation();
		}
		public void LookAt(WorldPos worldPosition)
		{
			var dir = this.Position.Towards(worldPosition);
			this.Rotation = dir.ToVector3().LookRot();
		}

	}
}
