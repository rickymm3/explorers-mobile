using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Networking;
using ExtensionMethods;
using RSG;
using SimpleJSON;
using Newtonsoft.Json;
using NodeJS;

using NodePromise = RSG.IPromise<NodeJS.NodeResponse>;

public class NodeJSManager<T> : ManagerSingleton<T> where T : Tracer {
    static string AUTH_DELIMITER = "::";
    static string PREF_DELIMITER = " -- ";
    static string PREF_NAME = "PREF:NODE_USER_TOKEN";
    static string PREF_LAST_USERNAME = "PREF:NODE_LAST_USERNAME";

    RemoteCertificateValidationCallback _tempCertCallback;

    public bool isLocal = false;
    public string urlLocal = "http://localhost:9000/api";
    public string urlRemote = "http://ec2-52-9-42-190.us-west-1.compute.amazonaws.com:9000/api";

    public string urlCurrent {
        get { return isLocal ? urlLocal : urlRemote; }
    }

    public bool acceptSelfSignedCert = true;
    public string authCode = "sf-dev";
    protected string _authUser;
    protected string _authToken;
    protected string _authPassMD5;
    string _authCodeB64;

    List<NodeRequest> _pendingRequests;
    NodeRequest _currentRequest;

    bool _isLoggedIn = false;
    public bool isLoggedIn { get { return _isLoggedIn; } }
    public string username { get { return _authUser; } } //DON'T ADD SETTER, try using SetCurrentUser instead.
    public string usernameLast { get { return PlayerPrefs.GetString(PREF_LAST_USERNAME); } }
    
    public void SetCurrentUser(string username, string token, string passwordMD5) {
        // When a USER Authentication code is assigned,
        // concat the 'authCode' and '_authUser' in Base64 format:
        _authUser = username;
        _authToken = token;
        _authCodeB64 = EncodeToken(authCode, _authUser, _authToken);
        _authPassMD5 = passwordMD5;
        _isLoggedIn = true;

        PlayerPrefs.SetString(PREF_LAST_USERNAME, _authUser);
        PlayerPrefs.SetString(PREF_NAME, _authUser + PREF_DELIMITER +
                                        _authToken + PREF_DELIMITER +
                                        _authPassMD5);
    }

    public void ClearUserLogin() {
        PlayerPrefs.DeleteKey(PREF_NAME);
    }

    public bool ReloadUserFromPrefs() {
        if(!PlayerPrefs.HasKey(PREF_NAME)) return false;

        string[] combined = PlayerPrefs.GetString(PREF_NAME).Split(PREF_DELIMITER);
        _authUser = combined[0];
        _authToken = combined[1];
        _authPassMD5 = combined[2];

        return true;
    }

    void Start() {
        _pendingRequests = new List<NodeRequest>();

        _authCodeB64 = EncodeToken(authCode);
        if(acceptSelfSignedCert) {
            EaseSecurity();
        }

        Initialize();

#if UNITY_EDITOR
        this.gameObject.AddComponent<CheatManager>();
#endif
    }

    protected virtual void Initialize() {} //Override in subclasses, not necessary to call base.Initialize()!

    ///////////////////////////////////////////////////////////////////////////////////////

    public NodePromise SendWhileLoggedIn(string partURL, string queries=null, object jsonData=null, string method="GET") {
        if (!isLoggedIn) return ForceReject();

        return SendAPI(partURL, queries, jsonData, method);
    }

    public NodePromise SendAPI(string partURL, string queries=null, object jsonData=null, string method="GET") {
        var req = new NodeRequest();
        req.isJSON = true;
        req.isAuth = true;

        var res = req.res = new NodeResponse();
        res.isJSON = true;
        res.req = req;

        //Support for Shortcut "HttpMethod::URL" url links:
        if(partURL.Contains("::")) {
            string[] partSplit = partURL.Split("::");
            method = partSplit[0];
            partURL = partSplit[1];
        }

        return Send(urlCurrent + partURL, queries, jsonData, method, req);
    }

    public IPromise<NodeResponse> Send(string fullURL, string queries=null, object jsonData =null, string method="GET", NodeRequest req=null) {
        req = req!=null ? req : new NodeRequest();
        req.promise = new Promise<NodeResponse>(); //Prepare the promise:
        var res = req.res!=null ? req.res : new NodeResponse();

        req.fullURL = fullURL;
        req.queries = queries;
        req.method = method;

        if(jsonData!=null) {
            if(jsonData is string) req.data = (string) jsonData;
            else req.data = JsonConvert.SerializeObject(jsonData);

            req.isJSON = true;
        }

        //Bind the Request and Response to eachother:
        res.req = req;
        req.res = res;

#if UNITY_EDITOR
        req.stackTrace = Environment.StackTrace;
#endif

        _pendingRequests.Add(req);

        //If it's not busy on a current Request, process this one immediately
        if (_currentRequest==null) {
            StartCoroutine(__Send()); //Start the WWW request:
        }

        return req.promise;
    }

    private IEnumerator __Send() {
        while(_pendingRequests.Count>0) {
            NodeRequest req = _currentRequest = _pendingRequests.Shift();

            yield return new WaitForEndOfFrame();

            var www = CreateWWWRequest(req);
            
            if (www == null) yield break;
            
            yield return www.Send();

            VerifyResponse(www, req);
        }
        
        _currentRequest = null;
    }

