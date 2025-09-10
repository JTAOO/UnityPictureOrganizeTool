using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;

public class PictureOrganizeTool : MonoBehaviour
{
    public enum ProcessItemStateEnum
    {
        NotProcess = 0,
        Copied = 1,
        Skipped = 2,
    }
    public class ProcessItemState
    {
        public int index;
        public string path;
        public ProcessItemStateEnum state;
    }
    public ShowProcessStatesUI showProcessStatesUI;
    public Color colorWhenProcessing;
    public Color colorWhenNotProcessing;
    public Camera mainCamera;
    public TMP_InputField sourceFolder;
    public TMP_InputField outputFoler;
    public TextMeshProUGUI processInfo;
    public string sourceFolderPath => sourceFolder.text;
    public string outputFolderPath => outputFoler.text;
    public bool includeSubfolders = true;

    private const string JPictureOrganizeToolInputFoler = "JPictureOrganizeToolInputFoler";
    private const string JPictureOrganizeToolOutputFoler = "JPictureOrganizeToolOutputFoler";


    private bool isInProcess = false;
    private string[] photoFiles = null;
    private List<ProcessItemState> photoStates = new List<ProcessItemState>();
    private int processedCount = 0;
    private int skippedCount = 0;
    private int nowIndex = 0;



    public void SelectSourceFolderPath()
    {
        sourceFolder.text = RuntimeFolderSelector.SelectFolder();
        PlayerPrefs.SetString(JPictureOrganizeToolInputFoler, sourceFolder.text);
        PlayerPrefs.Save();
    }
    public void SelectOutputFolderPath()
    {
        outputFoler.text = RuntimeFolderSelector.SelectFolder();
        PlayerPrefs.SetString(JPictureOrganizeToolOutputFoler, outputFoler.text);
        PlayerPrefs.Save();
    }

    private void Start()
    {
        sourceFolder.text = PlayerPrefs.GetString(JPictureOrganizeToolInputFoler);
        outputFoler.text = PlayerPrefs.GetString(JPictureOrganizeToolOutputFoler);

        processInfo.text = "Not Processing";
        photoStates = new List<ProcessItemState>();
    }


    private void Update()
    {
        if (isInProcess)
        {
            Processing();

            processInfo.text = "processedCount: " + (processedCount) + " , skippedCount: " + skippedCount + " , Total: " + photoFiles.Length;
            mainCamera.backgroundColor = colorWhenProcessing;
            showProcessStatesUI.RefreshUI_FollowIndex(nowIndex, photoStates);
        }
        else
        {
            mainCamera.backgroundColor = colorWhenNotProcessing;
        }


    }

