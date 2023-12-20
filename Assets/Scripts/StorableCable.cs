using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System;


[System.Serializable]
public class StorableCable: StorableObject
{
    public Vector3 positionOfFirstPlug;
    public Quaternion rotationOfFirstPlug;
    public Vector3 positionOfSecondPlug;
    public Quaternion rotationOfSecondPlug;
    public Dictionary<string, string> storedComponents; // not working yet
    //public List<SerializableComponent> components;

    public StorableCable(string name, string path, Vector3 pos, Quaternion rot, Vector3 positionOfFirstPlug, Quaternion rotationOfFirstPlug, Vector3 positionOfSecondPlug, Quaternion rotationOfSecondPlug): base(name, path, pos, rot)
    {
        this.positionOfFirstPlug = positionOfFirstPlug;
        this.rotationOfFirstPlug = rotationOfFirstPlug;
        this.positionOfSecondPlug = positionOfSecondPlug;
        this.rotationOfSecondPlug = rotationOfSecondPlug;
    }
    public string Serialize(GameObject obj)
    {
        storedComponents = new Dictionary<string, string>();

        //IStorableComponent[] components = obj.GetComponents<IStorableComponent>();
        //Component[] components = obj.GetComponents<Component>();
        //foreach (Component comp in components)
        //{
        //storedComponents.Add(comp.GetType().Name, JsonUtility.ToJson(comp));
        //}

        return JsonUtility.ToJson(this);
    }
}
[System.Serializable]
public class StorableCableList
{
    public List<StorableCable> JSONList;
}