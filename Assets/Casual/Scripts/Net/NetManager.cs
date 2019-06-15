using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using System.Text;
using System;
using LitJson;

public enum RequestType
{
    Get,
    Post,
}

public class NetRequest<T> : IDisposable
{
    public string Proto { private set; get; }
    public string FinalUrl { private set; get; }
    public RequestType RequestType { private set; get; }
    public byte[] Bytes { private set; get; }
    public Action<T> FinishHandle { private set; get; }
    public Action<NetError> ErrorHandle { private set; get; }

    public void Init(string proto, RequestType requestType, byte[] data = null, Action<T> finishHandle = null, Action<NetError> errorHandle = null)
    {
        this.Proto = proto;
        this.FinalUrl = proto;
        this.RequestType = requestType;
        this.Bytes = data;
        this.FinishHandle = finishHandle;
        this.ErrorHandle = errorHandle;
    }

    public void InitGet(string proto, string getParams, Action<T> finishHandle = null, Action<NetError> errorHandle = null)
    {
        this.Proto = proto;
        this.FinalUrl = !string.IsNullOrEmpty(getParams) ? proto + "?" + getParams : proto;
        this.FinishHandle = finishHandle;
        this.ErrorHandle = errorHandle;
    }

    public void Dispose()
    {
        Proto = null;
        FinalUrl = null;
        Bytes = null;
        FinishHandle = null;
        ErrorHandle = null;
    }
}

public class NetManager : Singleton<NetManager>
{
    private Dictionary<string/*url_path*/, int> m_checkNetworkPass = new Dictionary<string, int>();

    /// <summary>
    /// 当finishHandle参数不需要处理的时候，调用此方法
    /// </summary>
    /// <param name="proto"></param>
    /// <param name="requestType"></param>
    /// <param name="data"></param>
    /// <param name="finishHandle"></param>
    /// <param name="errorHandle"></param>
    public void SendWebRequest(string proto, RequestType requestType, object data, Action<object> finishHandle = null, Action<NetError> errorHandle = null)
    {
        SendWebRequest(CreateRequest(proto, requestType, data, finishHandle, errorHandle));
    }

    /// <summary>
    /// 当finishHandle参数需要处理的时候，调用此方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proto"></param>
    /// <param name="requestType"></param>
    /// <param name="data"></param>
    /// <param name="finishHandle"></param>
    /// <param name="errorHandle"></param>
    public void SendWebRequest<T>(string proto, RequestType requestType, object data, Action<T> finishHandle = null, Action<NetError> errorHandle = null)
    {
        SendWebRequest(CreateRequest(proto, requestType, data, finishHandle, errorHandle));
    }

    /// <summary>
    /// 当Value不需要处理的时候调用此方法
    /// </summary>
    /// <param name="request"></param>
    public void SendWebRequest(NetRequest<object> request)
    {
        CoroutineAgent.StartCoroutine(_SendWebRequest<object>(request));
    }

    /// <summary>
    /// 使用NetManager.CreateRequest<T>()创建参数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="request"></param>
    public void SendWebRequest<T>(NetRequest<T> request)
    {
        try
        {
            CoroutineAgent.StartCoroutine(_SendWebRequest<T>(request));
        }
        catch (Exception e)
        {
            Debug.Log("SendWebRequest happend a error:" + e);
        }
    }

