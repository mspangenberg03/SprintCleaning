using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Split stuff out from PlayerMovement

public static class PlayerMovementProcessor
{
    public static void SetFixedDeltaTime()
    {
        // Set the fixed delta time such that, if performance isn't an issue, the number of fixed updates is constant every frame.
        // Otherwise there's a little jitter.
        double frameRate = Screen.currentResolution.refreshRateRatio.value;
        double fixedUpdatesPerFrame = frameRate < 120 ? 2 : 1;
        Time.fixedDeltaTime = (float)(1f / (frameRate * fixedUpdatesPerFrame));
    }

    public static Quaternion NextRotation(float degreesPerSecond, Vector3 toTarget, Quaternion currentRotation)
    {
        Vector3 toTargetOnPlane = new Vector3(toTarget.x, 0, toTarget.z);
        float movementAngle = Quaternion.FromToRotation(Vector3.forward, toTargetOnPlane).eulerAngles.y;
        Vector3 currentAngles = currentRotation.eulerAngles;
        float angleChange = movementAngle - currentAngles.y;
        angleChange = ((angleChange + 540) % 360) - 180; // make change be from -180 to 180
        float maxChange = degreesPerSecond * Time.deltaTime;
        if (Mathf.Abs(angleChange) > maxChange)
        {
            angleChange = Mathf.Sign(angleChange) * maxChange;
        }

        float nextAngle = currentAngles.y + angleChange;

        return Quaternion.Euler(currentAngles.x, nextAngle, currentAngles.z);
    }
}
