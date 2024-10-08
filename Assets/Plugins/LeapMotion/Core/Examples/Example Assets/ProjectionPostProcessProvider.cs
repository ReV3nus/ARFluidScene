/******************************************************************************
 * Copyright (C) Ultraleap, Inc. 2011-2020.                                   *
 * Ultraleap proprietary and confidential.                                    *
 *                                                                            *
 * Use subject to the terms of the Leap Motion SDK Agreement available at     *
 * https://developer.leapmotion.com/sdk_agreement, or another agreement       *
 * between Ultraleap and you, your company or other organization.             *
 ******************************************************************************/

using System;
using UnityEngine;

namespace Leap.Unity.Examples {

  public class ProjectionPostProcessProvider : PostProcessProvider
  {



    [Header("Projection")]
    // public HandModelManager handModelManager;

    public Transform headTransform;

    [Tooltip("The exponent of the projection of any hand distance from the approximated "
           + "shoulder beyond the handMergeDistance.")]
    [Range(0f, 100f)]
    public float projectionExponent = 50f;

    [Tooltip("The distance from the approximated shoulder beyond which any additional "
           + "distance is exponentiated by the projectionExponent.")]
    [Range(0f, 1f)]
    public float handMergeDistance = 0.35f;
    public CapsuleHand LeftModel,RightModel;
    public Vector3 offset;
    
    [Range(0f, 1f)]
    public float scaleFactor = 0.85f;


    public override void ProcessFrame(ref Frame inputFrame) {
      
      Vector3 headPos = headTransform.position;
      var shoulderBasis = Quaternion.LookRotation(
        Vector3.ProjectOnPlane(headTransform.forward, Vector3.up),
        Vector3.up);

      foreach (var hand in inputFrame.Hands) {
        // Approximate shoulder position with magic values.
        var shoulderPos = headPos
                          + (shoulderBasis * (new Vector3(0f, -0.2f, -0.1f)
                          + Vector3.left * 0.1f * (hand.IsLeft ? 1f : -1f)));

        // Calculate the projection of the hand if it extends beyond the
        // handMergeDistance.
        var shoulderToHand = hand.PalmPosition.ToVector3() - shoulderPos;
        var handShoulderDist = shoulderToHand.magnitude;
        var projectionDistance = Mathf.Max(0f, handShoulderDist - handMergeDistance);
        var projectionAmount = Mathf.Pow(1 + projectionDistance, projectionExponent);
        projectionAmount = Mathf.Min(54.0f, projectionAmount);
        LeftModel.stretchFactor = projectionAmount * scaleFactor;
        RightModel.stretchFactor = projectionAmount * scaleFactor;

        hand.SetTransform(shoulderPos + shoulderToHand * projectionAmount + offset,
                          hand.Rotation.ToQuaternion());
        
        // Debug.Log(hand.PalmPosition);
      }
    }

  }
}
