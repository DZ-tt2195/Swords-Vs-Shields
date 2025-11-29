using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Reflection;
using System;
using System.Linq.Expressions;
using System.Linq;

[RequireComponent(typeof(PhotonView))]
public class PhotonCompatible : MonoBehaviourPunCallbacks
{

#region Setup

    Dictionary<string, MethodInfo> methodDictionary = new();
    protected Type bottomType;

    protected virtual void Awake()
    {
    }

    #endregion

#region Functions

    public void StringParameters(string methodName, object[] parameters)
    {
        MethodInfo info = FindMethod(methodName);
        if (info == null)
            Debug.LogError($"{this.name} - {methodName} failed");

        if (info.ReturnType == typeof(IEnumerator))
            StartCoroutine((IEnumerator)info.Invoke(this, parameters));
        else
            info.Invoke(this, parameters);
    }

    public (string instruction, object[] parameters) TranslateFunction(Expression<Action> expression)
    {
        if (expression.Body is MethodCallExpression methodCall)
        {
            ParameterInfo[] parameters = methodCall.Method.GetParameters();
            object[] arguments = new object[methodCall.Arguments.Count];

            for (int i = 0; i < methodCall.Arguments.Count; i++)
            {
                var argumentExpression = Expression.Lambda(methodCall.Arguments[i]).Compile();
                arguments[i] = argumentExpression.DynamicInvoke();
                //Debug.Log($"{parameters[i].Name}, {parameters[i].ParameterType.Name}, {arguments[i]}");
            }

            return (methodCall.Method.Name, arguments);
        }
        return (null, null);
    }

    public void DoFunction(Expression<Action> expression, RpcTarget affects = RpcTarget.AllBuffered)
    {
        (string instruction, object[] parameters) = this.TranslateFunction(expression);

        MethodInfo info = FindMethod(instruction);
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC(info.Name, affects, parameters);
        }
        else if (affects != RpcTarget.Others)
        {
            if (info.ReturnType == typeof(IEnumerator))
                StartCoroutine((IEnumerator)info.Invoke(this, parameters));
            else
                info.Invoke(this, parameters);
        }
    }

    public void DoFunction(Expression<Action> expression, int specificPlayerNumber)
    {
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            if (player.ActorNumber == specificPlayerNumber)
            {
                DoFunction(expression, player);
                break;
            }
        }
    }

    public void DoFunction(Expression<Action> expression, Photon.Realtime.Player specificPlayer)
    {
        (string instruction, object[] parameters) = this.TranslateFunction(expression);

        MethodInfo info = FindMethod(instruction);
        if (PhotonNetwork.IsConnected && specificPlayer != null)
            photonView.RPC(info.Name, specificPlayer, parameters);
        else if (info.ReturnType == typeof(IEnumerator))
            StartCoroutine((IEnumerator)info.Invoke(this, parameters));
        else
            info.Invoke(this, parameters);
    }

    protected MethodInfo FindMethod(string methodName)
    {
        if (methodDictionary.ContainsKey(methodName))
            return methodDictionary[methodName];

        MethodInfo method = null;
        Type currentType = bottomType;

        try
        {
            while (currentType != null && method == null)
            {
                method = currentType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                if (method == null)
                    currentType = currentType.BaseType;
                else
                    break;
            }
            if (method != null)
            {
                methodDictionary.Add(methodName, method);
            }
        }
        catch (ArgumentException) { }
        catch
        {
            Debug.LogError($"{this.name}: {methodName} failed");
        }
        return method;
    }

    #endregion

#region Properties

    public static void InstantChangePlayerProp(Photon.Realtime.Player player, string propertyName, object changeToThis, object expected = null)
    {
        ExitGames.Client.Photon.Hashtable changeTable = new() { { propertyName.ToString(), changeToThis } };
        ExitGames.Client.Photon.Hashtable expectedTable = null;
        if (expected != null && player.CustomProperties.ContainsKey(propertyName.ToString()))
            expectedTable = new() { { propertyName.ToString(), expected } };

        if (player.SetCustomProperties(changeTable, expectedTable))
        {
        }
        else
        {
            Debug.Log($"change to {propertyName} has been rejected");
        }
    }

    public static void InstantChangePlayerProp(Player playerObject, string propertyName, object changeToThis, object expected = null)
        => InstantChangePlayerProp(playerObject.photonView.Controller, propertyName, changeToThis, expected);

    public static object GetPlayerProperty(Photon.Realtime.Player player, string propertyName)
    {
        if (player == null)
        {
            Debug.LogError("failed to get player");
            return null;
        }
        else if (player.CustomProperties.ContainsKey(propertyName))
        {
            return player.CustomProperties[propertyName];
        }
        else
        {
            Debug.LogError($"property {propertyName} doesn't exist for {player.NickName}");
            return null;
        }
    }

    public static object GetPlayerProperty(Player player, string propertyName) => GetPlayerProperty(player.photonView.Owner, propertyName);

    public static void InstantChangeRoomProp(string propertyName, object changeToThis, object expected = null)
    {
        ExitGames.Client.Photon.Hashtable changeTable = new() { { propertyName, changeToThis } };
        ExitGames.Client.Photon.Hashtable expectedTable = null;
        if (expected != null && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(propertyName))
            expectedTable = new() { { propertyName, expected } };

        if (PhotonNetwork.CurrentRoom.SetCustomProperties(changeTable, expectedTable))
        {
        }
        else
        {
            Debug.Log($"change to {propertyName} has been rejected");
        }
    }

    public static object GetRoomProperty(string propertyName)
        => PhotonNetwork.CurrentRoom.CustomProperties[propertyName];

    #endregion

#region Misc

    public static (List<Photon.Realtime.Player>, List<Photon.Realtime.Player>) GetPlayers(bool printLog)
    {
        List<Photon.Realtime.Player> players = new();
        List<Photon.Realtime.Player> spectators = new();

        foreach (Photon.Realtime.Player nextPlayer in PhotonNetwork.CurrentRoom.Players.Values.OrderBy(p => p.ActorNumber))
        {
            if ((int)GetPlayerProperty(nextPlayer, ConstantStrings.MyPosition) == -1)
            {
                spectators.Add(nextPlayer);
                if (printLog)
                    Debug.Log($"spectating (inactive: {nextPlayer.IsInactive}, {nextPlayer.ActorNumber}): {nextPlayer.NickName}");
            }
            else
            {
                players.Add(nextPlayer);
                if (printLog)
                    Debug.Log($"playing (inactive: {nextPlayer.IsInactive}, {nextPlayer.ActorNumber}): {nextPlayer.NickName}");
            }
        }

        return (players, spectators);
    }

    public GameObject MakeObject(GameObject prefab)
    {
        if (PhotonNetwork.IsConnected)
            return PhotonNetwork.Instantiate(prefab.name, Vector3.zero, Quaternion.identity);
        else
            return Instantiate(prefab);
    }

    public bool AmMaster()
    {
        return !PhotonNetwork.IsConnected || PhotonNetwork.IsMasterClient;
    }

    #endregion

}