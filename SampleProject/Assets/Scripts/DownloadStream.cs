using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;
using System;
using System.IO;

//���� �ٿ�ε� ���� Ŭ����
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

    /// �ٿ�ε� ��Ʈ�� ��ü ����
    public DownloadStream SetDownloadStream(Action<DownloadStream> streamCompleteAction, params DownloadInfo[] downloadInfos)
    {
        jobList = downloadInfos.ToList();
        onStreamComplete = streamCompleteAction;
        maxJobSize = downloadInfos.Length;
        currentJobIndex = 0;

        return this;
    }

    /// �ٿ�ε� ����
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
            
            //�ٿ�ε� ����
            if (request.webRequest.result == UnityWebRequest.Result.ConnectionError ||
                request.webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.webRequest.result);

                job.onFailed();
                //������ �� ����Ʈ�� �߰�
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