    private IEnumerator _SendWebRequest<T>(NetRequest<T> request)
    {
        byte[] data = request.Bytes;
        RequestType type = request.RequestType;

        string url = string.Format("{0}/{1}", GameConfigs.WebURL, request.FinalUrl);
        using (UnityWebRequest www = new UnityWebRequest(url, type == RequestType.Get ? UnityWebRequest.kHttpVerbGET : UnityWebRequest.kHttpVerbPOST))
        {
            www.uploadHandler = new UploadHandlerRaw(data);
            www.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
            www.downloadHandler = new DownloadHandlerBuffer();

            StartNetworkDelayCheck(url);

            Debug.Log(type.ToString() + ":" + www.url);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log("网络异常：" + www.error);
                request.ErrorHandle?.Invoke(null);
                //UIManager.Instance().Open<UIToast>("UIToast/UIToast", new ToastInfo()
                //{
                //    ToastID = 10001,
                //});
            }
            else
            {
                Debug.Log("网络正常：" + www.downloadHandler.text);
                NetMessage<T> netMessage = LitJson.JsonMapper.ToObject<NetMessage<T>>(www.downloadHandler.text);
                if (netMessage.succ)
                {
                    request.FinishHandle?.Invoke(netMessage.value);
                    EventManager.Brocast(request.Proto, netMessage.value);
                }
                else
                {
                    Debug.Log(netMessage.errorCode.message);
                    request.ErrorHandle?.Invoke(netMessage.errorCode);
                    //UIManager.Instance().Open<UIToast>("UIToast/UIToast", new ToastInfo()
                    //{
                    //    ToastValue = netMessage.errorCode.message,
                    //});
                }
            }

            StopNetworkDelayCheck(url);
            ObjectPoolManager.Instance().ReturnObject(request);
        }
    }

    private void StartNetworkDelayCheck(string url)
    {
        if (!m_checkNetworkPass.ContainsKey(url))
            m_checkNetworkPass[url] = 0;

        m_checkNetworkPass[url]++;

        if (m_checkNetworkPass.Count == 1)
        {
            //UIManager.Instance().Open<UINetworkDelay>();
        }
    }

    private void StopNetworkDelayCheck(string url)
    {
        if (m_checkNetworkPass.ContainsKey(url))
        {
            m_checkNetworkPass[url]--;
            if (m_checkNetworkPass[url] <= 0)
                m_checkNetworkPass.Remove(url);
        }

        if (m_checkNetworkPass.Count == 0)
        {
            //UIManager.Instance().Close<UINetworkDelay>();
        }
    }

    public static NetRequest<T> CreateRequest<T>(string proto, RequestType requestType, object data, Action<T> finishHandle = null, Action<NetError> errorHandle = null)
    {
        return CreateRequest(proto, requestType, data == null ? null : JsonMapper.ToJson(data), finishHandle, errorHandle);
    }

    public static NetRequest<T> CreateRequest<T>(string proto, RequestType requestType, string data, Action<T> finishHandle = null, Action<NetError> errorHandle = null)
    {
        if(requestType == RequestType.Get && !string.IsNullOrEmpty(data))
        {
            JsonData json = JsonMapper.ToObject(data);
            StringBuilder getParams = new StringBuilder();

            int index = 0;
            foreach(string key in json.Keys)
            {
                getParams.AppendFormat("{0}={1}", key, json[key]);
                if(index != json.Keys.Count - 1)
                {
                    getParams.Append("&");
                }
                index++;
            }

            return CreateRequest(proto, getParams.ToString(), finishHandle, errorHandle);
        }

        return CreateRequest(proto, requestType, data == null ? null : Encoding.UTF8.GetBytes(data), finishHandle, errorHandle);
    }

    /// <summary>
    /// 无参Get或者Post
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proto"></param>
    /// <param name="requestType"></param>
    /// <param name="data"></param>
    /// <param name="finishHandle"></param>
    /// <param name="errorHandle"></param>
    /// <returns></returns>
    private static NetRequest<T> CreateRequest<T>(string proto, RequestType requestType, byte[] data, Action<T> finishHandle, Action<NetError> errorHandle)
    {
        ObjectPoolManager.Instance().RegistCreater<NetRequest<T>>(() =>
        {
            return new NetRequest<T>();
        });
        NetRequest<T> netRequest = ObjectPoolManager.Instance().FetchObject<NetRequest<T>>();
        netRequest.Init(proto, requestType, data, finishHandle, errorHandle);
        return netRequest;
    }

    /// <summary>
    /// 创建GetRequest
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="proto"></param>
    /// <param name="getParams"></param>
    /// <param name="finishHandle"></param>
    /// <param name="errorHandle"></param>
    /// <returns></returns>
    private static NetRequest<T> CreateRequest<T>(string proto, string getParams, Action<T> finishHandle, Action<NetError> errorHandle)
    {
        ObjectPoolManager.Instance().RegistCreater<NetRequest<T>>(() =>
        {
            return new NetRequest<T>();
        });
        NetRequest<T> netRequest = ObjectPoolManager.Instance().FetchObject<NetRequest<T>>();
        netRequest.InitGet(proto, getParams, finishHandle, errorHandle);
        return netRequest;
    }
}
