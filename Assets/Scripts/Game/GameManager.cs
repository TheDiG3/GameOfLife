using UnityEngine;
using System.Collections;

/**
 * This class handles the user interface as well as the creation of the game simulator needed to simulate a Game Of Life.
 */
public class GameManager : MonoBehaviour
{
    #region Public Members

    /** The time in seconds for the simulation loop. */
    public float simulation_time_ = 0.3F;

    public GameObject cell_prefab_;
    public Material dead_cell_material_;
    public Material alive_cell_material_;

    public string grid_rows_ = "5";
    public string grid_columns_ = "5";

    #endregion

    #region Private Members

    /** The simulator used to run the game of life. */
    private GameSimulator game_simulator_ = null;

    /** <summary> The grid containing the data about all the cells. </summary> */
    private GameSimulator.ECellState[, ] game_grid_;

    /** <summary> The grid containing all the references to the game objects. </summary> */
    private GameObject[, ] game_objects_grid_;

    /** <summary> The settings window rect reference. </summary> */
    private Rect settings_window_ = new Rect( 0 , 0 , 230 , 300 );

    private bool b_is_simulation_running_ = false;
    private bool b_is_window_settings_enabled_ = true;
    private bool b_use_multithreading_ = false;

    /** <summary> Counts the time since the last GameSimulator.simulate was called. </summary> */
    private float time_elapsed_since_last_simulation_ = 0;

    private float last_simulation_duration_;

    #endregion

    #region Public Methods

    /**
     * <summary> Changes the cell state in the given row and column. </summary>
     * <param name="_cell_row"> The row index of the cell to change. </param>
     * <param name="_cell_column"> The column index of the cell to change. </param>
     * <param name="_new_cell_state"> The new state of the cell. </param>
     */
    public void changeCellState( int _cell_row , int _cell_column , GameSimulator.ECellState _new_cell_state )
    {
        game_grid_[ _cell_row , _cell_column ] = _new_cell_state;
        if ( _new_cell_state == GameSimulator.ECellState.ALIVE )
        {
            game_objects_grid_[ _cell_row , _cell_column ].GetComponent<Renderer>().material = alive_cell_material_;
        }
        else
        {
            game_objects_grid_[ _cell_row , _cell_column ].GetComponent<Renderer>().material = dead_cell_material_;
        }

        // If the grid was rebuilt then the game simulator will be set back to null since no simulation is running.
        // The new grid assignment needs to be done only if the simulation is already running and the use changes any of the states of the cells.
        if ( game_simulator_ != null )
        {
            game_simulator_.setCellGridState( game_grid_ );
        }
    }

    #endregion

    #region Private Methods

    // Use this for initialization
    void Start()
    {
        buildGrid();
    }

    // Update is called once per frame
    void Update()
    {
        time_elapsed_since_last_simulation_ += Time.deltaTime;

        // Toggles the settings window.
        if ( Input.GetKeyDown( KeyCode.Escape ) )
        {
            b_is_window_settings_enabled_ = !b_is_window_settings_enabled_;
        }

        // Accelerates the simulation
        if ( Input.GetKey( KeyCode.E ) )
        {
            if ( simulation_time_ > 0.001F )
            {
                simulation_time_ /= 1.2F;
            }
        }
        // Slows the simulation
        else if ( Input.GetKey( KeyCode.Q ) )
        {
            if ( simulation_time_ < 100F )
            {
                simulation_time_ *= 1.2F;
            }
        }

        // If enough time has passed we need to run another simulation. I also need to make sure that the simulation is running.
        if ( b_is_simulation_running_ && ( time_elapsed_since_last_simulation_ >= simulation_time_ ) )
        {
            GameSimulator.SimulationStep temp_simulation_step = game_simulator_.Simulate();

            // Under multithreading environment the simulation queue might be empty if the render thread is faster than the simulating thread
            if ( temp_simulation_step != null )
            {
                game_grid_ = temp_simulation_step.cell_grid;
                last_simulation_duration_ = temp_simulation_step.time_to_simulate;
            }

            updateGridGraphics();

            time_elapsed_since_last_simulation_ = 0;
        }
    }

    // Code for the user interface.
    void OnGUI()
    {
        if ( b_is_window_settings_enabled_ )
        {
            settings_window_ = GUI.Window( 0 , settings_window_ , settingsWindow , "Settings" );
        }
    }

