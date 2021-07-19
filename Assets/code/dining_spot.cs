using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dining_spot : walk_to_settler_interactable, IAddsToInspectionText
{
    public override string task_summary()
    {
        return "Eating";
    }

    List<food_dipsenser> food_dispensers = new List<food_dipsenser>();
    List<item> foods = new List<item>();
    int index = 0;
    town_path_element.path path;
    float eating_timer = 0;
    settler_animations.simple_work work_anim;

    public override string added_inspection_text()
    {
        return base.added_inspection_text() + "\n" +
            "Dining spot (connected dispensers : " + food_dispensers.Count + ")";
    }

    protected override void Start()
    {
        base.Start();

        town_path_element.add_on_rooms_update_listener(() =>
        {
            if (this == null) return;

            // Identify connected food dispensers
            food_dispensers.Clear();
            var spot = path_element();
            spot.iterate_connected((e) =>
            {
                var fd = e.interactable as food_dipsenser;
                if (fd != null) food_dispensers.Add(fd);
                return false;
            }, same_room: true);

            // Record room :-> dining spots map
            room_dining_spots.access_or_set(spot.room, () => new List<dining_spot>()).Add(this);
        });
    }

    void reset()
    {
        // Reset stuff
        index = 0;
        path = null;
        eating_timer = 0;
        work_anim = null;

        foreach (var f in foods)
            if (f != null)
                Destroy(f.gameObject);

        foods.Clear();
    }

    bool food_available()
    {
        foreach (var fd in food_dispensers)
            if (fd.food_available) return true;
        return false;
    }

    protected override bool ready_to_assign(settler s)
    {
        return s.ready_to_eat() && food_available();
    }

    protected override void on_arrive(settler s)
    {
        reset();
    }

    STAGE_RESULT gather_foods(settler s)
    {
        if (path == null)
        {
            if (index < food_dispensers.Count)
            {
                // Path to the next food dispenser
                var goal = food_dispensers[index]?.path_element(s.group);
                path = town_path_element.path.get(s.town_path_element, goal);
                if (path == null) return STAGE_RESULT.TASK_FAILED;
            }
            else
            {
                // Path back to the dining spot
                var goal = this?.path_element(s.group);
                path = town_path_element.path.get(s.town_path_element, goal);
                if (path == null) return STAGE_RESULT.TASK_FAILED;
            }
        }

        switch (path.walk(s, s.walk_speed))
        {
            // Arrived at destination
            case town_path_element.path.WALK_STATE.COMPLETE:
                path = null;

                if (index < food_dispensers.Count)
                {
                    // Pickup some food
                    var food = food_dispensers[index]?.item_dispenser?.dispense_first_item();
                    if (food != null)
                    {
                        foods.Add(food);
                        food.transform.SetParent(index % 2 == 0 ?
                            s.right_hand.transform :
                            s.left_hand.transform);
                        food.transform.localPosition = Vector3.zero;
                    }

                    // Move to next thing
                    ++index;
                    return STAGE_RESULT.STAGE_UNDERWAY;
                }

                // Getting food stage is complete and we're back at the spot
                else return STAGE_RESULT.STAGE_COMPLETE;

            // Continue pathing
            case town_path_element.path.WALK_STATE.UNDERWAY:
                return STAGE_RESULT.STAGE_UNDERWAY;

            // Pathing failed => task failed
            default:
                return STAGE_RESULT.TASK_FAILED;
        }
    }

    STAGE_RESULT arrive_at_spot(settler s)
    {
        s.transform.position = transform.position;
        s.transform.forward = transform.forward;
        work_anim = new settler_animations.simple_work(s);

        var c = GetComponentInChildren<chair>();
        if (c != null)
        {

        }

        return STAGE_RESULT.STAGE_COMPLETE;
    }

    STAGE_RESULT eat_foods(settler s)
    {
        if (work_anim == null) return STAGE_RESULT.TASK_FAILED;
        work_anim.play();

        eating_timer += Time.deltaTime;
        if (eating_timer < foods.Count * 2)
            return STAGE_RESULT.STAGE_UNDERWAY;

        foreach (var f in foods)
        {
            s.consume_food(f.food_values);
            Destroy(f.gameObject);
        }
        foods.Clear();

        return STAGE_RESULT.TASK_COMPLETE;
    }

    protected override STAGE_RESULT on_interact_arrived(settler s, int stage)
    {
        switch (stage)
        {
            case 0: return gather_foods(s);
            case 1: return arrive_at_spot(s);
            default: return eat_foods(s);
        }
    }

    protected override void on_unassign(settler s)
    {
        reset();
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        path?.draw_gizmos(color: Color.cyan);
    }

    //##############//
    // STATIC STUFF //
    //##############//

    static Dictionary<int, List<dining_spot>> room_dining_spots = new Dictionary<int, List<dining_spot>>();

    static dining_spot()
    {
        town_path_element.add_on_rooms_update_listener(() =>
        {
            room_dining_spots.Clear();
        });
    }

    public static bool is_dining_room(int room)
    {
        if (room_dining_spots.TryGetValue(room, out List<dining_spot> spots))
            foreach (var s in spots)
                if (s != null)
                    return true;
        return false;
    }
}
