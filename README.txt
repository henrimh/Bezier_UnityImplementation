*** Yes the code could use comments if you're wondering. ***

This is a Bezier Spline implementation for Unity.
If you don't know what is a Bezier Spline, do check this out: https://en.wikipedia.org/wiki/Composite_B%C3%A9zier_curve

The implementation can be used to:
- Create meshes (even on runtime)
- Move objects 
- Create objects 
along the spline. And whatever else you figure out it is useful for.
The spline can be created manually in the scene or programmatically during runtime. 

You will find four example scenes in the project:
- Manual Modes Example: Choose the SplineExample game object. In the scene view, you can test the spline by dragging the different colored gizmos around. You can change the modes of the points from the inspector.
- Mesh Creation Example: Pretty much the same as above, but with automatic control point setting and the mesh drawing on top of it.
- Mover Example: Press play and see what happens. You can edit the spline on runtime as well. The thingy will still follow the spline.
- Spline Decorate Example: This is for creating multiple objects along the spline. Could be used for environment art etc. This mode is only a showcase of what could be done with it. As the decorator mode isn't fully completed. (Missing realtime mode, missing object persistence)


Runtime mesh creation doesn't work fully in editor. It works in build. 
Though if the editor is paused and unpaused, the mesh is redrawn.







The implementation was used in the production of Trail of Relics mobile game for Android.
In Trail of Relics the splines were used to draw the characters path in the levels and move visual effects from a to b.
All the splines used in Trail of Relics are created on runtime.
