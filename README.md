Game Of Life
==========

Commands
---------
- Click : toggles the state of the cell between DEAD and ALIVE
- Q : slows the simulation
- E : accelerates the simulation
- W : pans the camera up
- S : pans the camera down
- A : pans the camera left
- D : pans the camera right
- Mouse Wheel : Zoom
- Keypad + : accelerates the camera movement
- Keypad - : slows the camera movement
- ESCAPE : Toggles the settings menu 

==========

The Implementation
------

The simulation works by creating a **GameSimulator** object which handles all the data needed regarding the cells (at this stage each cell is only defined by its state: ALIVE or DEAD) by working with a **two-dimensional array**.

The size of the array can be decided at runtime by changing the corresponding values from inside the settings window and then rebuilding the grid.

The actual simulation is carried over in the object method **GameSimulator.simulate**. Inside the method, as explained in the code comments, a new array is created in order to store the result of the simulation made on the current processed step. This is necessary because the simulation is actually just a **function which takes in the current grid as input and returns the resulting next step as output** based on the classic rules of the Game Of Life. The calculations are supposed to be executed **simultaneously** for all the cells. This can be achieved by making so that the partial computations of the next step don't affect "concurrent" computations of other cells states.

After creating the holding array the method computes the **neighbors count** for each cell, taking into consideration **array wrapping**. This basically ensures that going outside the array bounds is actually seen as emerging from the opposite side of the array; so an index of -1 on the row would represent the last index possible for the row. -- Old code for non-wrapping grids is still present inside the method.

Simulation Wrapping
---

The whole simulation is run for aestethic purposes inside the **Unity Game Engine**.
More specifically, a **GameManager** object is created, which takes care of the **UI for the simulation settings**, the **speed of the simulation** and **start/stop** of it.

It's up to the user to decide whether to let the simulation run on its own or **step by step**.

The **GameManager** class also allows the user to modify the simulation grid in realtime by **changing the state of any individual cell** and then updating the internal grid of the **GameSimulator** object.

Visually the **GameManager** also updates the graphics of each cell just by **changing the material based on the current state of the cell**.

Recommendations
---

While it's not recommended to set the grid size any **higher than 50x50** for performance reasons (the game might lock up at 100x100 or more) the bigger the grid the most interesting the simulation will be.

Web Player Link
---
https://dl.dropboxusercontent.com/u/14583828/Game%20Of%20Life/Web%20Player/Web%20Player.html




























