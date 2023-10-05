using Godot;
using System;

namespace Nothke
{
	public static class Utils
	{
		public static Vector3 Forward(this Node3D node3D)
		{
			return -node3D.GlobalTransform.Basis.Z;
		}

		public static Vector3 Up(this Node3D node3D)
		{
			return -node3D.GlobalTransform.Basis.Y;
		}

		public static Vector3 Right(this Node3D node3D)
		{
			return node3D.GlobalTransform.Basis.X;
		}
	}
}