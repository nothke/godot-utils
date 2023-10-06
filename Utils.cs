using Godot;
using System;

namespace Nothke
{
	public static class Utils
	{
		// Nodes

		/// <summary>
		/// Creates a Node of type T and parents to parent or if null it attaches it to the scene root
		/// </summary>
		public static T Instantiate<T>(this Node node, Node parent = null) where T : Node, new()
		{
			var newNode = new T();

			if (parent == null)
				node.GetTree().Root.AddChild(newNode);
			else
				parent.AddChild(newNode);

			return newNode;
		}

		// 3D

		public static Vector3 Forward(this Node3D node3D)
		{
			return -node3D.GlobalTransform.Basis.Z;
		}

		public static Vector3 Up(this Node3D node3D)
		{
			return node3D.GlobalTransform.Basis.Y;
		}

		public static Vector3 Right(this Node3D node3D)
		{
			return node3D.GlobalTransform.Basis.X;
		}

		// Input

		/// <summary>
		/// Returns -1 when negative is pressed and 1 when positive is pressed, 0 when nothing
		/// </summary>
		public static float KeyAxisRaw(Key negative, Key positive)
		{
			return Input.IsKeyPressed(positive) ? 1 : (Input.IsKeyPressed(negative) ? -1 : 0);
		}

		// Physics

		public static void Detach(this Joint3D joint)
		{
			joint.NodeA = null;
			joint.NodeB = null;
		}
	}
}
