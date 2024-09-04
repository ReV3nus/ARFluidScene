/******************************************************************************
 * Copyright (C) Ultraleap, Inc. 2011-2020.                                   *
 * Ultraleap proprietary and confidential.                                    *
 *                                                                            *
 * Use subject to the terms of the Leap Motion SDK Agreement available at     *
 * https://developer.leapmotion.com/sdk_agreement, or another agreement       *
 * between Ultraleap and you, your company or other organization.             *
 ******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap.Unity.Attributes;

namespace Leap.Unity {
  /** A basic Leap hand model constructed dynamically vs. using pre-existing geometry*/
  public class CapsuleHand : HandModelBase {
    public const int TOTAL_JOINT_COUNT = 4 * 5;
    private const float CYLINDER_MESH_RESOLUTION = 0.1f; //in centimeters, meshes within this resolution will be re-used
    private const int THUMB_BASE_INDEX = (int)Finger.FingerType.TYPE_THUMB * 4;
    private const int PINKY_BASE_INDEX = (int)Finger.FingerType.TYPE_PINKY * 4;

    private static int _leftColorIndex = 0;
    private static int _rightColorIndex = 0;
    private static Color[] _leftColorList = { new Color(0.0f, 0.0f, 1.0f), new Color(0.2f, 0.0f, 0.4f), new Color(0.0f, 0.2f, 0.2f) };
    private static Color[] _rightColorList = { new Color(1.0f, 0.0f, 0.0f), new Color(1.0f, 1.0f, 0.0f), new Color(1.0f, 0.5f, 0.0f) };
    Finger.FingerType[] arr = { Finger.FingerType.TYPE_INDEX, Finger.FingerType.TYPE_MIDDLE };
    #pragma warning disable 0649
    [SerializeField]
    private Chirality handedness;

    [SerializeField]
    private bool _castShadows = true;

    [SerializeField]
    private Material _material;

    [SerializeField]
    private Mesh _sphereMesh;

    [MinValue(3)]
    [SerializeField]
    private int _cylinderResolution = 12;

    [MinValue(0)]
    [SerializeField]
    public float _jointRadius = 0.008f;

    [MinValue(0)]
    [SerializeField]
    private float _cylinderRadius = 0.006f;

    [MinValue(0)]
    [SerializeField]
    private float _palmRadius = 0.015f;
    #pragma warning restore 0649

    public Material _sphereMat;
    private Hand _hand;
    public Vector3[] _spherePositions;
    public float stretchFactor = 1.0f;

    public int previousHandType,handType;
    public float previousTime = 0f;

    // public ExampleFireBallController ExampleFireBallController;
    
    public override ModelType HandModelType {
      get {
        return ModelType.Graphics;
      }
    }

    public override Chirality Handedness {
      get {
        return handedness;
      }
      set { }
    }

    public override bool SupportsEditorPersistence() {
      return true;
    }

    public override Hand GetLeapHand() {
      return _hand;
    }

    public override void SetLeapHand(Hand hand) {
      _hand = hand;
    }

    public override void InitHand() {
      if (_material != null) {
        _sphereMat = new Material(_material);
        _sphereMat.hideFlags = HideFlags.DontSaveInEditor;
      }
    }

    private void OnValidate() {
      _meshMap.Clear();
    }

    public override void BeginHand() {
      base.BeginHand();
      previousHandType = 0;

      if (_hand.IsLeft) {
        // _sphereMat.color = _leftColorList[_leftColorIndex];
        // _leftColorIndex = (_leftColorIndex + 1) % _leftColorList.Length;
      } else {
        // _sphereMat.color = _rightColorList[_rightColorIndex];
        // _rightColorIndex = (_rightColorIndex + 1) % _rightColorList.Length;
      }
    }

    public bool holdingHandType(int type,float time)
    {
      return (type == previousHandType && Time.time - previousTime >= time);
    }

    void Update()
    {
      if (_hand == null || _sphereMat == null) return;
      if (isGrabHand(_hand))
      {
        handType = 1;
      }
      else if (IsOpenFullHand(_hand))
      {
        handType = 2;
      }
      else
      {
        handType = 0;
      }
      
      // if (_hand.IsLeft) {
      //   // _sphereMat.color = _leftColorList[handType];
      //   _sphereMat.color =  new Color(0.0f, 0.0f, 0.0f);
      //
      // } else {
      //   // _sphereMat.color = _rightColorList[handType];
      //   _sphereMat.color = _rightColorList[handType];
      //
      // }
      // _sphereMat.color =  new Color(0.0f, 0.0f, 0.0f);

      
      if (handType != previousHandType)
      {
        // float timeDifference = Time.time - previousTime;

        previousHandType = handType;
        previousTime = Time.time;
      }
 
      
      // if (CheckFingerOpenToHand(hand,arr))
      // {
      //   print("CheckFingerOpenToHand");
      // }
    }
    
    bool CheckFingerOpenToHand(Hand hand, Finger.FingerType[] fingerTypesArr,float deltaCloseFinger = 0.05f)
    {
      List<Finger> listOfFingers = hand.Fingers;
      float count = 0;
      for (int f = 0; f < listOfFingers.Count; f++)
      {
        Finger finger = listOfFingers[f];
        // 判读每个手指的指尖位置和掌心位置的长度是不是小于某个值，以判断手指是否贴着掌心
        if ((finger.TipPosition - hand.PalmPosition).Magnitude < deltaCloseFinger)
        {
          // 如果传进来的数组长度是0，有一个手指那么 count + 1，continue 跳出，不执行下面数组长度不是0 的逻辑
          if (fingerTypesArr.Length == 0)
          {
            count++;
            continue;
          }
          // 传进来的数组长度不是 0，
          for (int i = 0; i < fingerTypesArr.Length; i++)
          {
            // 假如本例子传进来的是食指和中指，逻辑走到这里，如果你的食指是紧握的，下面会判断这个手指是不是食指，返回 false
            if (finger.Type == fingerTypesArr[i])
            {
              return false;
            }
            else
            {
              count++;
            }
          }
 
        }
      }
      if (fingerTypesArr.Length == 0)
      {
        return count == 5;
      }
      // 这里除以length 是因为上面数组在每次 for 循环 count ++ 会执行 length 次
      return (count/ fingerTypesArr.Length == 5 - fingerTypesArr.Length);
    }
    
    bool isGrabHand(Hand hand)
    {
      return hand.GrabStrength > 0.8f;
    }

    bool IsCloseHand(Hand hand)
    {
      List<Finger> listOfFingers = hand.Fingers;
      int count = 0;
      for (int f = 0; f < listOfFingers.Count; f++)
      {
        Finger finger = listOfFingers[f];
        if ((finger.TipPosition - hand.PalmPosition).Magnitude < 0.05f)
        {
          count++;
        }
      }
      return (count == 4);
    }
    bool IsOpenFullHand(Hand hand)
    {
      return hand.GrabStrength == 0;
    }

    public override void UpdateHand() {
      if (_spherePositions == null || _spherePositions.Length != TOTAL_JOINT_COUNT) {
        _spherePositions = new Vector3[TOTAL_JOINT_COUNT];
      }

      if (_sphereMat == null) {
        _sphereMat = new Material(_material);
        _sphereMat.hideFlags = HideFlags.DontSaveInEditor;
      }
      
      
      Vector3 previousPosition = new Vector3(0.0f, 0.0f, 0.0f); 

      //Update all joint spheres in the fingers
      foreach (var finger in _hand.Fingers) {
        for (int j = 0; j < 4; j++) {
          int key = getFingerJointIndex((int)finger.Type, j);
          Vector3 position = finger.Bone((Bone.BoneType)j).NextJoint.ToVector3();
          if (j > 0) {
            Vector3 direction = position - finger.Bone((Bone.BoneType)(j-1)).NextJoint.ToVector3();
            position = previousPosition + direction * stretchFactor ;
          }
          else
          {
            Vector3 direction = position - _hand.PalmPosition.ToVector3();
            position = _hand.PalmPosition.ToVector3() + direction * stretchFactor ;
          }
          previousPosition = position;
          _spherePositions[key] = position;
        }
      }
      
      for (int i = 0; i < _spherePositions.Length; i++)
      {
        drawSphere(_spherePositions[i]);

      }

      //Now we just have a few more spheres for the hands
      //PalmPos, WristPos, and mockThumbJointPos, which is derived and not taken from the frame obj

      Vector3 palmPosition = _hand.PalmPosition.ToVector3();
      drawSphere(palmPosition, _palmRadius * stretchFactor);
      Debug.Log(palmPosition);

      Vector3 thumbBaseToPalm = _spherePositions[THUMB_BASE_INDEX] - _hand.PalmPosition.ToVector3();
      Vector3 mockThumbJointPos = _hand.PalmPosition.ToVector3() + Vector3.Reflect(thumbBaseToPalm, _hand.Basis.xBasis.ToVector3());
      drawSphere(mockThumbJointPos);



      //Draw cylinders between finger joints
      for (int i = 0; i < 5; i++) {
        for (int j = 0; j < 3; j++) {
          int keyA = getFingerJointIndex(i, j);
          int keyB = getFingerJointIndex(i, j + 1);

          Vector3 posA = _spherePositions[keyA];
          Vector3 posB = _spherePositions[keyB];

          drawCylinder(posA, posB);
        }
      }

      //Draw cylinders between finger knuckles
      for (int i = 0; i < 4; i++) {
        int keyA = getFingerJointIndex(i, 0);
        int keyB = getFingerJointIndex(i + 1, 0);

        Vector3 posA = _spherePositions[keyA];
        Vector3 posB = _spherePositions[keyB];

        drawCylinder(posA, posB);
      }

      //Draw the rest of the hand
      drawCylinder(mockThumbJointPos, THUMB_BASE_INDEX);
      drawCylinder(mockThumbJointPos, PINKY_BASE_INDEX);
    }

    private void drawSphere(Vector3 position) {
      drawSphere(position, _jointRadius * stretchFactor);
    }

    private void drawSphere(Vector3 position, float radius)
    {
      // return;
      if (isNaN(position)) { return; }

      //multiply radius by 2 because the default unity sphere has a radius of 0.5 meters at scale 1.
      Graphics.DrawMesh(_sphereMesh, 
                        Matrix4x4.TRS(position, 
                                      Quaternion.identity, 
                                      Vector3.one * radius * 2.0f * transform.lossyScale.x), 
                        _sphereMat, 0, 
                        null, 0, null, _castShadows);
    }

    private void drawCylinder(Vector3 a, Vector3 b)
    {
      // return;
      if (isNaN(a) || isNaN(b)) { return; }

      float length = (a - b).magnitude;

      if ((a - b).magnitude > 0.001f) {
        Graphics.DrawMesh(getCylinderMesh(length),
                          Matrix4x4.TRS(a, 
                                        Quaternion.LookRotation(b - a),
                                        new Vector3(transform.lossyScale.x, transform.lossyScale.x, 1)),
                          _sphereMat,
                          gameObject.layer, 
                          null, 0, null, _castShadows);
      }
    }

    private bool isNaN(Vector3 v) {
      return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z);
    }

    private void drawCylinder(int a, int b) {
      drawCylinder(_spherePositions[a], _spherePositions[b]);
    }

    private void drawCylinder(Vector3 a, int b) {
      drawCylinder(a, _spherePositions[b]);
    }

    private int getFingerJointIndex(int fingerIndex, int jointIndex) {
      return fingerIndex * 4 + jointIndex;
    }

    private Dictionary<int, Mesh> _meshMap = new Dictionary<int, Mesh>();
    private Mesh getCylinderMesh(float length) {
      int lengthKey = Mathf.RoundToInt(length * 100 / CYLINDER_MESH_RESOLUTION);

      Mesh mesh;
      if (_meshMap.TryGetValue(lengthKey, out mesh)) {
        return mesh;
      }

      mesh = new Mesh();
      mesh.name = "GeneratedCylinder";
      mesh.hideFlags = HideFlags.DontSave;

      List<Vector3> verts = new List<Vector3>();
      List<Color> colors = new List<Color>();
      List<int> tris = new List<int>();

      Vector3 p0 = Vector3.zero;
      Vector3 p1 = Vector3.forward * length;
      for (int i = 0; i < _cylinderResolution; i++) {
        float angle = (Mathf.PI * 2.0f * i) / _cylinderResolution;
        float dx = _cylinderRadius * stretchFactor * Mathf.Cos(angle);
        float dy = _cylinderRadius * stretchFactor * Mathf.Sin(angle);

        Vector3 spoke = new Vector3(dx, dy, 0);

        verts.Add(p0 + spoke);
        verts.Add(p1 + spoke);

        colors.Add(Color.white);
        colors.Add(Color.white);

        int triStart = verts.Count;
        int triCap = _cylinderResolution * 2;

        tris.Add((triStart + 0) % triCap);
        tris.Add((triStart + 2) % triCap);
        tris.Add((triStart + 1) % triCap);

        tris.Add((triStart + 2) % triCap);
        tris.Add((triStart + 3) % triCap);
        tris.Add((triStart + 1) % triCap);
      }

      mesh.SetVertices(verts);
      mesh.SetIndices(tris.ToArray(), MeshTopology.Triangles, 0);
      mesh.RecalculateBounds();
      mesh.RecalculateNormals();
      mesh.UploadMeshData(true);

      _meshMap[lengthKey] = mesh;

      return mesh;
    }
  }
}
