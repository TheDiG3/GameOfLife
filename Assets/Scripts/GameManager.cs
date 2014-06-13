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

    #endregion

    #region Private Members

    /** The simulator used to run the game of life. */
    private GameSimulator game_simulator_;

    #endregion


    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Code for the user interface.
    void OnGUI()
    {

    }
}
