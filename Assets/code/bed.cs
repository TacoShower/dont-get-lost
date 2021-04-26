﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bed : settler_interactable
{
    public const float TIREDNESS_RECOVERY_RATE = 100f / 60f;
    public Transform sleep_orientation;

    float delta_tired;
    float time_slept;
    int start_tiredness;
    int last_tiredness;

    int less_tired_amount => Mathf.Max(0, start_tiredness - last_tiredness);

    protected override bool ready_to_assign(settler s)
    {
        // You have to be tired enough to sleep
        return s.tiredness.value > 80;
    }

    protected override void on_assign(settler s)
    {
        // Reset stuff
        time_slept = 0f;
        delta_tired = 0f;
        start_tiredness = s.tiredness.value;
        last_tiredness = start_tiredness;
    }

    protected override RESULT on_interact(settler s)
    {
        // Lie down
        s.transform.position = sleep_orientation.position;
        s.transform.rotation = sleep_orientation.rotation;

        time_slept += Time.deltaTime;
        last_tiredness = s.tiredness.value;

        // Only modify tiredness on authority client
        if (!s.has_authority) return RESULT.UNDERWAY;

        // Beomce un-tired
        delta_tired -= TIREDNESS_RECOVERY_RATE * Time.deltaTime;
        if (delta_tired < -1f)
        {
            delta_tired = 0f;
            s.tiredness.value -= 1;
        }

        if (s.tiredness.value < 20 && time_slept > 5f)
            return RESULT.COMPLETE;
        return RESULT.UNDERWAY;
    }

    protected override void on_unassign(settler s)
    {
        // Un-lie down
        s.transform.rotation = Quaternion.identity;
        s.add_mood_effect("just_got_up");
    }

    public override string task_info()
    {
        return "Sleeping\n" +
               "    Slept for " + Mathf.Round(time_slept) + "s\n" +
               "    " + less_tired_amount + "% less tired";
    }
}
