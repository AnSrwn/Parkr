using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPosition : MonoBehaviour
{
    public GameObject objectToFollow;
    Transform followTransform;
    Vector3 relationToFollowObject;

    void Start()
    {
        followTransform = objectToFollow.GetComponent<Transform>();
        relationToFollowObject = followTransform.position - transform.position;
    }

    void Update()
    {
        transform.position = followTransform.position - relationToFollowObject;
    }
}
