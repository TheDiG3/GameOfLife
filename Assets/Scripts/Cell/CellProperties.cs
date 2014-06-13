using UnityEngine;
using System.Collections;

public class CellProperties : MonoBehaviour
{
    public int cell_row_;
    public int cell_column_;
    public GameSimulator.CELL_STATE cell_state_;

    public Material dead_cell_material_;
    public Material alive_cell_material_;

    // When clicked on the cell toggles its state.
    void OnMouseDown()
    {
        if ( cell_state_ == GameSimulator.CELL_STATE.DEAD )
        {
            cell_state_ = GameSimulator.CELL_STATE.ALIVE;
            this.renderer.material = alive_cell_material_;
        }
        else
        {
            cell_state_ = GameSimulator.CELL_STATE.DEAD;
            this.renderer.material = dead_cell_material_;
        }

        GameObject.Find( "Game Manager" ).GetComponent<GameManager>().changeCellState( this.cell_row_ , this.cell_column_ , cell_state_ );
    }
}
