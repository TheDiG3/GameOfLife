using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour
{
    public Camera camera_;
    public float camera_panning_speed_;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        // Camera Panning
        if ( Input.GetKey( KeyCode.W ) )
        {
            camera_.transform.Translate( Vector3.up * camera_panning_speed_ * Time.deltaTime );
        }
        if ( Input.GetKey( KeyCode.S ) )
        {
            camera_.transform.Translate( Vector3.down * camera_panning_speed_ * Time.deltaTime );
        }
        if ( Input.GetKey( KeyCode.A ) )
        {
            camera_.transform.Translate( Vector3.left * camera_panning_speed_ * Time.deltaTime );
        }
        if ( Input.GetKey( KeyCode.D ) )
        {
            camera_.transform.Translate( Vector3.right * camera_panning_speed_ * Time.deltaTime );
        }

        // Panning Speed 
        if ( Input.GetKey( KeyCode.KeypadPlus ) )
        {
            camera_panning_speed_++;
        }
        else if ( Input.GetKey( KeyCode.KeypadMinus ) )
        {
            // To avoid inverting the camera movement ( camera panning speed < 0 ) and a still camera ( camera panning speed = 0 ).
            if ( camera_panning_speed_ >= 2 )
            {
                camera_panning_speed_--;
            }
        }

        // Camera Zoom
        if ( Input.GetAxis("Mouse ScrollWheel") >= 0.1)
        {
            // To avoid inverting the camera frustum.
            if ( camera_.orthographicSize >= 2 )
            {
                camera_.orthographicSize--;
            }
        }
        else if(Input.GetAxis("Mouse ScrollWheel") <= -0.1)
        {
            camera_.orthographicSize++;
        }

    }

}
