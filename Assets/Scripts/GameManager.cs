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

    public string grid_rows_ = "5";
    public string grid_columns_ = "5";

    #endregion

    #region Private Members

    /** The simulator used to run the game of life. */
    private GameSimulator game_simulator_;

    /** <summary> The grid containing the data about all the cells. </summary> */
    private GameSimulator.CELL_STATE[ , ] game_grid_;

    /** <summary> The settings window rect reference. </summary> */
    private Rect settings_window_ = new Rect( 0 , 0 , 150 , 200 );

    private bool b_is_simulation_running_ = false;

    private bool b_is_window_settings_enabled_ = true;

    #endregion

    #region Public Methods

    /**
     * <summary> Changes the cell state in the given row and column. </summary>
     * <param name="_cell_row"> The row index of the cell to change. </param name="_cell_row">
     * <param name="_cell_column"> The column index of the cell to change. </param name="_cell_column">
     * <param name="_new_cell_state"> The new state of the cell. </param name="_new_cell_state">
     */
    public void changeCellState(int _cell_row, int _cell_column, GameSimulator.CELL_STATE _new_cell_state)
    {
        game_grid_[ _cell_row , _cell_column ] = _new_cell_state;
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
        if ( Input.GetKeyDown( KeyCode.Escape ) )
        {
            b_is_window_settings_enabled_ = !b_is_window_settings_enabled_;
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

        // I need to destroy the previous cells (they are tagged as "Cell") to avoid conflicts with the new ones.
        GameObject[] temp_previous_cells = GameObject.FindGameObjectsWithTag( "Cell" );
        foreach ( GameObject temp_previous_cell in temp_previous_cells )
        {
            Destroy( temp_previous_cell );
        }

        // Create a new empty grid.
        game_grid_ = new GameSimulator.CELL_STATE[ local_grid_rows , local_grid_columns ];

        // Temporary game object used to store the reference for the newly created cell so that I can parent it in the scene hierarchy.
        GameObject temp_cell_reference;

        // Temporary reference to the GRID in the hierarchy used for easier organization of the objects and parenting.
        GameObject temp_hierarchy_reference = GameObject.Find( "GRID" );
        for ( int cycle_row_index = 0 ; cycle_row_index < local_grid_rows ; cycle_row_index++ )
        {
            for ( int cycle_column_index = 0 ; cycle_column_index < local_grid_columns ; cycle_column_index++ )
            {
                temp_cell_reference = (GameObject) Instantiate( cell_prefab_ , new Vector2( cycle_column_index - local_grid_columns / 2 , -cycle_row_index + local_grid_rows / 2 ) , Quaternion.identity );
                temp_cell_reference.transform.parent = temp_hierarchy_reference.transform;
                CellProperties temp_cell_properties = temp_cell_reference.GetComponent<CellProperties>();
                temp_cell_properties.cell_column_ = cycle_column_index;
                temp_cell_properties.cell_row_ = cycle_row_index;
                temp_cell_properties.cell_state_ = GameSimulator.CELL_STATE.DEAD;
                game_grid_[ cycle_row_index , cycle_column_index ] = GameSimulator.CELL_STATE.DEAD;
            }
        }
    }

    /**
     * Responsible of drawing the settings window.
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
        }

        // Starts the simulation.
        if ( GUILayout.Button( "Start" ) )
        {
            b_is_simulation_running_ = true;
        }

        // Stops the simulation
        if ( GUILayout.Button( "Stop" ) )
        {
            b_is_simulation_running_ = false;
        }

        GUI.DragWindow();
    }

    #endregion
}
