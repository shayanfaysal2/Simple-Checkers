// Script from Chapter 4 of "Unity 2020 Mobile Game Development", Second Edition, by John P. Doran
// Attach this to any child object of a Canvas object that you'd like to scale and adapt around notches on various mobile devices

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISafeAreaHandler : MonoBehaviour
{
    RectTransform panel;

    void Start()
    {
        panel = GetComponent<RectTransform>();
    }

    void Update()
    {
        // The Screen.safeArea property returns a variable of the Rect type, which contains an X and Y position and a width and height,
        // just like the Rect Transform component. This Rect Transform gives a box containing the safe area that doesn't have notches inside it.
        // Screen.safeArea will change depending on the orientation that the device is currently in. Since we want to support all orientations
        // (landscape and portrait mode), we'll have to check for the safe area changing at runtime, which is why we use the Update function
        // to do modifications.
        Rect area = Screen.safeArea;

        // Pixel size in screen space of the whole screen
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        // For testing purposes
        // We are adding the ability, if we are inside the Unity Editor, to press the spacebar to change the value of the area and set the panel's
        // anchor values to something different. In this case, we are using the values given by the iPhone XS Max if calling Screen.safeArea.
        // Since the editor doesn't set orientation properties in Unity, we check the screen size to determine whether we are in landscope or portrait mode.
        // NOTE: The below if statement does not work in the Device Simulator
        if (Application.isEditor && Input.GetButton("Jump"))
        {
            // Use the notch properties of the iPhone XS Max
            if (Screen.height > Screen.width)
            {
                // Portrait
                // In portrait mode, the top portion of the screen is cut off for the notch and the bottom is cut off for the home button
                area = new Rect(0f, 0.038f, 1f, 0.913f);
            }
            else
            {
                // Landscape
                // Switching to landscape, we lose the left/right side for the notch and on iOS, it cuts off the other side as well.
                // Just as in portrait, the top is cut off for the home key.
                area = new Rect(0.049f, 0.051f, 0.902f, 0.949f);
            }

            panel.anchorMin = area.position;
            panel.anchorMax = (area.position + area.size);

            return;
        }

        // As we know, anchors can be used to specify the size of a panel. Anchors work in viewport space, which is to say that the values go
        // from (0, 0) to (1, 1) Since the Rect given by Screen.safeArea is in screen (pixel) space, we divide by the screen size in pixels to convert to the points
        // to viewport space.
        // Set anchors to percentages of the screen used
        panel.anchorMin = area.position / screenSize;
        panel.anchorMax = (area.position + area.size) / screenSize;
    }
}
