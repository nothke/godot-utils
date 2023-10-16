using Godot;
using System;

using JP = Godot.Generic6DofJoint3D.Param;

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

		/// <summary>
		/// Reparents a node to another parent. If node is Node3D, it attempts to keep the global transform.
		/// If parentTo is null, it parents node to the scene root.
		/// </summary>
		public static void SetParent(this Node node, Node parentTo)
		{
			Node3D node3D = node as Node3D;
			Transform3D transform = default;

			if (node3D != null)
				transform = node3D.GlobalTransform;

			parentTo ??= node.GetTree().Root;

			node.GetParent().RemoveChild(node);
			parentTo.AddChild(node);

			if (node3D != null)
				node3D.GlobalTransform = transform;
		}

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

		/// <summary>
		/// Resets node's transform in relation to the parent.
		/// Position becomes 0,0,0, rotation 0,0,0, and scale 1,1,1.
		/// </summary>
		public static void ZeroOut(this Node3D node3D)
		{
			node3D.Transform = Transform3D.Identity;
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

		/// <summary>
		/// If nothing is passed for otherBody, it will fix to world
		/// </summary>
		public static Generic6DofJoint3D CreateFixedJoint(this RigidBody3D body, RigidBody3D otherBody = null)
		{
			var joint = Instantiate<Generic6DofJoint3D>(body);
			joint.ZeroOut();

			joint.NodeA = body.GetPath();
			joint.NodeB = otherBody?.GetPath();

			return joint;
		}

		public struct JointOptions
		{
			public bool anchorInWorldSpace = false;
			public Vector3 anchorOffset = default;
			public Vector3 anchorRotation = default;

			/// <summary>
			/// The object will be relocated to this position before joining, it may jerk
			/// </summary>
			public Vector3 targetPosition = default;

			public Vector3 lowAngularLimit = default;
			public Vector3 highAngularLimit = default;
			public Vector3 lowLinearLimit = default;
			public Vector3 highLinearLimit = default;
			public bool unlimitedAngularMotion = false;
			public bool unlimitedLinearMotion = false;

			public float angularSpringStiffness = 0;
			public float angularSpringDamping = 0;
			public float linearSpringStiffness = 0;
			public float linearSpringDamping = 0;

			public JointOptions() { }
		};

		public static Generic6DofJoint3D CreateJoint(this RigidBody3D body, RigidBody3D otherBody, in JointOptions options)
		{
			var joint = Instantiate<Generic6DofJoint3D>(options.anchorInWorldSpace ? body.GetTree().Root : body);

			joint.ZeroOut();
			joint.Position = options.anchorOffset;
			joint.Rotation = options.anchorRotation;

			if (options.unlimitedAngularMotion)
			{
				joint.Set("angular_limit_x/enabled", false);
				joint.Set("angular_limit_y/enabled", false);
				joint.Set("angular_limit_z/enabled", false);
			}

			joint.SetParamX(JP.AngularLowerLimit, options.lowAngularLimit.X);
			joint.SetParamY(JP.AngularLowerLimit, options.lowAngularLimit.Y);
			joint.SetParamZ(JP.AngularLowerLimit, options.lowAngularLimit.Z);

			joint.SetParamX(JP.AngularUpperLimit, options.highAngularLimit.X);
			joint.SetParamY(JP.AngularUpperLimit, options.highAngularLimit.Y);
			joint.SetParamZ(JP.AngularUpperLimit, options.highAngularLimit.Z);

			if (options.angularSpringStiffness > 0)
			{
				joint.Set("angular_spring_x/enabled", true);
				joint.Set("angular_spring_y/enabled", true);
				joint.Set("angular_spring_z/enabled", true);

				joint.SetParamX(JP.AngularSpringStiffness, options.angularSpringStiffness);
				joint.SetParamY(JP.AngularSpringStiffness, options.angularSpringStiffness);
				joint.SetParamZ(JP.AngularSpringStiffness, options.angularSpringStiffness);

				joint.SetParamX(JP.AngularSpringDamping, options.angularSpringDamping);
				joint.SetParamY(JP.AngularSpringDamping, options.angularSpringDamping);
				joint.SetParamZ(JP.AngularSpringDamping, options.angularSpringDamping);
			}

			if (options.linearSpringStiffness > 0)
			{
				joint.Set("linear_spring_x/enabled", true);
				joint.Set("linear_spring_y/enabled", true);
				joint.Set("linear_spring_z/enabled", true);

				joint.SetParamX(JP.LinearSpringStiffness, options.linearSpringStiffness);
				joint.SetParamY(JP.LinearSpringStiffness, options.linearSpringStiffness);
				joint.SetParamZ(JP.LinearSpringStiffness, options.linearSpringStiffness);

				joint.SetParamX(JP.LinearSpringDamping, options.linearSpringDamping);
				joint.SetParamY(JP.LinearSpringDamping, options.linearSpringDamping);
				joint.SetParamZ(JP.LinearSpringDamping, options.linearSpringDamping);
			}

			joint.NodeA = body.GetPath();
			joint.NodeB = otherBody?.GetPath();

			return joint;
		}
	}
}
