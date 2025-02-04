using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class SimulationManager : MonoBehaviour
{
    [SerializeField] private GameObject taskList;
    [SerializeField] private GameObject devicePrefab;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private List<Device> devices;
    [SerializeField] private RequirementData requirementData;

    [SerializeField] TextMeshProUGUI result;
    List<GameObject> allTasks;
    List<DeviceWithTasks> allDevices;    //used to differentiate between identical tasks on different devices


    struct DeviceWithTasks
    {
        public Device device;
        public List<GameObject> deviceTasks;
    }

    void Start()
    {
        devices = new List<Device>(FindObjectsOfType<Device>());
        if(requirementData != null)
        {
            //FillTaskList();
        }
        foreach(ConnectionRequirement requiredConnection in requirementData.requiredConnections)
        {
            Debug.LogWarning(requiredConnection.device.name);
        }
    }

    void Update()
    {

    }
    private void FillTaskList()
    {
        GameObject childDevice;
        GameObject childItem;
        allDevices = new List<DeviceWithTasks>();
        foreach (ConnectionRequirement connectionRequirement in requirementData.requiredConnections)
        {
            childDevice = Instantiate(devicePrefab, taskList.transform);
            childDevice.transform.Find("Header").GetComponentInChildren<TextMeshProUGUI>().text = connectionRequirement.device.name;
            List<GameObject> tasks = new List<GameObject>();

            //childDevice.transform.Find("Header").GetComponentInChildren<TextMeshProUGUI>().text = "Something";
            //childDevice.transform.Find("Header").GetComponentInChildren<TextMeshProUGUI>().text = "Something";
            foreach (Connection connection in connectionRequirement.requiredConnections)
            {
                childItem = Instantiate(itemPrefab, childDevice.transform.Find("Connections").transform);
                childItem.GetComponentInChildren<TextMeshProUGUI>().text = connection.ToString();
                childItem.GetComponentInChildren<TextMeshProUGUI>(). enableAutoSizing = true;
                childItem.GetComponentInChildren<TextMeshProUGUI>().color = Color.red;
                tasks.Add(childItem);
            }
            allDevices.Add(new DeviceWithTasks { device = connectionRequirement.device, deviceTasks = tasks });
        }
    }

    

    public void CheckResults()
    {
        int solvedCount = 0;
        if(requirementData != null)
        {
            foreach (Device device in devices)
            {
                int count = 0;
                ConnectionRequirement connectionRequirement = requirementData.requiredConnections.Find(item => item.device.name == device.name);
                Debug.LogWarning(device.name);
                if(connectionRequirement != null)
                {
                    Debug.LogWarning(connectionRequirement.device.name);
                    foreach (Connection connection in device.connections)
                    {
                        Debug.LogWarning(connection.ToString());
                        Debug.LogWarning(connectionRequirement.requiredConnections[0].ToString());
                        if(connection.Equals(connectionRequirement.requiredConnections[0]))
                        {
                            Debug.LogWarning("Bing chilling");
                        }
                        if (connectionRequirement.requiredConnections.Contains(connection))
                        {
                            count++;
                            
                            foreach (DeviceWithTasks checkDevice in allDevices)
                            {
                                if (checkDevice.device.name == connectionRequirement.device.name)
                                {
                                    Debug.LogWarning("Found device");
                                    foreach (GameObject task in checkDevice.deviceTasks)
                                    {
                                        if (task.GetComponentInChildren<TextMeshProUGUI>().text == connection.ToString())
                                        {
                                            task.GetComponentInChildren<TextMeshProUGUI>().color = Color.green;
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                            
                            Debug.LogWarning("Connection valid: " + connectionRequirement.device.name + " connected to " + connection.otherDevice.name + " via " + connection.connectorData.name);
                        }
                    }
                    if (count == connectionRequirement.requiredConnections.Count)
                    {
                        Debug.LogWarning("Count + 1");
                        solvedCount++;
                    }
                }
            }
        }
        if(result != null && requirementData != null)
        {
            if(solvedCount == requirementData.requiredConnections.Count)
            {
                result.text = "Solved";
            }
            else
            {
                result.text = "Not solved";
            }
        }
    }
    public void CloseApplication()
    {
        Debug.LogWarning("Quitting");
        Application.Quit();
    }
    public void ReloadScenario()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
[Serializable]
public class ConnectionRequirement
{
    public Device device;
    public List<Connection> requiredConnections;

    public ConnectionRequirement(Device device)
    {
        this.device = device;
        requiredConnections = new List<Connection>();
    }
}
