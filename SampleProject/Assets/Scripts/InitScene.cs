using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using FileInfoDef;
using UnityEngine.UI;
using TMPro;

public class InitScene : MonoBehaviour
{
    private string resourceVersionFileUrl;
    private string resourceVersionLocalFileUrl;
    private string localAssetSaveUrl;
    private string assetBundleInfoLocalUrl;

    private string downloadTargetResourceVersionUrl;

    public DownloadStream downloadStream;

    public ProgressBar progressBar;
    public TextMeshProUGUI noticeText;

    public Button googleLoginBtn;

    void Start()
    {
        resourceVersionFileUrl = ConstValue.awsRootUrl + ConstValue.resourceVersionFileName;

        resourceVersionLocalFileUrl = Application.persistentDataPath + "/" + ConstValue.resourceVersionFileName;

        localAssetSaveUrl = Application.persistentDataPath + "/AssetBundles/";

        assetBundleInfoLocalUrl = localAssetSaveUrl + ConstValue.assetBundleInfoFileName;

        if (googleLoginBtn)
        {
            googleLoginBtn.onClick.AddListener(() => AuthManager.Instance.Login(AuthType.Google));
            googleLoginBtn.gameObject.SetActive(false);
        }


        StartCoroutine(StartAssetBundleDownload());
    }

    //���ҽ� �������� ���� �ٿ�ε� �Ϸ� �� �ش� ���ҽ� ���� ����� AssetBundleInfo.json�� �޴´�.
    private IEnumerator StartAssetBundleDownload()
    {
        noticeText.gameObject.SetActive(true);
        noticeText.SetText("���ҽ� ���� Ȯ�� ��...");
        progressBar.gameObject.SetActive(false);

        yield return StartCoroutine(DownloadResourceVersionInfo());

        noticeText.SetText("���ҽ� ���� �˻���...");

        var downloadAssetInfo = new DownloadInfo();
        downloadAssetInfo.downloadUrl = downloadTargetResourceVersionUrl + "/AssetBundles/" + ConstValue.assetBundleInfoFileName;
        downloadAssetInfo.localSaveUrl = assetBundleInfoLocalUrl;

        downloadStream.SetDownloadStream(null, downloadAssetInfo);

        yield return downloadStream.OnWork();

        noticeText.SetText("���ҽ� ���� �ٿ�ε� ��...");
        progressBar.gameObject.SetActive(true);

        yield return StartCoroutine(CrcCheckAndDownloadAssetBundle());

        noticeText.SetText("���ҽ� �ٿ�ε� �Ϸ�");
        progressBar.gameObject.SetActive(false);

        googleLoginBtn.gameObject.SetActive(true);
    }

    //���ҽ� �������� ���� �ٿ�ε� �� �ٿ���� ���ҽ� url(resourceVersionDirectory) ����
    private IEnumerator DownloadResourceVersionInfo()
    {
        var downloadAssetInfo = new DownloadInfo();
        downloadAssetInfo.downloadUrl = resourceVersionFileUrl;
        downloadAssetInfo.localSaveUrl = resourceVersionLocalFileUrl;

        downloadStream.SetDownloadStream(null, downloadAssetInfo);

        yield return downloadStream.OnWork();

        string[] bundleInfoText = File.ReadAllLines(resourceVersionLocalFileUrl);
        var resourceVersionInfo = JsonUtility.FromJson<ResourceVersionInfo>(bundleInfoText.AddStringArray());
        downloadTargetResourceVersionUrl = ConstValue.awsRootUrl + resourceVersionInfo.version;
    }

    //�ٿ���� AssetBundleInfo.json�� �ε��ؼ� ���� �������ִ� ���µ�� crc�� ��
    //�޶󠺴ٸ� ���¿� �������� �ִٴ� ��, ���� �ٿ�ε� ����
    private IEnumerator CrcCheckAndDownloadAssetBundle()
    {
        List<DownloadInfo> downloadInfos = new List<DownloadInfo>();
        string[] bundleInfoTextAllLine = File.ReadAllLines(assetBundleInfoLocalUrl);

        var bundleInfos = JsonUtility.FromJson<AssetBundleInfos>(bundleInfoTextAllLine.AddStringArray());

        if (progressBar) progressBar?.Init(0, bundleInfos.assetBundleInfos.Count, true);

        foreach(var bundleInfo in bundleInfos.assetBundleInfos)
        {
            FileInfo fileInfo = new FileInfo(localAssetSaveUrl + bundleInfo.bundleName);

            bool needDownload = true;

            //���� �����ϸ� crc����
            if (fileInfo.Exists)
            {
                var bundleLoadRequest = AssetBundle.LoadFromFileAsync(localAssetSaveUrl + bundleInfo.bundleName, bundleInfo.crc);

                yield return bundleLoadRequest;

                //crc ��ġ���� ���� �� (null) ���� �ٿ�ε�
                needDownload = bundleLoadRequest.assetBundle == null;
            }

            if (needDownload)
            {
                downloadInfos.Add(new DownloadInfo()
                {
                    downloadUrl = downloadTargetResourceVersionUrl + "/AssetBundles/" + bundleInfo.bundleName,
                    localSaveUrl = localAssetSaveUrl + bundleInfo.bundleName,
                    onComplete = x =>
                    {
                        if (progressBar) progressBar.Init(downloadStream.currentJobIndex, (float)downloadStream.maxJobSize, true);
                        
                        Debug.Log("Download Complete : " + bundleInfo.bundleName);
                    }
                });

                Debug.Log("Reservation Download : " + bundleInfo.bundleName);
            }
            else
            {
                //������ ���� �̹� ����Ǿ� ����
                Debug.Log("Already Exist : " + bundleInfo.bundleName);
            }
        }

        downloadStream.SetDownloadStream(x =>
        {
            Debug.Log("Download Stream Complete");
        }, downloadInfos.ToArray());

        yield return downloadStream.OnWork();
    }
}
