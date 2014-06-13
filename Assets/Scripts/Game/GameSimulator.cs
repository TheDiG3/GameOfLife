using UnityEngine;

/**
 * <summary> This class stores the game of life data and handles the simulation process. </summary>
 */
public class GameSimulator
{

    #region Public Members

    public enum CELL_STATE
    {
        DEAD ,
        ALIVE
    }

    #endregion

    #region Private Members

    /** <summary> The grid containing the states of all the cells. </summary> */
    private CELL_STATE[ , ] cell_grid_;

    #endregion


    #region Public Methods

    /**
     * <summary> Constructs the game simulator object with the given cell grid. </summary>
     * <param name="_cell_grid"> The cell grid to use. </param>
     */
    public GameSimulator( CELL_STATE[ , ] _cell_grid )
    {
        cell_grid_ = _cell_grid;
    }

    /**
     * <summary> Simulates one cycle of the game of life. </summary>
     */
    public void simulate()
    {
        // Since the simulation is basically a function that outputs a new grid I need a new array to store the new states of the cell
        // in order to avoid interferences on the current state of the game grid if new cells spawn or some die.
        // This array will then become the new game grid.
        CELL_STATE[ , ] local_next_step_game_grid = new CELL_STATE[ this.cell_grid_.GetLength( 0 ) , this.cell_grid_.GetLength( 1 ) ];

        // Counts the neighbors of each cell and spawns or kills based on the rules of the game.
        byte local_neighbors_count;
        for ( int cycle_row_index = 0 ; cycle_row_index < cell_grid_.GetLength( 0 ) ; cycle_row_index++ )
        {
            for ( int cycle_column_index = 0 ; cycle_column_index < cell_grid_.GetLength( 1 ) ; cycle_column_index++ )
            {
                local_neighbors_count = getNeighborsCount( cycle_row_index , cycle_column_index );

                // Count == 3 => New cell
                // Count < 2 => Cell dies
                // Count > 3 => Cell dies
                // Else => The same value as the original step is applied.
                if ( local_neighbors_count == 3 )
                {
                    local_next_step_game_grid[ cycle_row_index , cycle_column_index ] = CELL_STATE.ALIVE;
                }
                else if ( local_neighbors_count < 2 )
                {
                    local_next_step_game_grid[ cycle_row_index , cycle_column_index ] = CELL_STATE.DEAD;
                }
                else if ( local_neighbors_count > 3 )
                {
                    local_next_step_game_grid[ cycle_row_index , cycle_column_index ] = CELL_STATE.DEAD;
                }
                else
                {
                    local_next_step_game_grid[ cycle_row_index , cycle_column_index ] = cell_grid_[ cycle_row_index , cycle_column_index ];
                }
            }
        }

        cell_grid_ = local_next_step_game_grid;
    }

    /**
     * <summary> Returns the bidimensional array containing the states of the cells. </summary>
     */
    public CELL_STATE[ , ] getCellGridState()
    {
        return cell_grid_;
    }

    /**
     * <summary> Sets the bidimensional array containing the states of the cells. </summary>
     */
    public void setCellGridState( CELL_STATE[ , ] _cell_grid )
    {
        this.cell_grid_ = _cell_grid;
    }

    #endregion

    #region Private Methods

    /** <summary> Mod function (not remainder) </summary> */
    private int modFunction( int _a , int _b )
    {
        return _a - ( _b * Mathf.FloorToInt( (float) _a / (float) _b ) );
    }

