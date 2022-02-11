using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using FileInfoDef;

/// <summary>
/// 사용 조건
/// ./AssetBundles 폴더가 존재해야함.
/// ./Assets/AssetBundleResource 폴더가 존재해야함.
/// 
/// 동작 설명
/// ./Assets/AssetBundleResource 폴더의 모든 하위 폴더의 이름으로 에셋번들 생성, 해당 폴더에 있는 모든 오브젝트들은 해당 에셋에 포함
/// ./AssetBundles/AssetBundleInfo.json 생성된 에셋 번들들에 대한 정보 json파일 생성
/// 
/// 사용법
/// BuildAllAssetBundles 함수 호출
/// 
/// </summary>

public class AssetBundleBuilder : MonoBehaviour
{
    public class AssetBundleBaseInfo
    {
        //에셋에 할당할 번들 이름
        readonly public string bundleName;
        //에셋 절대경로
        readonly public string assetPath;

        public AssetBundleBaseInfo(string bundleName, string assetPath)
        {
            this.bundleName = bundleName;
            this.assetPath = assetPath;
        }
    }

    //버전 정보 (나중에 인자로 뺼 예정)
    private static string resourceVersion = "1110";


    static void GetFileList(string path, ref List<AssetBundleBaseInfo> filePathList)
    {
        foreach(var directory in Directory.GetDirectories(path))
        {
            foreach(var filePath in Directory.GetFiles(directory))
            {
                FileInfo fileInfo = new FileInfo(filePath);

                if (fileInfo == null) continue;

                if (fileInfo.Extension == ".meta") continue;

                //경로 규격에 맞게 수정
                string finalFilePath = filePath.Replace("\\", "/");
                string projectDirectory = Directory.GetCurrentDirectory().Replace("\\", "/");

                //절대 경로를 상대경로로 수정
                finalFilePath = finalFilePath.Replace(projectDirectory + "/", "");

                filePathList.Add(new AssetBundleBaseInfo(
                    Path.GetFileNameWithoutExtension(directory), finalFilePath));
                
            }

            //하위 폴더들 재귀적으로 탐색
            GetFileList(directory, ref filePathList);
        }
    }

    //에셋에 번들 이름 작성
    static void Reimport(AssetImporter importer, string bundleName)
    {
        if (string.Equals(importer?.assetBundleName, bundleName))
            return;

        importer?.SetAssetBundleNameAndVariant(bundleName, importer.assetBundleVariant);
    }

    //생성된 번들들에 대한 정보 json파일 생성
    static void MakeAssetBundleInfo(string bundleFolder, string saveTargetPath)
    {
        var manifestFiles = Directory.GetFiles(bundleFolder, "*.manifest");
        List<AssetBundleInfo> bundleInfoList = new List<AssetBundleInfo>();
        foreach (var manifestFile in manifestFiles)
        {
            //manifestFile 이름에서 확장자때는걸로 에셋번들 이름 가져오기
            var assetName = Path.GetFileNameWithoutExtension(manifestFile);

            string assetLocalPath = bundleFolder + "/" + assetName;
            
            BuildPipeline.GetCRCForAssetBundle(assetLocalPath, out uint crcForAssetBundle);
            var fileInfo = new FileInfo(assetLocalPath);
            

            bundleInfoList.Add(new AssetBundleInfo(assetName, fileInfo.GetHashCode(), fileInfo.Length, crcForAssetBundle));

        }

        AssetBundleInfos bundleInfos = new AssetBundleInfos();
        bundleInfos.assetBundleInfos = bundleInfoList;
        
        string bundleInfoListJson = JsonUtility.ToJson(bundleInfos);


        //async로 변경 가능
        File.WriteAllText(saveTargetPath + "/AssetBundleInfo.json", bundleInfoListJson);
    }

    //리소스 버전 정보 파일 생성
    static void MakeResourceVersionInfo()
    {
        ResourceVersionInfo resourceVersionInfo = new ResourceVersionInfo();
        resourceVersionInfo.version = resourceVersion;

        string resourceVersionInfoJson = JsonUtility.ToJson(resourceVersionInfo);

        File.WriteAllText(Directory.GetCurrentDirectory() + "/AssetBundles/" + ConstValue.resourceVersionFileName, resourceVersionInfoJson);
    }

    [MenuItem("Bundles/Build All AssetBundles")]
    static void BuildAllAssetBundles()
    {
        List<AssetBundleBaseInfo> filePathList = new List<AssetBundleBaseInfo>();
        GetFileList(Application.dataPath + "/AssetBundleResource", ref filePathList);
        filePathList.ForEach(x => Reimport(AssetImporter.GetAtPath(x.assetPath), x.bundleName));

        string savePath = "AssetBundles/" + resourceVersion + "/AssetBundles";
        ExtentionFunc.CheckAndCreateDirectory(Directory.GetCurrentDirectory().Replace("\\", "/") + "/" + savePath);
        
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(savePath, BuildAssetBundleOptions.None, BuildTarget.Android);
        MakeAssetBundleInfo(savePath, Directory.GetCurrentDirectory() + "/AssetBundles/" + resourceVersion + "/AssetBundles");
        MakeResourceVersionInfo();
    }
}
