﻿//  ---------------------------------------------------------------------
//  Copyright (c) 2016 Magic Leap. All Rights Reserved.
//  Magic Leap Confidential and Proprietary
//  ---------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(BoxCollider), typeof(Rigidbody), typeof(ConstantForce))]
public class FeatherFall : MonoBehaviour {
  [SerializeField, Range(0.01f, 1)] private float m_FloatForce = 0.8f;
  [SerializeField, Range(0.01f, 1)] private float m_SlidePower = 0.2f;
  [SerializeField, Range(0.01f, 1)] private float m_PuffPower = 0.05f;
  [SerializeField, Range(0.01f, 1)] private float m_PuffDelayMin = 0.2f;
  [SerializeField, Range(0.01f, 1)] private float m_PuffDelayMax = 0.3f;

  private Rigidbody m_Rigidbody;
  private BoxCollider m_Collider;
  private Vector3 m_AntigravityForce;

  private float m_LastTime;
  private float m_Delay;

  private Vector3[] m_EdgePoints;

  private void Start() {
    m_Rigidbody = GetComponent<Rigidbody>();
    m_Collider = GetComponent<BoxCollider>();
    m_AntigravityForce = GetAntigravityForce();
    GetComponent<ConstantForce>().force = m_AntigravityForce*m_FloatForce;

    Vector3 center = transform.InverseTransformPoint(m_Collider.bounds.center);
    Vector3 min = m_Collider.bounds.min;
    Vector3 max = m_Collider.bounds.max;

    m_EdgePoints = new[] {
      
      new Vector3(0, 0, min.z) + center,      //bottom
      new Vector3(0, 0, max.z) + center,      //top
      new Vector3(min.x, 0, 0) + center,      //left
      new Vector3(max.x, 0, 0) + center,      //right
      new Vector3(min.x, 0, min.z) + center,  //bottom left
      new Vector3(max.x, 0, min.z) + center,  //bottom right
      new Vector3(min.x, 0, max.z) + center,  //top left
      new Vector3(max.x, 0, max.z) + center}; //top right
  }

  private void Update() {
    if (!m_Rigidbody) {
      return;
    }

    UpdatePuffs();
    UpdateSlide();
  }

  private Vector3 m_SlideVector;

  private void UpdateSlide() {
    Vector3 normal = transform.up;
    m_SlideVector.x = normal.x*normal.y;
    m_SlideVector.z = normal.z*normal.y;
    m_SlideVector.y = -(normal.x*normal.x) - normal.z*normal.z;

    m_Rigidbody.AddForce(m_SlideVector.normalized*m_SlidePower);
  }

  private void UpdatePuffs() {
    if (m_LastTime + m_Delay < Time.time) {
      //Debug.Log("Puff!");
      Puff();
      m_LastTime = Time.time;
      m_Delay = Random.Range(m_PuffDelayMin, m_PuffDelayMax);
    }
  }

  private void Puff() {
    if (!m_Rigidbody) {
      return;
    }

    float downwardVelocity = -m_Rigidbody.velocity.y;
    if (downwardVelocity > 0.0001f) {
      m_Rigidbody.AddForceAtPosition(m_AntigravityForce*m_PuffPower*downwardVelocity, GetPuffPosition(), ForceMode.Impulse);
    }
  }

  private Vector3 GetPuffPosition() {
    Vector3 worldOffset = m_Collider.bounds.center;
    List<Vector3> worldEdges = m_EdgePoints.Select<Vector3, Vector3>(transform.TransformPoint).ToList();
    List<Vector3> validEdges = worldEdges.Where(v => v.y <= worldOffset.y).ToList();

    if (validEdges.Count == 0) {
      Debug.LogWarning("Couldn't find a lower edge.");
      validEdges = worldEdges;
    }

    int index = Random.Range(0, validEdges.Count - 1);
    return validEdges[index];
  }

  private Vector3 GetAntigravityForce() {
    float totalMass = transform.GetComponentsInChildren<Rigidbody>().Sum(rb => rb.mass);
    return Physics.gravity*totalMass*-1f;
  }
}