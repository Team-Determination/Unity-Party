using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace Unity.RemoteConfig
{
  public interface IRCUnityWebRequest
  {
    UnityWebRequest unityWebRequest { get; set; }

    /// <summary>
    ///   <para>If true, any CertificateHandler attached to this UnityWebRequest will have CertificateHandler.Dispose called automatically when UnityWebRequest.Dispose is called.</para>
    /// </summary>
    bool disposeCertificateHandlerOnDispose { get; set; }

    /// <summary>
    ///   <para>If true, any DownloadHandler attached to this UnityWebRequest will have DownloadHandler.Dispose called automatically when UnityWebRequest.Dispose is called.</para>
    /// </summary>
    bool disposeDownloadHandlerOnDispose { get; set; }

    /// <summary>
    ///   <para>If true, any UploadHandler attached to this UnityWebRequest will have UploadHandler.Dispose called automatically when UnityWebRequest.Dispose is called.</para>
    /// </summary>
    bool disposeUploadHandlerOnDispose { get; set; }

    /// <summary>
    ///   <para>Defines the HTTP verb used by this UnityWebRequest, such as GET or POST.</para>
    /// </summary>
    string method { get; set; }

    /// <summary>
    ///   <para>A human-readable string describing any system errors encountered by this UnityWebRequest object while handling HTTP requests or responses. (Read Only)</para>
    /// </summary>
    string error { get; }

    /// <summary>
    ///   <para>Determines whether this UnityWebRequest will include Expect: 100-Continue in its outgoing request headers. (Default: true).</para>
    /// </summary>
    bool useHttpContinue { get; set; }

    /// <summary>
    ///   <para>Defines the target URL for the UnityWebRequest to communicate with.</para>
    /// </summary>
    string url { get; set; }

    /// <summary>
    ///   <para>Defines the target URI for the UnityWebRequest to communicate with.</para>
    /// </summary>
    Uri uri { get; set; }

    /// <summary>
    ///   <para>The numeric HTTP response code returned by the server, such as 200, 404 or 500. (Read Only)</para>
    /// </summary>
    long responseCode
    {
      get;
    }

    /// <summary>
    ///   <para>Returns a floating-point value between 0.0 and 1.0, indicating the progress of uploading body data to the server.</para>
    /// </summary>
    float uploadProgress { get; }

    /// <summary>
    ///   <para>Returns true while a UnityWebRequest’s configuration properties can be altered. (Read Only)</para>
    /// </summary>
    bool isModifiable
    {
      get;
    }

    /// <summary>
    ///   <para>Returns true after the UnityWebRequest has finished communicating with the remote server. (Read Only)</para>
    /// </summary>
    bool isDone
    {
      get;
    }

    /// <summary>
    ///   <para>Returns true after this UnityWebRequest encounters a system error. (Read Only)</para>
    /// </summary>
    bool isNetworkError
    {
      get;
    }

    /// <summary>
    ///   <para>Returns true after this UnityWebRequest receives an HTTP response code indicating an error. (Read Only)</para>
    /// </summary>
    bool isHttpError
    {
      get;
    }

    /// <summary>
    ///   <para>Returns a floating-point value between 0.0 and 1.0, indicating the progress of downloading body data from the server. (Read Only)</para>
    /// </summary>
    float downloadProgress { get; }

    /// <summary>
    ///   <para>Returns the number of bytes of body data the system has uploaded to the remote server. (Read Only)</para>
    /// </summary>
    ulong uploadedBytes
    {
      get;
    }

    /// <summary>
    ///   <para>Returns the number of bytes of body data the system has downloaded from the remote server. (Read Only)</para>
    /// </summary>
    ulong downloadedBytes
    {
      get;
    }

    /// <summary>
    ///   <para>Indicates the number of redirects which this UnityWebRequest will follow before halting with a “Redirect Limit Exceeded” system error.</para>
    /// </summary>
    int redirectLimit { get; set; }

    /// <summary>
    ///   <para>Indicates whether the UnityWebRequest system should employ the HTTP/1.1 chunked-transfer encoding method.</para>
    /// </summary>
    bool chunkedTransfer { get; set; }

    /// <summary>
    ///   <para>Holds a reference to the UploadHandler object which manages body data to be uploaded to the remote server.</para>
    /// </summary>
    UploadHandler uploadHandler { get; set; }

    /// <summary>
    ///   <para>Holds a reference to a DownloadHandler object, which manages body data received from the remote server by this UnityWebRequest.</para>
    /// </summary>
    DownloadHandler downloadHandler { get; set; }

    /// <summary>
    ///   <para>Holds a reference to a CertificateHandler object, which manages certificate validation for this UnityWebRequest.</para>
    /// </summary>
    CertificateHandler certificateHandler { get; set; }

    /// <summary>
    ///   <para>Sets UnityWebRequest to attempt to abort after the number of seconds in timeout have passed.</para>
    /// </summary>
    int timeout { get; set; }

    /// <summary>
    ///   <para>Signals that this UnityWebRequest is no longer being used, and should clean up any resources it is using.</para>
    /// </summary>
    void Dispose();

    /// <summary>
    ///   <para>Begin communicating with the remote server.</para>
    /// </summary>
    UnityWebRequestAsyncOperation SendWebRequest();

    /// <summary>
    ///   <para>If in progress, halts the UnityWebRequest as soon as possible.</para>
    /// </summary>
    void Abort();

    /// <summary>
    ///   <para>Retrieves the value of a custom request header.</para>
    /// </summary>
    /// <param name="name">Name of the custom request header. Case-insensitive.</param>
    /// <returns>
    ///   <para>The value of the custom request header. If no custom header with a matching name has been set, returns an empty string.</para>
    /// </returns>
    string GetRequestHeader(string name);

    /// <summary>
    ///   <para>Set a HTTP request header to a custom value.</para>
    /// </summary>
    /// <param name="name">The key of the header to be set. Case-sensitive.</param>
    /// <param name="value">The header's intended value.</param>
    void SetRequestHeader(string name, string value);

    /// <summary>
    ///   <para>Retrieves the value of a response header from the latest HTTP response received.</para>
    /// </summary>
    /// <param name="name">The name of the HTTP header to retrieve. Case-insensitive.</param>
    /// <returns>
    ///   <para>The value of the HTTP header from the latest HTTP response. If no header with a matching name has been received, or no responses have been received, returns null.</para>
    /// </returns>
    string GetResponseHeader(string name);

    /// <summary>
    ///   <para>Retrieves a dictionary containing all the response headers received by this UnityWebRequest in the latest HTTP response.</para>
    /// </summary>
    /// <returns>
    ///   <para>A dictionary containing all the response headers received in the latest HTTP response. If no responses have been received, returns null.</para>
    /// </returns>
    Dictionary<string, string> GetResponseHeaders();
  }
}