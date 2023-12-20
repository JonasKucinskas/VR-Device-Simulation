using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private List<string> paths;
    private string objectPath;
    private string conectionPath;
    public void Start()
    {

    }
    public void Save()
    {
        JsonAllObjectsArray allObjects = new JsonAllObjectsArray();
        List<string> deviceType = new List<string>();
        deviceType.Add("Devices");
        deviceType.Add("Connectors");
        Connector[] allConnectors = GameObject.FindObjectsOfType<Connector>();
        string path = Application.persistentDataPath + "/LevelData.json";
        if (File.Exists(path))
            File.Delete(path);
        else
            File.Create(path);
        foreach (string type in deviceType)
        {
            Transform transform;
            transform = GameObject.Find(type).transform;
            object[] data = transform.GetComponentsInChildren<Transform>();
            int deviceCount = 0;
            int connectorCount = 0;
            foreach (Transform obj in transform)
            {
                GameObject go = obj.gameObject;
                string prefabPath = "";
                if (ObjectPrefabPaths.paths.ContainsKey(go.name))
                {
                    prefabPath = ObjectPrefabPaths.paths[go.name];
                    StorableObject storable;
                    if (prefabPath.Contains("Devices"))
                    {
                        deviceCount++;
                        storable = new StorableObject(go.name, prefabPath, go.transform.position, go.transform.rotation);
                        allObjects.JSONListDevices.Add(storable);
                    }
                    else
                    {
                        if (prefabPath.Contains("Cables"))
                        {
                            connectorCount++;
                            List<Connector> thisConn = new List<Connector>();
                            foreach (Connector conn in allConnectors)
                            {
                                if (conn.originalParent.name == go.name)
                                {
                                    thisConn.Add(conn);
                                }
                            }
                            storable = new StorableCable(go.name, prefabPath, go.transform.position, go.transform.rotation, thisConn[0].gameObject.transform.position, thisConn[0].gameObject.transform.rotation, thisConn[1].gameObject.transform.position, thisConn[1].gameObject.transform.rotation);
                            allObjects.JSONListConnectors.Add((StorableCable)storable);
                        }
                        else
                        {
                            storable = new StorableObject(go.name, prefabPath, go.transform.position, go.transform.rotation);
                            allObjects.JSONListDevices.Add(storable);
                        }
                    }
                    //todo: fix so that components loaded from json are will be saved when trying to save again
                }
            }
        }
        string json = JsonUtility.ToJson(allObjects);
        File.WriteAllText(path, json);

        Debug.Log("Saved to: " + path);
    }

    public static void Load() // sceneName - scene, where objects should be loaded
    {
        //SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        //Debug.Log(SceneManager.GetActiveScene().name);

        string path = Application.persistentDataPath + "/LevelData.json";

        if (File.Exists(path))
        {
            string JsonData = File.ReadAllText(path);

            JsonAllObjectsArray allObjects = JsonUtility.FromJson<JsonAllObjectsArray>(JsonData);

            StorableObject[] objects = allObjects.JSONListDevices.ToArray();
            StorableCable[] cables = allObjects.JSONListConnectors.ToArray();

            foreach (StorableObject obj in objects)
            {
                var prefab = AssetDatabase.LoadAssetAtPath(obj.prefabPath, typeof(GameObject));
                GameObject instObj = Instantiate(prefab, obj.position, obj.rotation) as GameObject; ;

                instObj.name = obj.objectName;

                instObj.transform.parent = GameObject.Find("Devices").transform;

                //Add to prefab list

                if (!ObjectPrefabPaths.paths.ContainsKey(obj.objectName))
                {
                    ObjectPrefabPaths.paths.Add(obj.objectName, obj.prefabPath);
                }
            }

            foreach(StorableCable cable in cables)
            {
                var prefab = AssetDatabase.LoadAssetAtPath(cable.prefabPath, typeof(GameObject));
                GameObject go = Instantiate(prefab, cable.position, cable.rotation) as GameObject;

                go.name = cable.objectName;

                go.transform.parent = GameObject.Find("Connectors").transform;

                go.transform.GetChild(0).position = cable.positionOfFirstPlug;
                go.transform.GetChild(0).rotation = cable.rotationOfFirstPlug;
                go.transform.GetChild(1).position = cable.positionOfSecondPlug;
                go.transform.GetChild(1).rotation = cable.rotationOfSecondPlug;

                if (!ObjectPrefabPaths.paths.ContainsKey(cable.objectName))
                {
                    ObjectPrefabPaths.paths.Add(cable.objectName, cable.prefabPath);
                }
            }
        }
        else
            Debug.Log("Saved file does not exist!");
    }

}

[System.Serializable]
public class JsonAllObjectsArray
{
    public List<StorableObject> JSONListDevices;
    public List<StorableCable> JSONListConnectors;

    public JsonAllObjectsArray() 
    { 
        JSONListConnectors = new List<StorableCable>();
        JSONListDevices = new List<StorableObject>();
    }
}

