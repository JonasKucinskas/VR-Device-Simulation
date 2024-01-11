using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR;
using UnityEditor;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;

public class EditManager : MonoBehaviour
{

    [SerializeField] private GameObject taskList;
    [SerializeField] private GameObject devicePrefab;
    [SerializeField] private GameObject itemPrefab; 

    //[SerializeField] private TMP_Dropdown deviceDropdown;

    [SerializeField] private GameObject[] devicePrefabs;
    [SerializeField] private GameObject[] cablePrefabs;
    [SerializeField] private GameObject controller;
    [SerializeField] private XRRayInteractor rayInteractor;
    private int selectedDeviceIndex = -1;
    private int selectedCableIndex = -1;
    private bool placing = false;
    private bool isDevice = false;
    private GameObject obj; // the obj that is created
    List<GameObject> spawnedGameObjects = new List<GameObject>();
    RequirementData requirementData;

    Dictionary<string, string> possibleConnections;

    // Start is called before the first frame update
    void Start()
    {   
        //declare possible opposite connections.
        possibleConnections = new Dictionary<string, string>();
        possibleConnections.Add("IN", "OUT");
        possibleConnections.Add("USB", "USB");
        possibleConnections.Add("3.5 mm Audio", "3.5 mm Audio");
        // Initial drop-down selections
        SelectDevice(0);
        SelectCable(0);
        requirementData = new RequirementData();
        requirementData.requiredConnections = new List<ConnectionRequirement>();
        //fillDeviceDropDown();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (placing)
        {
            RaycastHit res;
            bool triggerValue;
            InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.triggerButton,
                              out triggerValue);
            bool cancelValue;
            InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.triggerButton,
                  out cancelValue);
            Vector2 rotationValue;
            InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.primary2DAxis,
                out rotationValue);
            if (triggerValue)
            {
                placing = false;
                var components = obj.GetComponentsInChildren<Rigidbody>();
                foreach (var component in components)
                {
                    component.useGravity = true; // restore gravity for placed obejcts
                    //component.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                }
                updateTasks(obj);
            }
            else
            {
                if (rayInteractor.TryGetCurrent3DRaycastHit(out res)) // && surface is pointing up
                {
                    Vector3 groundPt = res.point; // the coordinate that the ray hits
                    if (isDevice)
                    {
                        Rigidbody rb = obj.GetComponent<Rigidbody>();
                        rb.MovePosition(groundPt);
                        //obj.GetComponent<Rigidbody>().MoveRotation(res.transform.rotation);
                        if (rotationValue != Vector2.zero)
                        {
                            rb.rotation *= Quaternion.AngleAxis(rotationValue.y * 10, new Vector3(0, rotationValue.y, 0));
                        }
                    }
                    else
                    {
                        // is a cable
                        foreach (var comp in obj.GetComponentsInChildren<Rigidbody>())
                        {
                            comp.MovePosition(groundPt);
                            if (rotationValue != Vector2.zero)
                            {
                                comp.rotation *= Quaternion.AngleAxis(rotationValue.y * 10, new Vector3(0, rotationValue.y, 0));
                            }
                        }
                    }
                    //Debug.Log(" coordinates on the ground: " + groundPt);
                }
                else
                {

                }

            }
            if (cancelValue)
            {
                placing = false;
                Destroy(obj);
            }
        }
    }

    /*private void fillDeviceDropDown()
    {
        DirectoryInfo dir = new DirectoryInfo("Assets/Resources/Prefabs/Devices");
        FileInfo[] files = dir.GetFiles("*.*");

        foreach (FileInfo file in files)
        {
            if (file.Name.EndsWith(".prefab"))
            {
                string name = file.Name.Replace(".prefab", "");
                deviceDropdown.options.Add(new TMP_Dropdown.OptionData() { text = name });
            }
        }
    }*/
    private GameObject SelectedDevice
    {
        get
        {
            return selectedDeviceIndex >= 0 && selectedDeviceIndex < devicePrefabs.Length
                ? devicePrefabs[selectedDeviceIndex] : null;
        }
    }
    private GameObject SelectedCable
    {
        get
        {
            return selectedCableIndex >= 0 && selectedCableIndex < cablePrefabs.Length
                ? cablePrefabs[selectedCableIndex] : null;
        }
    }
    //objectType need to be the same as the name of prefab folder.
    public void spawnObject(bool isDevice)
    {
        if (SelectedDevice != null || SelectedCable != null)
        {
            this.isDevice = isDevice;
            GameObject temp = isDevice ? SelectedDevice : SelectedCable;
            obj = Instantiate(temp, controller.transform.position, new Quaternion(0, 0, 0, 0));

            string prefabPath = AssetDatabase.GetAssetPath(temp);

            //Add to object or connectors list in unity hierarchy

            if (isDevice)
            {
                obj.transform.parent = GameObject.Find("Devices").transform;
            }
            else
            {
                obj.transform.parent = GameObject.Find("Connectors").transform;
            }

            //updateTasks(obj);
            Debug.Log("Prefab path: " + prefabPath);
            obj.name = obj.name.Replace("(Clone)", "") + obj.GetInstanceID();
            ObjectPrefabPaths.paths.Add(obj.name, prefabPath);
            placing = true;
            var components = obj.GetComponentsInChildren<Rigidbody>();
        }
    }
    public void SelectDevice(int index)
    {
        selectedDeviceIndex = index;
    }
    public void SelectCable(int index)
    {
        selectedCableIndex = index;
    }
    void updateTasks(GameObject newObject)
    {
        Device spawnedDevice = newObject.GetComponent<Device>();
        spawnedGameObjects.Add(newObject);

        foreach (GameObject gameObjectInScene in spawnedGameObjects)
        {
            ConnectionRequirement requirement = new ConnectionRequirement(spawnedDevice);

            for (int i = 0; i < newObject.transform.childCount; i++)
            {
                GameObject spawnedDevicePort = newObject.transform.GetChild(i).gameObject;
                
                if (!isConnectionPossible(gameObjectInScene, spawnedDevicePort))
                {
                    continue;
                }
                
                ConnectorData connector = new ConnectorData();
                connector.name = spawnedDevicePort.name;

                SocketType socketType = new SocketType();
                socketType = 0;

                Device device = gameObjectInScene.GetComponent<Device>();
                device.name = device.name.Replace("(Clone)", "");

                Connection connection = new Connection(device, connector, socketType);
                requirement.requiredConnections.Add(connection);
            }

            if (requirement.requiredConnections.Count > 0)
            {
                requirement.device.name = requirement.device.name.Replace("(Clone)", "");
                requirementData.requiredConnections.Add(requirement);
                FillTaskList(requirement);
            }
        }
        
    }
    
    private bool isConnectionPossible(GameObject gameObjectInScene, GameObject spawnedDevicePort)
    {
        for (int i = 0; i < gameObjectInScene.transform.childCount; i++)
        {
            GameObject oldDevicePort = gameObjectInScene.transform.GetChild(i).gameObject;


            foreach (var possibleConnection in possibleConnections)
            {
                if (oldDevicePort.name.Contains(possibleConnection.Key))
                {
                    string newName = oldDevicePort.name.Replace(possibleConnection.Key, "");

                    if (spawnedDevicePort.name.Contains(possibleConnection.Value) && spawnedDevicePort.name.Contains(newName))
                    {
                        return true;
                    }
                }
                else if (oldDevicePort.name.Contains(possibleConnection.Value)) 
                {
                    string newName = oldDevicePort.name.Replace(possibleConnection.Value, "");

                    if (spawnedDevicePort.name.Contains(possibleConnection.Key) && spawnedDevicePort.name.Contains(newName))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void FillTaskList(ConnectionRequirement connectionRequirement)
    {
        GameObject childDevice;
        GameObject childItem;

        Transform connectionList = getConnectionList(connectionRequirement.device);

        //if device doesnt have connections already, create new device and write connections to it.
        if (connectionList == null){
            
            childDevice = Instantiate(devicePrefab, taskList.transform);
            childDevice.transform.Find("Header").GetComponentInChildren<TextMeshProUGUI>().text = connectionRequirement.device.name;
            connectionList = childDevice.transform.Find("Connections").transform;
        }

        foreach (Connection connection in connectionRequirement.requiredConnections)
        {
            childItem = Instantiate(itemPrefab, connectionList);
            childItem.GetComponentInChildren<TextMeshProUGUI>().text = connection.ToString();
            childItem.GetComponentInChildren<TextMeshProUGUI>().enableAutoSizing = true;
            childItem.GetComponentInChildren<TextMeshProUGUI>().color = Color.red;
        }
    }

    /// <summary>
    /// if device already has tasks, get its connection list.
    /// </summary>
    /// <param name="device">latest spawned device</param>
    /// <returns></returns>
    private Transform getConnectionList(Device device)
    {
        for (int i = 0; i < taskList.transform.childCount; i++)
        {
            if (taskList.transform.GetChild(i).transform.Find("Header").GetComponentInChildren<TextMeshProUGUI>().text == device.name)
            {
                return taskList.transform.GetChild(i).transform.Find("Connections").transform;
            }
        }
        return null;
    }
}