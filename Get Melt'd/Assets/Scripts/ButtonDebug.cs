using UnityEngine;

public class ButtonDebug : MonoBehaviour
{
    void Update()
    {
        // Checks all 20 possible joystick buttons
        for (int i = 0; i < 20; i++)
        {
            if (Input.GetKeyDown((KeyCode)(350 + i)))
            {
                Debug.Log("Button pressed: joystick button " + i);
            }
        }
    }
}