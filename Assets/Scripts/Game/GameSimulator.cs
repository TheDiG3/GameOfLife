using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

/**
 * <summary> 
 * This class stores the game of life data and handles the simulation process. 
 * It can be set to use multithreading by changing <see cref=""/>
 * </summary>
 */
public class GameSimulator : System.IDisposable
{

    #region Public Members

    public enum ECellState
    {
        DEAD,
        ALIVE
    }

    public class SimulationStep
    {
        public ECellState[, ] cell_grid;
        public float time_to_simulate;

        public SimulationStep( ECellState[, ] _state , float _time )
        {
            cell_grid = _state;
            time_to_simulate = _time;
        }
    }

    /// <summary>
    /// Queue used to push out simulation states to the main thread (the one doing the rendering).
    /// As per C# specification, having it public static "should" ensure thread safety.
    /// </summary>
    public static Queue<SimulationStep> simulation_queue_;

    /** <summary> Maximum size the queue can grow to. </summary> */
    public static uint kMaxQueueSize = 10000;

    #endregion

    #region Private Members

    /** <summary> The grid containing the states of all the cells. </summary> */
    private static ECellState[, ] cell_grid_;

    /** 
     * <summary>
     * The thread on which the threaded simulation is running. This will run at full speed with no time steps in between each simulation.
     * Only one thread supported per application process.
     * </summary> 
     */
    private static Thread simulation_thread_;

    /** <summary> Should the simulation use multithreading? </summary> */
    public static bool b_use_threading_;

    private static bool b_is_simulation_running_ = false;

    /** Used to calculate time intervals of simulations. */
    private static Stopwatch performance_stopwatch_ = new Stopwatch();

    #endregion

    #region Public Methods

    /** 
     * <summary>
     * Property accessor. This also handles automatic switching from multithread to singlethread processing of simulation. 
     * If set to true it will create/resume a simulation thread which will keep on simulating until stopped. 
     * No time-step simulation is supported for multithreading.
     * If you want to simulate in fixed time steps you should do so by calling <see cref="Simulate"/> externally on an instance
     * of this class with <see cref="UseThreading"/> set to <value>false</value>
     * </summary>
     */
    public bool UseThreading
    {
        get
        {
            return b_use_threading_;
        }

        set
        {
            b_use_threading_ = value;

            // Should check if we already have a thread running
            // If not spawn a new one and start the simulation
            // Remember that whether the thread actually simulates or not depends on the value of b_is_simulation_running_
            // Need to clear the queue in order to start "clean"
            // This just controls whether the thread is running or not (basically just allocates the resources for the thread and keeps it in "idle" state)
            if ( b_use_threading_ )
            {
                if ( simulation_thread_ == null )
                {
                    simulation_queue_.Clear();
                    simulation_thread_ = new Thread( new ThreadStart( ThreadedSimulate ) );
                    simulation_thread_.Start();
                }
            }
            // We are requesting a change of environment back to singlethread
            // Stop and join any simulating thread and clear up the resources used
            // Reset the queue and updates the current cell_grid to be the value that was going to be dequed next in order to avoid visual jumps forward in time
            // Remember to set the simulation_thread to null in order to be able to start a new thread later on if a new environment change is requested
            else
            {
                if ( simulation_thread_ != null ) // Sanity check if the thread is actually running
                {
                    simulation_thread_.Join();
                    cell_grid_ = simulation_queue_.Dequeue().cell_grid;
                    simulation_queue_.Clear();
                    simulation_thread_ = null;
                }
            }
        }
    }