    private void VerifyResponse(UnityWebRequest www, NodeRequest req) {
        var promise = req.promise;
        var res = req.res;
        string text = www.downloadHandler.text;
        
        if (www.error.Exists()) {
            JSONNode errJSON = new JSONNode();
            if (text.Exists() && text.StartsWith("{")) {
                errJSON = JSON.Parse(text);
            }

            NotifyError(req, www.error, text, errJSON);
            return;
        }

        if (!text.Exists()) {
            NotifyError(req, "Server did not respond any 'text' data.");
            return;
        }

        res.text = text;

        if (!text.StartsWith("{") && !text.StartsWith("[")) {
            NotifyError(req, "Response does not contain JSON data!", text);
            return;
        }

        if (!res.isJSON) {
            promise.Resolve(res);
            return;
        }

        JSONNode json;

        try {
            json = JSON.Parse(text);
        } catch (Exception err) {
            NotifyError(req, "Malformed JSON: " + err.Message);
            return;
        }

        res.json = json;

        promise.Resolve(res);
    }

    ///////////////////////////////////////////////////////////////////////////////////////

    private UnityWebRequest CreateWWWRequest(NodeRequest req) {
        var downloader = new DownloadHandlerBuffer();
        var uploader = !req.data.Exists() ? null : new UploadHandlerRaw(req.data.ToUTF8());
        var www = new UnityWebRequest(); //req.fullURL, req.method.ToUpper(), hDownload, hUpload);
        
        if (req.isJSON) {
            www.SetRequestHeader("Content-Type", "application/json");
        }

        if (req.isAuth) {
            www.SetRequestHeader("Authorization", _authCodeB64);
        }

        www.url = req.fullURL;
        www.method = req.method.ToUpper();
        www.downloadHandler = downloader;
        www.uploadHandler = uploader;

        req.www = www;

        return www;
    }

    private static void NotifyError(NodeRequest req, string errMessage, string text=null, JSONNode json=null) {
        //traceError("NodeJSManager - Error loading from URL: " + req.fullURL);
        var err = new NodeError(req.fullURL + " : " + errMessage);
        err.json = json;
        err.text = text;
        err.request = req;
        err.response = req.res;

#if UNITY_EDITOR
        if(err.Message.Contains("Generic")) {
            traceError("Error Response: " + req.www.downloadHandler.text +
                "\n==========\n" + req.GetStackTrace() + "\n==========\n");
        } else {
            traceError(err.Message + "\n" + req.GetStackTrace());
        }
#endif

        req.promise.Reject(err);
    }

    public static string EncodeToken(params string[] parts) {
        return parts.Join(AUTH_DELIMITER).ToBase64();
    }

    public static string GetErrorMessage(Exception err) {
        string errMessage = err.Message;
        if (err is NodeError) {
            var nodeErr = ((NodeError)err);
            errMessage = nodeErr.json["error"];
            if(errMessage==null) errMessage = nodeErr.text;
            if(errMessage==null) errMessage = err.Message;
        }

        return errMessage;
    }

    public NodePromise ForceReject(string err=null) {
        if(err==null) {
            if(!_isLoggedIn) {
                err = "Couldn't call the API, possibly can't reach the API?";
            } else {
                err = "Unknown Error, Force-Rejected.";
            }
        }

        var promise = new Promise<NodeResponse>();
        this.Wait(-1, () => promise.Reject(new Exception(err)));
        return promise;
    }

    ///////////////////////////////////////////////////////////////////////////////////////
    // PIERRE: may not need these after ssl cert is correctly set:

    private void EaseSecurity() {
        _tempCertCallback = ServicePointManager.ServerCertificateValidationCallback;

        ServicePointManager.ServerCertificateValidationCallback =
            delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
    }

    private void HardenSecurity() {
        if (_tempCertCallback == null) return;
        ServicePointManager.ServerCertificateValidationCallback = _tempCertCallback;
    }

    ///////////////////////////////////////////////////////////////////////////////////////
}

namespace NodeJS {
    public class NodeError : Exception {
        public JSONNode json;
        public string text;
        public NodeRequest request;
        public NodeResponse response;

        public NodeError(string message) : base(message) { }
    }

    public class NodeRequest {
        public NodeResponse res;
        public Promise<NodeResponse> promise;
        public UnityWebRequest www;
        public string fullURL;
        public string queries;
        public string data;
        public string method;
        public bool isJSON = true;
        public bool isAuth = false;
        public string stackTrace;

        public string GetStackTrace() {
            string s = stackTrace.Replace("\\" , "/");

            s = s.Replace(Application.dataPath, "...");
            s = s.Replace("   at ", "-");
            s = s.Replace("at Assets", "...");
            s = s.Replace("UnityEngine.", "");
            s = s.Replace("UnityEditor.", "");
            s = s.Replace("Experimental.UIElements.", "");
            s = s.Replace("C:/buildslave/unity/build/artifacts/generated/common/runtime/", "");
            s = s.Replace("C:/buildslave/unity/build/artifacts/generated/common/", "");
            s = s.Replace("C:/buildslave/unity/build/Editor/Mono/", "");
            s = s.Replace("C:/buildslave/unity/build/Runtime/", "");
            s = s.Replace("/Users/builduser/buildslave/mono/build/mcs/class/corlib/", "");

            var split = s.Split("\n");
            var split2 = split.Slice(1);
            return split2.Join("\n");
        }
    }

    public class NodeResponse {
        public NodeRequest req;
        public JSONNode json;
        public JSONNode temp;
        public string text;
        public bool isJSON = true;

        public string pretty { get { return json.ToString("  "); } }

        public JSONNode this[string aKey] {
            get { return json["data"][aKey]; }
        }
    }
}
