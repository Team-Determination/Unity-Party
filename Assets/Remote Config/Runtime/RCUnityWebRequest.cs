using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace Unity.RemoteConfig
{
    public class RCUnityWebRequest : IRCUnityWebRequest
    {

        UnityWebRequest _unityWebRequest { get; set; }
        public bool isHttpError => _unityWebRequest.isHttpError;

        public float downloadProgress => _unityWebRequest.downloadProgress;
        public ulong uploadedBytes => _unityWebRequest.uploadedBytes;
        public ulong downloadedBytes => _unityWebRequest.downloadedBytes;

        public int redirectLimit
        {
            get => _unityWebRequest.redirectLimit;
            set => redirectLimit = value;
        }

        public bool chunkedTransfer
        {
            get => _unityWebRequest.chunkedTransfer;
            set => chunkedTransfer = value;
        }

        public bool isDone => _unityWebRequest.isDone;

        public bool isNetworkError => _unityWebRequest.isNetworkError;

        public string url
        {
            get => _unityWebRequest.url;
            set => _unityWebRequest.url = value;
        }

        public Uri uri
        {
            get => _unityWebRequest.uri;
            set => _unityWebRequest.uri = value;
        }

        public long responseCode => _unityWebRequest.responseCode;

        public float uploadProgress => _unityWebRequest.uploadProgress;
        public bool isModifiable => _unityWebRequest.isModifiable;

        public bool disposeUploadHandlerOnDispose
        {
            get => _unityWebRequest.disposeUploadHandlerOnDispose;
            set => _unityWebRequest.disposeUploadHandlerOnDispose = value;
        }

        public string method
        {
            get => _unityWebRequest.method;
            set => _unityWebRequest.method = value;
        }

        public string error => _unityWebRequest.error;

        public bool useHttpContinue
        {
            get => _unityWebRequest.useHttpContinue;
            set => _unityWebRequest.useHttpContinue = value;
        }

        public UnityWebRequest unityWebRequest
        {
            get { return _unityWebRequest; }
            set { _unityWebRequest = value; }
        }

        public bool disposeCertificateHandlerOnDispose
        {
            get => _unityWebRequest.disposeCertificateHandlerOnDispose;
            set => _unityWebRequest.disposeCertificateHandlerOnDispose = value;
        }

        public bool disposeDownloadHandlerOnDispose
        {
            get => _unityWebRequest.disposeDownloadHandlerOnDispose;
            set => _unityWebRequest.disposeDownloadHandlerOnDispose = value;
        }

        public DownloadHandler downloadHandler
        {
            get { return _unityWebRequest.downloadHandler; }
            set { _unityWebRequest.downloadHandler = value; }
        }

        public CertificateHandler certificateHandler
        {
            get => _unityWebRequest.certificateHandler;
            set => _unityWebRequest.certificateHandler = value;
        }

        public int timeout
        {
            get => _unityWebRequest.timeout;
            set => _unityWebRequest.timeout = value;
        }

        public void Dispose()
        {
            _unityWebRequest.Dispose();
        }

        public UploadHandler uploadHandler
        {
            get { return _unityWebRequest.uploadHandler; }
            set { _unityWebRequest.uploadHandler = value; }
        }

        public UnityWebRequestAsyncOperation SendWebRequest()
        {
            return _unityWebRequest.SendWebRequest();
        }

        public void Abort()
        {
            _unityWebRequest.Abort();
        }

        public string GetRequestHeader(string name)
        {
            return _unityWebRequest.GetRequestHeader(name);
        }

        public void SetRequestHeader(string name, string value)
        {
            _unityWebRequest.SetRequestHeader(name, value);
        }

        public string GetResponseHeader(string name)
        {
            return _unityWebRequest.GetResponseHeader(name);
        }

        public Dictionary<string, string> GetResponseHeaders()
        {
            return _unityWebRequest.GetResponseHeaders();
        }
    }
}