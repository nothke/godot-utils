A couple of utilities for Godot, started while working on my Limited Space LDJAM game.

Right now they're all in Utils.cs, simply include `using Nothke;` and you're good to go.

Most utils are implemented as extension methods. If you want to use them on the self, you need to write them like `this.Forward()`. 
- Note that a lot of functions use "this" simply to get a reference to the scene the node belongs to, like Raycast().