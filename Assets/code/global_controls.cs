﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class global_controls : MonoBehaviour
{
    void Start()
    {
        version_control.on_startup();

        // Persistant through scene loads
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // Cycle fullscreen modes
        if (controls.key_press(controls.BIND.CYCLE_FULLSCREEN_MODES))
        {
            switch (Screen.fullScreenMode)
            {
                case FullScreenMode.Windowed:
                    Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                    break;
                case FullScreenMode.FullScreenWindow:
                    Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                    break;
                case FullScreenMode.ExclusiveFullScreen:
                    Screen.fullScreenMode = FullScreenMode.Windowed;
                    break;
                default:
                    throw new System.Exception("Unkown fullscreen mode!");
            }
        }
    }
}