using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;
using System;
using System.IO;

//파일 다운로드 정보 클래스
public class DownloadInfo
{
    public string downloadUrl;
    public string localSaveUrl;
    public Action<DownloadInfo> onComplete;
    public Action onFailed;
}

public class DownloadStream : MonoBehaviour
{
    private List<DownloadInfo> jobList = new List<DownloadInfo>();
    private Action<DownloadStream> onStreamComplete = null;
    public int maxJobSize = -1;
    public int currentJobIndex = -1;

    private List<DownloadInfo> failedJobList = new List<DownloadInfo>();

    /// 다운로드 스트림 객체 세팅
    public DownloadStream SetDownloadStream(Action<DownloadStream> streamCompleteAction, params DownloadInfo[] downloadInfos)
    {
        jobList = downloadInfos.ToList();
        onStreamComplete = streamCompleteAction;
        maxJobSize = downloadInfos.Length;
        currentJobIndex = 0;

        return this;
    }

    /// 다운로드 시작
    public Coroutine OnWork()
    {
        return StartCoroutine(Download());
    }

    private IEnumerator Download()
    {
        foreach (var job in jobList)
        {
            ++currentJobIndex;
            
            ExtentionFunc.CheckAndCreateDirectory(Path.GetFileNameWithoutExtension(job.localSaveUrl));

            UnityWebRequest uwr = new UnityWebRequest(job.downloadUrl, UnityWebRequest.kHttpVerbGET);
            uwr.downloadHandler = new DownloadHandlerFile(job.localSaveUrl);
            var request = uwr.SendWebRequest();

            yield return request;
            
            //다운로드 실패
            if (request.webRequest.result == UnityWebRequest.Result.ConnectionError ||
                request.webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.webRequest.result);

                job.onFailed();
                //실패한 잡 리스트에 추가
                failedJobList.Add(job);
            }
            else
            {
                job.onComplete?.Invoke(job);
            }

            uwr.downloadHandler.Dispose();
            uwr.Dispose();

            yield return new WaitForSeconds(5.0f);
        }

        onStreamComplete?.Invoke(this);
    }

}