    private void Processing()
    {
        int maxProcessCountInOneUpte = 1;
        int processCounter = 0;

        for (int i = nowIndex; i < photoFiles.Length; i++)
        {
            string filePath = photoFiles[i];


            string extension = Path.GetExtension(filePath).ToLower();

            // 只处理常见图片和视频格式
            if (IsImageFile(extension) || IsVideoFile(extension))
            {
                try
                {
                    DateTime photoDate = GetPhotoDate(filePath);
                    string dateFolder = photoDate.ToString("yyyy/MMdd");
                    string typeFolder = extension.TrimStart('.').ToLower();

                    string targetFolder = Path.Combine(outputFolderPath, dateFolder, typeFolder);
                    // 创建输出目录（如果不存在）
                    if (!Directory.Exists(targetFolder))
                        Directory.CreateDirectory(targetFolder);

                    string targetPath = Path.Combine(targetFolder, Path.GetFileName(filePath));

                    // 处理文件冲突
                    targetPath = HandleFileConflict(filePath, targetPath);

                    // 复制文件（如果目标路径已更改）
                    if (!File.Exists(targetPath) || !CompareFileMD5(filePath, targetPath))
                    {
                        File.Copy(filePath, targetPath, true);
                        Debug.Log("Copy Complete: " + filePath);
                        photoStates[i].state = ProcessItemStateEnum.Copied;
                        processedCount++;
                    }
                    else
                    {
                        Debug.Log("Skipped: " + filePath);
                        photoStates[i].state = ProcessItemStateEnum.Skipped;
                        skippedCount++;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Error processing file {filePath}: {ex.Message}");
                }
            }

            nowIndex += 1;



            if (nowIndex >= photoFiles.Length)
            {
                isInProcess = false;
            }

            processCounter++;
            if (processCounter >= maxProcessCountInOneUpte)
            {
                break;
            }
        }

        if (!isInProcess)
        {
            Debug.Log($"Photo organization completed! Processed: {processedCount}, Skipped duplicates: {skippedCount}");
        }
    }
    public void OrganizePhotos()
    {
        if (isInProcess)
            return;

        isInProcess = true;


        if (string.IsNullOrEmpty(sourceFolderPath) || string.IsNullOrEmpty(outputFolderPath))
        {
            Debug.LogError("Source or output folder path is empty!");
            return;
        }

        if (!Directory.Exists(sourceFolderPath))
        {
            Debug.LogError("Source folder does not exist!");
            return;
        }

        try
        {
            // 创建输出目录（如果不存在）
            if (!Directory.Exists(outputFolderPath))
                Directory.CreateDirectory(outputFolderPath);

            // 获取所有照片文件
            photoFiles = Directory.GetFiles(
                sourceFolderPath,
                "*.*",
                includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            processedCount = 0;
            skippedCount = 0;
            nowIndex = 0;
            photoStates.Clear();
            for (int i = 0; i < photoFiles.Length; i++)
            {
                photoStates.Add(new ProcessItemState() { index = i, path = photoFiles[i], state = ProcessItemStateEnum.NotProcess });
            }

            showProcessStatesUI.ShowThis();

            Debug.Log($"Photo organization Started!  ");


        }
        catch (Exception ex)
        {
            Debug.LogError($"Error during photo organization: {ex.Message}");
        }
    }

    private static bool IsImageFile(string extension)
    {
        string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".arw", ".raw", ".cr2", ".nef" };
        return Array.IndexOf(imageExtensions, extension) >= 0;
    }

    private static bool IsVideoFile(string extension)
    {
        string[] videoExtensions = { ".mp4", ".mov", ".webm", ".wmv" };
        return Array.IndexOf(videoExtensions, extension) >= 0;
    }


    private DateTime GetPhotoDate(string filePath)
    {
        try
        {
            // 先尝试从EXIF获取拍摄时间
            // 这里简化处理，实际项目中可以使用第三方库如MetadataExtractor
            // 这里使用文件最后修改时间作为回退方案
            return File.GetLastWriteTime(filePath);
        }
        catch
        {
            return DateTime.Now;
        }
    }

    private string HandleFileConflict(string sourcePath, string targetPath)
    {
        if (!File.Exists(targetPath))
        {
            return targetPath;
        }

        // 如果MD5相同，返回原路径（表示可以跳过）
        if (CompareFileMD5(sourcePath, targetPath))
        {
            return targetPath;
        }

        // MD5不同，需要重命名
        string directory = Path.GetDirectoryName(targetPath);
        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(targetPath);
        string extension = Path.GetExtension(targetPath);

        int counter = 1;
        string newTargetPath;

        do
        {
            newTargetPath = Path.Combine(directory, $"{fileNameWithoutExt}({counter}){extension}");
            counter++;
        }
        while (File.Exists(newTargetPath));

        return newTargetPath;
    }

    private bool CompareFileMD5(string filePath1, string filePath2)
    {
        try
        {
            string hash1 = ComputeFileMD5(filePath1);
            string hash2 = ComputeFileMD5(filePath2);
            return hash1 == hash2;
        }
        catch
        {
            return false;
        }
    }

    private string ComputeFileMD5(string filePath)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hashBytes = md5.ComputeHash(stream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