    public bool IsSimulationRunning
    {
        get
        {
            return b_is_simulation_running_;
        }

        set
        {
            b_is_simulation_running_ = value;

            // Special conditions need to be handled when we stop the simulation while in a threaded environment
            // If the simulation is stopped we should join the thread
            // If the simulation is started we should create a new thread if one does not exist already
            if ( b_use_threading_ )
            {
                // Simulation has been turned on
                // Since the previous running thread might have terminated we should try and start a new one
                if ( b_is_simulation_running_ )
                {
                    if ( simulation_thread_ == null )
                    {
                        simulation_thread_ = new Thread( new ThreadStart( ThreadedSimulate ) );
                        simulation_thread_.Start();
                    }
                }
                // Simulation has been turned off
                // Join the thread that should be terminating shortly
                else
                {
                    if ( simulation_thread_ != null ) // Sanity check if the thread is actually running
                    {
                        simulation_thread_.Join();
                        simulation_thread_ = null;
                    }
                }
            }
        }
    }

    /**
     * <summary> Constructs the game simulator object with the given cell grid. </summary>
     * <param name="_cell_grid"> The cell grid to use. </param>
     * <param name="_b_use_multithreading"> Should the simulation be run on a separate thread? </param>
     * <param name="_start_enabled"> Should the simulation start immediately? </param>
     */
    public GameSimulator( ECellState[, ] _cell_grid , bool _b_use_multithreading , bool _start_enabled )
    {
        cell_grid_ = _cell_grid;
        simulation_queue_ = new Queue<SimulationStep>();
        IsSimulationRunning = _start_enabled;
        UseThreading = _b_use_multithreading;
    }

    /**
     * <summary> 
     * Simulates one cycle of the game of life. 
     * </summary>
     * <returns>
     * When called on a singlethreaded instance it just simulates one step and returns the next state of the simulation. This is a locking operation.
     * When called on a multithreaded instance it dequeues a single state and returns it, if available. If no new simulations steps are available yet, returns null.
     * </returns>
     */
    public SimulationStep Simulate()
    {
        // If we are not using multithreading simulate a single step and return the result
        if ( !b_use_threading_ )
        {
            return SimulateNextStep();
        }
        // With multithreading enabled dequeues a single step from the simulation queue, if any is available
        else
        {
            if ( simulation_queue_.Count != 0 )
            {
                return simulation_queue_.Dequeue();
            }
            else
            {
                return null;
            }
        }
    }

