using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBackForth : MonoBehaviour
{
    public enum Direction { Left, Right }
    public Direction direction = Direction.Right;
    public float speed = 3f;
    public float maxDistance = 3.0f;

    private Vector3 originalPos;

    private void Start()
    {
        originalPos = transform.position;
    }

    private void Update()
    {
        float dirValue = (direction == Direction.Right) ? 1f : -1f;
        transform.Translate(dirValue * speed * Time.deltaTime, 0, 0, Space.World);

        float offset = transform.position.x - originalPos.x;
        if (Mathf.Abs(offset) > maxDistance)
        {
            float clampedX = originalPos.x + Mathf.Sign(offset) * maxDistance;
            transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);

            direction = (direction == Direction.Right) ? Direction.Left : Direction.Right;
        }
    }



}
//Problem - Move an object back n forth in any axis
//Solution - move them in a direction detect how much they move from there original pos
//if they are over move them in other direction.
//Pseudo - 1.store the original pos
//2.Move the object in a axis in a certain direction
//3.check the distance from original pos and current position
//4.If the distance is over max change direction 
//Data - original position, max distance, direction, distance from original pos
//Tools - transform.Translate(), Vector3.Distance(), 