    /**
     * <summary> Builds the grid based on the rows and columns specified inside the class. </summary>
     */
    private void buildGrid()
    {
        int local_grid_rows = int.Parse( grid_rows_ );
        int local_grid_columns = int.Parse( grid_columns_ );

        // This is actually only needed for the first time the buildGrid method is called
        // and to make sure that the game objects grid contains any object to destroy.
        if ( game_objects_grid_ != null )
        {
            // I need to destroy the previous cells to avoid conflicts with the new ones.
            foreach ( GameObject temp_previous_cell in game_objects_grid_ )
            {
                Destroy( temp_previous_cell );
            }
        }

        // Creates a new empty grid.
        game_grid_ = new GameSimulator.ECellState[ local_grid_rows, local_grid_columns ];
        game_objects_grid_ = new GameObject[ local_grid_rows, local_grid_columns ];

        // Temporary game object used to store the reference for the newly created cell so that I can parent it in the scene hierarchy.
        GameObject temp_cell_reference;

        // Temporary reference to the GRID in the hierarchy used for easier organization of the objects and parenting.
        GameObject temp_hierarchy_reference = GameObject.Find( "GRID" );
        for ( int cycle_row_index = 0 ; cycle_row_index < local_grid_rows ; cycle_row_index++ )
        {
            for ( int cycle_column_index = 0 ; cycle_column_index < local_grid_columns ; cycle_column_index++ )
            {
                temp_cell_reference = (GameObject) Instantiate( cell_prefab_ ,
                                                                new Vector2( cycle_column_index - local_grid_columns / 2 , -cycle_row_index + local_grid_rows / 2 ) ,
                                                                Quaternion.identity );
                temp_cell_reference.transform.parent = temp_hierarchy_reference.transform;
                CellProperties temp_cell_properties = temp_cell_reference.GetComponent<CellProperties>();
                temp_cell_properties.cell_column_ = cycle_column_index;
                temp_cell_properties.cell_row_ = cycle_row_index;
                temp_cell_properties.cell_state_ = GameSimulator.ECellState.DEAD;
                game_grid_[ cycle_row_index , cycle_column_index ] = GameSimulator.ECellState.DEAD;
                game_objects_grid_[ cycle_row_index , cycle_column_index ] = temp_cell_reference;
            }
        }
    }

    /**
     * <summary> Updates the materials of the game objects grid for each cell based on the state found in the game grid. </summary>
     * Also updates the cell properties of each cell to make running state changes by the user possible.
     */
    private void updateGridGraphics()
    {
        for ( int cycle_row_index = 0 ; cycle_row_index < game_grid_.GetLength( 0 ) ; cycle_row_index++ )
        {
            for ( int cycle_column_index = 0 ; cycle_column_index < game_grid_.GetLength( 1 ) ; cycle_column_index++ )
            {
                if ( game_grid_[ cycle_row_index , cycle_column_index ] == GameSimulator.ECellState.ALIVE )
                {
                    game_objects_grid_[ cycle_row_index , cycle_column_index ].GetComponent<Renderer>().material = alive_cell_material_;
                }
                else
                {
                    game_objects_grid_[ cycle_row_index , cycle_column_index ].GetComponent<Renderer>().material = dead_cell_material_;
                }

                game_objects_grid_[ cycle_row_index , cycle_column_index ].GetComponent<CellProperties>().cell_state_ = game_grid_[ cycle_row_index , cycle_column_index ];
            }
        }
    }

    /**
     * Responsible for drawing the settings window.
     */
    private void settingsWindow( int _windowID )
    {
        GUILayout.Label( "Rows" );
        grid_rows_ = GUILayout.TextField( grid_rows_ );
        GUILayout.Label( "Columns" );
        grid_columns_ = GUILayout.TextField( grid_columns_ );

        // Rebuilds the grid.
        if ( GUILayout.Button( "Rebuild" ) )
        {
            buildGrid();

            if ( game_simulator_ != null )
            {
                game_simulator_.Dispose();
            }

            // By setting the game simulator to null I can ensure that no simulation is running whenever the "Start" button is clicked afterwards.
            game_simulator_ = null;

            b_is_simulation_running_ = false;
        }

        // Starts the simulation.
        if ( GUILayout.Button( "Start" ) )
        {
            b_is_simulation_running_ = true;

            // If the game simulator is set to null it means that we need a new game simulator object to run the simulation since we've most likely rebuilt the grid.
            if ( game_simulator_ == null )
            {
                game_simulator_ = new GameSimulator( game_grid_ , b_use_multithreading_ , true );
            }
            else
            {
                game_simulator_.IsSimulationRunning = true;
                // I need to reset the threading environment since it might have changed if the user clicked on the "Step" button
                game_simulator_.UseThreading = b_use_multithreading_;
            }
        }

        // Stops the simulation
        if ( GUILayout.Button( "Stop" ) )
        {
            b_is_simulation_running_ = false;
            game_simulator_.IsSimulationRunning = false;
        }

        // Runs one step of the simulation
        if ( GUILayout.Button( "Step (Disables Multithreading)" ) )
        {
            b_is_simulation_running_ = false;
            b_use_multithreading_ = false;

            // I can only run valid simulations. If no simulations are found I start a new one without multithreading and ticking.
            if ( game_simulator_ == null )
            {
                game_simulator_ = new GameSimulator( game_grid_ , false , false );
            }
            else
            {
                game_simulator_.IsSimulationRunning = false;
                game_simulator_.UseThreading = false;
            }

            GameSimulator.SimulationStep temp_simulation_step = game_simulator_.Simulate();
            game_grid_ = temp_simulation_step.cell_grid;
            last_simulation_duration_ = temp_simulation_step.time_to_simulate;

            updateGridGraphics();
        }

        if ( GUILayout.Toggle( b_use_multithreading_ , "Multithreading" ) != b_use_multithreading_ )
        {
            b_use_multithreading_ = !b_use_multithreading_;

            // If we still have not created a simulation create a non ticking, single threaded one
            // Threading environment will be set immediately after and ticking should only start with the appropriate button
            if ( game_simulator_ == null )
            {
                game_simulator_ = new GameSimulator( game_grid_ , false , false );
            }

            game_simulator_.UseThreading = b_use_multithreading_;
        }

        GUILayout.Label( "Last simulation time: " + last_simulation_duration_ + "ms" );

        GUI.DragWindow();
    }

    // We need to stop the thread we created
    public void OnDestroy()
    {
        if ( game_simulator_ != null )
        {
            game_simulator_.Dispose();
        }
    }

    #endregion

}