    /** <summary> Simulates the next step. </summary> */
    private SimulationStep SimulateNextStep()
    {
        performance_stopwatch_.Start();

        // Since the simulation is basically a function that outputs a new grid I need a new array to store the new states of the cell
        // in order to avoid interferences on the current state of the game grid if new cells spawn or some die.
        // This array will then become the new game grid.
        ECellState[, ] ret_next_step_game_grid = new ECellState[ cell_grid_.GetLength( 0 ), cell_grid_.GetLength( 1 ) ];

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
                    ret_next_step_game_grid[ cycle_row_index , cycle_column_index ] = ECellState.ALIVE;
                }
                else if ( local_neighbors_count < 2 )
                {
                    ret_next_step_game_grid[ cycle_row_index , cycle_column_index ] = ECellState.DEAD;
                }
                else if ( local_neighbors_count > 3 )
                {
                    ret_next_step_game_grid[ cycle_row_index , cycle_column_index ] = ECellState.DEAD;
                }
                else
                {
                    ret_next_step_game_grid[ cycle_row_index , cycle_column_index ] = cell_grid_[ cycle_row_index , cycle_column_index ];
                }
            }
        }

        cell_grid_ = ret_next_step_game_grid;

        performance_stopwatch_.Stop();
        float elapsed_time = performance_stopwatch_.ElapsedMilliseconds;
        performance_stopwatch_.Reset();

        return new SimulationStep( ret_next_step_game_grid , elapsed_time );
    }

    /**
     * <summary>
     * Method continuously executed by the spawned thread.
     * Internally it simulates one step and adds the resulting state to the simulating queue.
     * </summary>
     */
    private void ThreadedSimulate()
    {
        while ( b_is_simulation_running_ && b_use_threading_ )
        {
            if ( simulation_queue_.Count < kMaxQueueSize )
            {
                simulation_queue_.Enqueue( SimulateNextStep() );
            }
        }
    }

    /**
     * <summary> Returns the bidimensional array containing the states of the cells. </summary>
     */
    [System.Obsolete( "Simulate now directly returns the newly simulated cell_grid. It is advised not to try and access cell_grid anymore since in a multithreaded environment it could lead to problems.\n Use the return value from Simulate() for single threaded simulation or dequeue states from the simulation queue for multithreaded simulation." )]
    public ECellState[, ] getCellGridState()
    {
        return cell_grid_;
    }

    /**
     * <summary> Sets the bidimensional array containing the states of the cells. </summary>
     */
    public void setCellGridState( ECellState[, ] _cell_grid )
    {
        cell_grid_ = _cell_grid;
        // If we are running a simulation thread the queue becomes invalid since we most likely changed a previous state in the simulation
        if ( simulation_thread_ != null )
        {
            simulation_queue_.Clear(); // I think this "should" be thread safe. Not too sure about it!
        }
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

        if ( cell_grid_[ temp_check_row , temp_check_column ] == ECellState.ALIVE )
        {
            ++local_neighbors_count;
        }

        // Top-Center
        temp_check_row = modFunction( _cell_row - 1 , cell_grid_.GetLength( 0 ) );
        temp_check_column = modFunction( _cell_column , cell_grid_.GetLength( 1 ) );

        if ( cell_grid_[ temp_check_row , temp_check_column ] == ECellState.ALIVE )
        {
            ++local_neighbors_count;
        }

        // Top-Right
        temp_check_row = modFunction( _cell_row - 1 , cell_grid_.GetLength( 0 ) );
        temp_check_column = modFunction( _cell_column + 1 , cell_grid_.GetLength( 1 ) );

        if ( cell_grid_[ temp_check_row , temp_check_column ] == ECellState.ALIVE )
        {
            ++local_neighbors_count;
        }

        // Left
        temp_check_row = modFunction( _cell_row , cell_grid_.GetLength( 0 ) );
        temp_check_column = modFunction( _cell_column - 1 , cell_grid_.GetLength( 1 ) );

        if ( cell_grid_[ temp_check_row , temp_check_column ] == ECellState.ALIVE )
        {
            ++local_neighbors_count;
        }

        // Right
        temp_check_row = modFunction( _cell_row , cell_grid_.GetLength( 0 ) );
        temp_check_column = modFunction( _cell_column + 1 , cell_grid_.GetLength( 1 ) );

        if ( cell_grid_[ temp_check_row , temp_check_column ] == ECellState.ALIVE )
        {
            ++local_neighbors_count;
        }

        // Bottom-Left
        temp_check_row = modFunction( _cell_row + 1 , cell_grid_.GetLength( 0 ) );
        temp_check_column = modFunction( _cell_column - 1 , cell_grid_.GetLength( 1 ) );

        if ( cell_grid_[ temp_check_row , temp_check_column ] == ECellState.ALIVE )
        {
            ++local_neighbors_count;
        }

        // Bottom
        temp_check_row = modFunction( _cell_row + 1 , cell_grid_.GetLength( 0 ) );
        temp_check_column = modFunction( _cell_column , cell_grid_.GetLength( 1 ) );

        if ( cell_grid_[ temp_check_row , temp_check_column ] == ECellState.ALIVE )
        {
            ++local_neighbors_count;
        }

        // Bottom-Right
        temp_check_row = modFunction( _cell_row + 1 , cell_grid_.GetLength( 0 ) );
        temp_check_column = modFunction( _cell_column + 1 , cell_grid_.GetLength( 1 ) );

        if ( cell_grid_[ temp_check_row , temp_check_column ] == ECellState.ALIVE )
        {
            ++local_neighbors_count;
        }

        return local_neighbors_count;
    }

    public void Dispose()
    {
        simulation_queue_.Clear();
        IsSimulationRunning = false;
    }

    #endregion
}
