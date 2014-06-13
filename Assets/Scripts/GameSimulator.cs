/**
 * This class stores the game of life data and handles the simulation process. 
 */
public class GameSimulator
{

    #region Public Members

    public enum CELL_STATE
    {
        DEAD,
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
    public GameSimulator(CELL_STATE[,] _cell_grid)
    {
        cell_grid_ = _cell_grid;
    }

    /**
     * Simulates one cycle of the game of life.
     */
    public void simulate()
    {

    }
    
    #endregion

    #region Private Methods



    #endregion
}
