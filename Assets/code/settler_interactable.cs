﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class settler_interactable : MonoBehaviour, INonBlueprintable, INonEquipable
{
    /// <summary> The path element that a settler 
    /// goes to in order to use this object. </summary>
    public settler_path_element path_element;

    /// <summary> The type of interaction this is. This will determine when 
    /// a settler decides to use this interactable object. </summary>
    public TYPE type;

    public enum TYPE
    {
        WORK,
        EAT
    }

    bool registered = false;
    private void Start()
    {
        registered = true;
        register_interactable(this);
    }

    private void OnDestroy()
    {
        if (registered)
            forget_interactable(this);
    }

    public virtual bool interact()
    {
        return true;
    }

    //##############//
    // STATIC STUFF //
    //##############//

    static Dictionary<TYPE, List<settler_interactable>> interactables;

    public static settler_interactable nearest(TYPE t, Vector3 v)
    {
        return utils.find_to_min(interactables[t], 
            (i) => (i.transform.position - v).sqrMagnitude);
    }

    public delegate bool accept_func(settler_interactable i);
    public static settler_interactable nearest_acceptable(TYPE t, Vector3 v, accept_func f)
    {
        var ret = utils.find_to_min(interactables[t], (i) =>
        {
            if (!f(i)) return Mathf.Infinity; // i not acceptable => infinite score
            return (i.transform.position - v).magnitude; // distance = score
        });

        // Only return if acceptable
        if (f(ret)) return ret; 
        return null;
    }

    public static settler_interactable random(TYPE t)
    {
        var l = interactables[t];
        if (l.Count == 0) return null;
        return l[Random.Range(0, l.Count)];
    }

    public static void initialize()
    {
        // Initialize the dictionary of interactables
        interactables = new Dictionary<TYPE, List<settler_interactable>>();
        foreach (TYPE e in System.Enum.GetValues(typeof(TYPE)))
            interactables[e] = new List<settler_interactable>();
    }

    static void register_interactable(settler_interactable i)
    {
        if (interactables[i.type].Contains(i))
            throw new System.Exception("Tried to multiply-register interactable!");
        interactables[i.type].Add(i);
    }

    static void forget_interactable(settler_interactable i)
    {
        if (!interactables[i.type].Remove(i))
            throw new System.Exception("Tried to remove unregistered interactable!");
    }
}