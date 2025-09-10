using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using SFB;

public class RuntimeFolderSelector
{

    public static string SelectFolder()
    {
        var paths = StandaloneFileBrowser.OpenFolderPanel("Select Folder", "", true);
        if (paths.Length > 0)
        {
            return paths[0];
        }
        return "";
    }
}
