using UnityEngine;

public class OpenAndClose : MonoBehaviour
{
    public float swingAngle = 90f; // How many degrees to open
    public float loopSpeed = 1f;

    private float startAngle;

    void Start()
    {
        // Capture the -90 (or whatever it is) when the game starts
        startAngle = transform.localEulerAngles.y;
    }

    void Update()
    {
        float lerpTime = Mathf.PingPong(Time.time * loopSpeed, 1.0f);

        // This calculates the movement ADDED to the start angle
        float currentAngle = startAngle + (lerpTime * swingAngle);

        transform.localRotation = Quaternion.Euler(0, currentAngle, 0);
    }
}