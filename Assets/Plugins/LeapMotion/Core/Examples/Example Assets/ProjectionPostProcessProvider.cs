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


    public override void ProcessFrame(ref Frame inputFrame) {
      
      // if (_inputLeapProvider == null)
      // {
      //   _inputLeapProvider = Hands.Provider;
      // }

      // Calculate the position of the head and the basis to calculate shoulder position.
      
      // if (headTransform == null)
      // {
      //   GameObject LeapProviderEsky =  GameObject.Find("LeapMotion");
      //   if(LeapProviderEsky != null){
      //     _inputLeapProvider = LeapProviderEsky.GetComponent<LeapXRServiceProvider>();
      //     Debug.Log("Setting the hand models");
      //     headTransform = LeapProviderEsky.transform;
      //
      //   }else{
      //     Debug.LogError("Couldn't find a 'LeapMotion' game object in scene, the Esky Leapmotion provider needs this, did you modify the transform structure???");
      //   }
      //   
      // }
      
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
        LeftModel.stretchFactor = projectionAmount;
        RightModel.stretchFactor = projectionAmount;

        hand.SetTransform(shoulderPos + shoulderToHand * projectionAmount,
                          hand.Rotation.ToQuaternion());
        // Debug.Log(hand.PalmPosition);
      }
    }

  }
}
