using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.XR;

public class CreateObject : MonoBehaviour
{
    [SerializeField] private GameObject[] objectPrefabs;
    [SerializeField] private GameObject controller;
    private int selectedPrefabIndex = -1;
    private int objectIndex = 0;
    private bool placing = false;
    private GameObject obj;
    private Color oldColor;
    // Start is called before the first frame update
    void Start()
    {

    }
    private GameObject SelectedPrefab
    {
        get
        {
            return selectedPrefabIndex >= 0 && selectedPrefabIndex < objectPrefabs.Length
                ? objectPrefabs[selectedPrefabIndex] : null;
        }
    }
    // Only objects created by this function will be saved to json
    public void Create()
    {
        if (SelectedPrefab != null)
        {
            GameObject obj = Instantiate(SelectedPrefab, controller.transform.position, controller.transform.rotation);
            MonoScript targetScript = MonoScript.FromMonoBehaviour(obj.GetComponent<Device>());

            if (targetScript != null)
            {
                obj.transform.parent = GameObject.Find("Devices").transform;
            }
            else
            {
                targetScript = MonoScript.FromMonoBehaviour(obj.GetComponent<Wire>());
                if (targetScript != null)
                {
                    obj.transform.parent = GameObject.Find("Connectors").transform;

                }
                else
                {
                    Debug.Log("Unknown device type");
                }
            }

            string prefabPath = AssetDatabase.GetAssetPath(SelectedPrefab);

            Debug.Log("Prefab path: " + prefabPath);
            if (ObjectPrefabPaths.paths.ContainsKey(obj.name))
            {
                obj.name = obj.name + objectIndex;
                objectIndex++;
                ObjectPrefabPaths.paths.Add(obj.name, prefabPath);
            }
            else
            {
                ObjectPrefabPaths.paths.Add(obj.name, prefabPath);
            }
            placing = true;
            var components = obj.GetComponentsInChildren<Rigidbody>();
            foreach (var component in components)
            {
                component.useGravity = false; // disable gravity while placing objects
                component.isKinematic = true;
            }
            //oldColor = obj.GetComponent<Renderer>().material.
        }
    }
    public void SelectPrefab(int index)
    {
        selectedPrefabIndex = index;
    }
    private void FixedUpdate()
    {
        if (placing)
        {
            //obj.GetComponent<Renderer>().material.SetColor("_Color", new Color(oldColor.r, oldColor.g, oldColor.b, 0f));
            bool triggerValue;
            InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.triggerButton,
                              out triggerValue);
            bool cancelValue;
            InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.triggerButton,
                  out cancelValue);
            if (triggerValue)
            {
                placing = false;
                var components = obj.GetComponentsInChildren<Rigidbody>();
                foreach (var component in components)
                {
                    component.useGravity = true; // restore gravity for placed obejcts
                }
                //obj.GetComponent<Renderer>().material.SetColor("_Color", new Color(oldColor.r, oldColor.g, oldColor.b, oldColor.a));
            }
            else
            {
                obj.transform.SetPositionAndRotation(controller.transform.position, controller.transform.rotation);
            }
            if (cancelValue)
            {
                placing = false;
                Destroy(obj);
            }
        }
    }
}

public static class ObjectPrefabPaths
{
    public static Dictionary<string, string> paths = new Dictionary<string, string>(); // key - object_name, value - prefab_path
}