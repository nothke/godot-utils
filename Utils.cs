using Godot;
using System;
using System.Collections.Generic;

using JP = Godot.Generic6DofJoint3D.Param;

namespace Nothke
{
	public static class Utils
	{
		// Nodes

		/// <summary>
		/// Spawns a Node of type T. If parent is not set, it will be added to the scene root.
		/// </summary>
		public static T Instantiate<T>(this Node node, Node parent = null, bool deferred = false) where T : Node, new()
		{
			var newNode = new T();

			parent ??= node.GetTree().Root;

			if (!deferred)
				parent.AddChild(newNode);
			else
				parent.CallDeferred("add_child", newNode);

			return newNode;
		}

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

		/// <summary>
		/// Returns the first child of type T. Returns null if none is found.
		/// </summary>
		public static T GetFirstChild<T>(this Node node, bool nested = false) where T : Node
		{
			int cc = node.GetChildCount();


			for (int i = 0; i < cc; i++)
			{
				var child = node.GetChild(i);

				if (child is T childT)
					return childT;
			}

			for (int i = 0; i < cc; i++)
			{
				var child = node.GetChild(i);

				T childT = GetFirstChild<T>(child, true);
				if (childT != null)
					return childT;
			}

			return null;
		}

		/// <summary>
		/// Cached variant of node.GetFirstChild<T>().
		/// </summary>
		public static T GetFirstChild<T>(this Node node, ref T cachedNode) where T : Node
		{
			cachedNode ??= node.GetFirstChild<T>();
			return cachedNode;
		}

		/// <summary>
		/// Fills a list with all children of given type. 
		/// It doesn't clear the list, it's up to you to do it.
		/// If nested is true, it will get all subchildren too.
		/// </summary>
		public static void GetChildren<T>(this Node node, List<T> childrenList, bool nested = false) where T: Node
		{
			int cc = node.GetChildCount();
			for (int i = 0; i < cc; i++)
			{
				var child = node.GetChild(i);
				if (child is T t)
				{
					childrenList.Add(t);
				}

				if (nested)
					child.GetChildren(childrenList, true);
			}
		}

		/// <summary>
		/// Climbs up the hierarchy until it finds a node of type T.
		/// Returns null if none is found.
		/// </summary>
		public static T GetFirstParent<T>(this Node node, bool includeSelf = false) where T: Node
		{
			var parent = includeSelf ? node : node.GetParent();

			if (parent == null)
				return null;

			if (parent is T t)
				return t;
			else 
				return parent.GetFirstParent<T>();
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

		// Thanks to SpaceJellyfishDev:
		public static Vector3 TransformPoint(this Node3D node, Vector3 localPoint) => node.GlobalTransform * localPoint;
		public static Vector3 InverseTransformPoint(this Node3D node, Vector3 worldPoint) => worldPoint * node.GlobalTransform;//or  node.GlobalTransform.affine_inverse() * worldPoint
		public static Vector3 TransformDirection(this Node3D node, Vector3 localDirection) => node.Transform.Basis * localDirection;
		public static Vector3 InverseTransformDirection(this Node3D node, Vector3 localDirection) => localDirection * node.Transform.Basis;
		public static Vector3 TransformVector(this Node3D node, Vector3 localVector) => node.TransformPoint(localVector) - node.GlobalPosition;

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

		public static void ApplyForceAtPosition(this RigidBody3D rb, Vector3 globalForce, Vector3 globalPosition)
		{
			rb.ApplyForce(globalForce, globalPosition - rb.GlobalPosition);
		}

		public static void ApplyForceAtPositionLocal(this RigidBody3D rb, Vector3 localForce, Vector3 localPosition)
		{
			Vector3 globalForce = rb.TransformDirection(localForce);
			Vector3 globalPosition = rb.TransformPoint(localPosition);
			rb.ApplyForce(globalForce, globalPosition - rb.GlobalPosition);
		}

        /// <summary>
        /// Adds all nested colliison shapes of node to body.
        /// </summary>
        public static void AddNestedShapesToBody(this Node node, PhysicsBody3D body)
		{
            var childShapes = new List<CollisionShape3D>();
            node.GetChildren(childShapes, true);

            foreach (var col in childShapes)
            {
                var ownerId = body.CreateShapeOwner(col);
                body.ShapeOwnerAddShape(ownerId, col.Shape);
                body.ShapeOwnerSetTransform(ownerId, body.GlobalTransform.Inverse() * col.GlobalTransform);
                body.ShapeOwnerSetDisabled(ownerId, col.Disabled);
            }
        }

        /// <summary>
        /// Adds all nested colliison shapes to the first PhysicsBody3D parent.
		/// Throws exception if none is found.
        /// </summary>
        public static void AddNestedShapesToFirstParentBody(this Node node)
		{
            var body = node.GetFirstParent<PhysicsBody3D>(true) ?? throw new Exception("AddNestedShapesToFirstParentBody: Parent PhysicsBody3D not found");
			node.AddNestedShapesToBody(body);
        }

		// Raycast

		/// <summary>
		/// Gets the shape node that raycast has hit. This is usually a CollisionShape3D.
		/// Returns null if none exists, or the shape owner is not a node.
		/// </summary>
		public static Node GetShapeNode(this RayCast3D raycast)
		{
			if (!raycast.IsColliding())
				return null;

			var hitCollider = raycast.GetCollider();
			var shapeId = raycast.GetColliderShape();

			if (hitCollider is CollisionObject3D collisionObj)
			{
				var ownerId = collisionObj.ShapeFindOwner(shapeId);
				var ownerObject = collisionObj.ShapeOwnerGetOwner(ownerId);

				if (ownerObject is Node shapeNode)
					return shapeNode;
			}

			return null;
		}

		// Joints

		public static void Detach(this Joint3D joint)
		{
			joint.NodeA = null;
			joint.NodeB = null;
		}

		/// <summary>
		/// If nothing is passed for otherBody, it will fix to world
		/// </summary>
		public static Generic6DofJoint3D CreateFixedJoint(this PhysicsBody3D body, PhysicsBody3D otherBody = null)
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
