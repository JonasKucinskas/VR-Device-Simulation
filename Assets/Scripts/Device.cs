using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Device : MonoBehaviour
{
    public string deviceName;
    public List<Connection> connections = new List<Connection>();
    [HideInInspector] public int connectedCount = 0;

    void Start()
    {
        deviceName = gameObject.name;
    }
    public void PrintConnections()
    {
        foreach(Connection connection in connections)
        {
            print(connection.ToString());
        }
    }
}
[Serializable]
public class Connection : IEquatable<Connection>
{
    public Device otherDevice;
    public ConnectorData connectorData;
    public SocketType socketType;

    public Connection(Device otherDevice, ConnectorData connectorData, SocketType socketType)
    {
        this.otherDevice = otherDevice;
        this.connectorData = connectorData;
        this.socketType = socketType;
    }
    public override string ToString()
    {
        return connectorData.name + " to " + otherDevice.name;
    }
    public override bool Equals(object other)
    {
        Connection otherItem = other as Connection;

        if (otherItem == null)
        {
            return false;
        }

        return otherDevice.name == otherItem.otherDevice.name && connectorData == otherItem.connectorData && socketType == otherItem.socketType;
    }
    public bool Equals(Connection other)
    {
        if (other == null)
        {
            return false;
        }

        return otherDevice.name == other.otherDevice.name && connectorData == other.connectorData && socketType == other.socketType;
    }
    public override int GetHashCode()
    {
        int hash = 13;
        hash = (hash * 7) + otherDevice.name.GetHashCode();
        hash = (hash * 7) + connectorData.GetHashCode();
        return hash;
    }
}