    /**
     * <summary> Gets the total count of how many alive cells surround the cell situated in the specified row and column of the grid. </summary>
     * <param name="_cell_row"> The row of the cell to check for neighbors. </param>
     * <param name="_cell_column"> The column of the cell to check for neighbors. </param>
     * <returns> The count of the alive neighboring cells. </returns>
     */
    private byte getNeighborsCount( int _cell_row , int _cell_column )
    {
        byte local_neighbors_count = 0;

        int temp_check_row;
        int temp_check_column;

        // Top-Left
        temp_check_row = modFunction( _cell_row - 1 , cell_grid_.GetLength( 0 ) );
        temp_check_column = modFunction( _cell_column - 1 , cell_grid_.GetLength( 1 ) );

        if ( this.cell_grid_[ temp_check_row , temp_check_column ] == CELL_STATE.ALIVE )
        {
            ++local_neighbors_count;
        }

        // Top-Center
        temp_check_row = modFunction( _cell_row - 1 , cell_grid_.GetLength( 0 ) );
        temp_check_column = modFunction( _cell_column , cell_grid_.GetLength( 1 ) );

        if ( this.cell_grid_[ temp_check_row , temp_check_column ] == CELL_STATE.ALIVE )
        {
            ++local_neighbors_count;
        }

        // Top-Right
        temp_check_row = modFunction( _cell_row - 1 , cell_grid_.GetLength( 0 ) );
        temp_check_column = modFunction( _cell_column + 1 , cell_grid_.GetLength( 1 ) );

        if ( this.cell_grid_[ temp_check_row , temp_check_column ] == CELL_STATE.ALIVE )
        {
            ++local_neighbors_count;
        }

        // Left
        temp_check_row = modFunction( _cell_row , cell_grid_.GetLength( 0 ) );
        temp_check_column = modFunction( _cell_column - 1 , cell_grid_.GetLength( 1 ) );

        if ( this.cell_grid_[ temp_check_row , temp_check_column ] == CELL_STATE.ALIVE )
        {
            ++local_neighbors_count;
        }

        // Right
        temp_check_row = modFunction( _cell_row , cell_grid_.GetLength( 0 ) );
        temp_check_column = modFunction( _cell_column + 1 , cell_grid_.GetLength( 1 ) );

        if ( this.cell_grid_[ temp_check_row , temp_check_column ] == CELL_STATE.ALIVE )
        {
            ++local_neighbors_count;
        }

        // Bottom-Left
        temp_check_row = modFunction( _cell_row + 1 , cell_grid_.GetLength( 0 ) );
        temp_check_column = modFunction( _cell_column - 1 , cell_grid_.GetLength( 1 ) );

        if ( this.cell_grid_[ temp_check_row , temp_check_column ] == CELL_STATE.ALIVE )
        {
            ++local_neighbors_count;
        }

        // Bottom
        temp_check_row = modFunction( _cell_row + 1 , cell_grid_.GetLength( 0 ) );
        temp_check_column = modFunction( _cell_column , cell_grid_.GetLength( 1 ) );

        if ( this.cell_grid_[ temp_check_row , temp_check_column ] == CELL_STATE.ALIVE )
        {
            ++local_neighbors_count;
        }

        // Bottom-Right
        temp_check_row = modFunction( _cell_row + 1 , cell_grid_.GetLength( 0 ) );
        temp_check_column = modFunction( _cell_column + 1 , cell_grid_.GetLength( 1 ) );

        if ( this.cell_grid_[ temp_check_row , temp_check_column ] == CELL_STATE.ALIVE )
        {
            ++local_neighbors_count;
        }

        /* OLD CODE - NO WRAP AROUND
        // Top-Left
        if ( ( _cell_row != 0 ) && ( _cell_column != 0 ) )
        {
            if ( this.cell_grid_[ _cell_row - 1 , _cell_column - 1 ] == CELL_STATE.ALIVE )
            {
                ++local_neighbors_count;
            }
        }
        // Top-Center
        if ( ( _cell_row != 0 ) )
        {
            if ( this.cell_grid_[ _cell_row - 1 , _cell_column ] == CELL_STATE.ALIVE )
            {
                ++local_neighbors_count;
            }
        }
        // Top-Right
        if ( ( _cell_row != 0 ) && ( _cell_column != this.cell_grid_.GetLength( 1 ) - 1 ) )
        {
            if ( this.cell_grid_[ _cell_row - 1 , _cell_column + 1 ] == CELL_STATE.ALIVE )
            {
                ++local_neighbors_count;
            }
        }
        // Left
        if ( ( _cell_column != 0 ) )
        {
            if ( this.cell_grid_[ _cell_row , _cell_column - 1 ] == CELL_STATE.ALIVE )
            {
                ++local_neighbors_count;
            }
        }
        // Right
        if ( ( _cell_column != this.cell_grid_.GetLength( 1 ) - 1 ) )
        {
            if ( this.cell_grid_[ _cell_row , _cell_column + 1 ] == CELL_STATE.ALIVE )
            {
                ++local_neighbors_count;
            }
        }

        // Bottom-Left
        if ( ( _cell_row != this.cell_grid_.GetLength( 0 ) - 1 ) && ( _cell_column != 0 ) )
        {
            if ( this.cell_grid_[ _cell_row + 1 , _cell_column - 1 ] == CELL_STATE.ALIVE )
            {
                ++local_neighbors_count;
            }
        }
        // Bottom-Center
        if ( ( _cell_row != this.cell_grid_.GetLength( 0 ) - 1 ) )
        {
            if ( this.cell_grid_[ _cell_row + 1 , _cell_column ] == CELL_STATE.ALIVE )
            {
                ++local_neighbors_count;
            }
        }
        // Bottom-Right
        if ( ( _cell_row != this.cell_grid_.GetLength( 0 ) - 1 ) && ( _cell_column != this.cell_grid_.GetLength( 1 ) - 1 ) )
        {
            if ( this.cell_grid_[ _cell_row + 1 , _cell_column + 1 ] == CELL_STATE.ALIVE )
            {
                ++local_neighbors_count;
            }
        }
        */
        return local_neighbors_count;

    }


    #endregion
}
