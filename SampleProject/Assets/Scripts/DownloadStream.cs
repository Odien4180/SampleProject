using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;
using UnityEngine.Networking;
using System;

//���� �ٿ�ε� ���� Ŭ����
public class DownloadInfo
{
    public string downloadUrl;
    public string localSaveUrl;
    public Action<AsyncOperation> onComplete;
    public Action onFailed;
}

public class DownloadStream : MonoBehaviour
{
    private List<DownloadInfo> jobList = new List<DownloadInfo>();
    private Action onStreamComplete = null;
    private int maxJobSize = -1;
    private int currentJobIndex = -1;

    private List<DownloadInfo> failedJobList = new List<DownloadInfo>();

    /// �ٿ�ε� ��Ʈ�� ��ü ����
    public DownloadStream SetDownloadStream(Action streamCompleteAction, params DownloadInfo[] downloadInfos)
    {
        jobList = downloadInfos.ToList();
        onStreamComplete = streamCompleteAction;
        maxJobSize = downloadInfos.Length;
        currentJobIndex = 0;

        return this;
    }

    /// �ٿ�ε� ����
    public void OnWork()
    {
        StartCoroutine(Download());
    }

    private IEnumerator Download()
    {
        foreach (var job in jobList)
        {
            ++currentJobIndex;

            var directory = Path.GetDirectoryName(job.localSaveUrl);
            if (string.IsNullOrEmpty(directory) && false == Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            UnityWebRequest uwr = new UnityWebRequest(job.downloadUrl, UnityWebRequest.kHttpVerbGET);
            uwr.downloadHandler = new DownloadHandlerFile(job.localSaveUrl);
            var request = uwr.SendWebRequest();

            if (job.onComplete != null)
                request.completed += job.onComplete;

            yield return request;

            //�ٿ�ε� ����
            if (request.webRequest.result == UnityWebRequest.Result.ConnectionError ||
                request.webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.webRequest.result);

                job.onFailed();
                //������ �� ����Ʈ�� �߰�
                failedJobList.Add(job);
            }

            uwr.downloadHandler.Dispose();
            uwr.Dispose();
        }

        onStreamComplete?.Invoke();
    }

}