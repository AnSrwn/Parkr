using Google.Protobuf.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResetCars : MonoBehaviour
{
    public List<GameObject> cars;
    private MapField<int, Transform> carTransforms = new MapField<int, Transform>();

    public void ResetCarPositions()
    {
        if (carTransforms.Count() > 0)
        {
            for (int i = 0; i < cars.Count(); i++)
            {
                cars[i].transform.localPosition = carTransforms[i].localPosition;
                cars[i].transform.localRotation = carTransforms[i].localRotation;
            }
        }
    }
    void Start()
    {
        //get transforms of cars
        for (int i = 0; i < cars.Count(); i++)
        {
            var newTrans = new GameObject().transform;
            newTrans.localPosition = cars[i].transform.localPosition;
            newTrans.localRotation = cars[i].transform.localRotation;

            carTransforms.Add(i, newTrans);
        }
    }
}
