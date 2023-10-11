using Godot;
using System;

namespace Nothke
{
	public static class Utils
	{
		// Nodes

		/// <summary>
		/// Spawns a Node of type T. If parent is not set, it will be added to the scene root.
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

		// Materials

		public static T CloneMaterial<T>(this MeshInstance3D mesh, int surface = 0) where T : BaseMaterial3D
		{
			var mat = mesh.GetActiveMaterial(surface);
			var newMat = mat.Duplicate(false) as T;
			mesh.MaterialOverlay = newMat;
			return newMat;
		}

		public static T GetOrCloneMaterial<T>(this MeshInstance3D mesh, ref T mat) where T : BaseMaterial3D
		{
			if (mat == null)
				return CloneMaterial<T>(mesh);
			else return mat;
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

		public static void KillMotion(this RigidBody3D rb)
		{
			rb.LinearVelocity = Vector3.Zero;
			rb.AngularVelocity = Vector3.Zero;
		}

		public static void Detach(this Joint3D joint)
		{
			joint.NodeA = null;
			joint.NodeB = null;
		}
	}
}
