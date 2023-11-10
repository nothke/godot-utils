using Godot;
using System;
using Nothke;
using System.Collections.Generic;

/// <summary>
/// Adds all nested colliison shapes to the first PhysicsBody3D parent.
/// </summary>
public partial class AddShapesToParent : Node3D
{
	public override void _Ready()
    {
		var body = this.GetFirstParent<PhysicsBody3D>(true);
		
		if (body == null) 
			throw new Exception($"AddShapesToParent: Parent PhysicsBody3D not found. '{Name}' ({this})");
        
		var childShapes = new List<CollisionShape3D>();

		this.GetChildren(childShapes, true);

		foreach (var col in childShapes)
		{
			var ownerId = body.CreateShapeOwner(col);
			body.ShapeOwnerAddShape(ownerId, col.Shape);
			body.ShapeOwnerSetTransform(ownerId, body.GlobalTransform.Inverse() * col.GlobalTransform);
			body.ShapeOwnerSetDisabled(ownerId, col.Disabled);
		}
	}
}
