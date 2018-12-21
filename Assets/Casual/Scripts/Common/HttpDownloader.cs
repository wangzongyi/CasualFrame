using System.Collections;
using UnityEngine;
using System;

public class HttpDownloader
{
    public HttpDownloader(string url, MonoBehaviour behaviour, Action<HttpDownloader> callback = null)
    {
        _error = null;
        behaviour.StartCoroutine(Download(url, callback));
    }

    private WWW _www;

    private string _error;
    public string Error
    {
        get { return _error ?? (_www == null ? "unknown" : _www.error); }
    }
    public string URL { get; private set; }
    public bool IsDownloading { get; private set; }

    IEnumerator Download(string url, Action<HttpDownloader> callback)
    {
        this.URL = url;
        this.IsDownloading = true;
        float timer = 0;

        for (int times = 0; times < GameConfigs.TRY_DOWNLOAD_TIMES; times++)
        {
            float progress = 0;
            if (_www != null)
            {
                _www.Dispose();
            }

            _www = new WWW(url);

            while (!_www.isDone && _www.error == null)
            {
                if (progress == _www.progress)
                {
                    timer += Time.deltaTime;
                }
                else
                {
                    progress = _www.progress;
                    timer = 0;
                }

                if (timer > GameConfigs.DDOWNLOAD_TIMEOUT)
                {
                    _error = "Timeout";
                    break;
                }

                yield return null;
            }

            if (_error == null && _www.error == null)
                break;
        }
        IsDownloading = false;

        callback?.Invoke(this);
    }

    public bool IsDone
    {
        get
        {
            return _www != null && _www.error == null && _www.isDone;
        }
    }

    public float Progress
    {
        get
        {
            return (_www == null) ? 0f : _www.progress;
        }
    }

    public AssetBundle assetBundle
    {
        get
        {
            return IsDone ? _www.assetBundle : null;
        }
    }

    public byte[] Bytes
    {
        get
        {
            return IsDone ? _www.bytes : null;
        }
    }

    public string Text
    {
        get
        {
            return IsDone ? _www.text : string.Empty;
        }
    }

    public Texture Texture
    {
        get
        {
            return IsDone ? _www.texture : null;
        }
    }

    public void Dispose()
    {
        if (_www != null)
        {
            _www.Dispose();
        }
    }
}
