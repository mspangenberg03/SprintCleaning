using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TrackGarbageLaneDecider
{
    private int _maxConsecutiveBeatsWithLaneChange;
    private float _oddsForciblyChangeLane;

    private int _priorLane;
    private int _priorBeat = -1;
    private int _consecutiveBeatsWithLaneChange;

    public TrackGarbageLaneDecider(int maxConsecutiveBeatsWithLaneChange, float oddsForciblyChangeLane)
    {
        _maxConsecutiveBeatsWithLaneChange = maxConsecutiveBeatsWithLaneChange;
        _oddsForciblyChangeLane = oddsForciblyChangeLane;
    }

    public int SelectGarbageLane(int beat)
    {
        bool isConsecutiveBeat = (beat == _priorBeat + 1 || (beat == 0 && _priorBeat == 15)) && _priorBeat != -1;

        // Select the lane.
        int lane;
        if (isConsecutiveBeat && _consecutiveBeatsWithLaneChange >= _maxConsecutiveBeatsWithLaneChange)
            lane = _priorLane;
        else
        {
            if (isConsecutiveBeat)
            {
                do
                {
                    lane = Random.Range(-1, 2);
                } while (System.Math.Abs(lane - _priorLane) > 1);
            }
            else
            {
                _consecutiveBeatsWithLaneChange = 0;
                lane = Random.Range(-1, 2);
            }

            bool forciblyChangeLane = lane == _priorLane && Random.value < _oddsForciblyChangeLane;
            if (forciblyChangeLane)
            {
                do
                {
                    lane = Random.Range(-1, 2);
                } while (System.Math.Abs(lane - _priorLane) == _priorLane);

                if (isConsecutiveBeat)
                {
                    while (System.Math.Abs(lane - _priorLane) > 1)
                        lane = Random.Range(-1, 2);
                }

            }
        }

        if (isConsecutiveBeat && lane != _priorLane)
            _consecutiveBeatsWithLaneChange++;
        else
            _consecutiveBeatsWithLaneChange = 0;
        _priorBeat = beat;
        _priorLane = lane;

        return lane;
    }
}
