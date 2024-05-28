using excellent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class ConfigTool
{
    [MenuItem("Tools/导出配置 &#c")]
    public static void ExportConfig()
    {
        List<Type> types = new List<Type>();//初始化了一个types的列表，用于存储从特定程序集中获取的类型

        foreach (var type in Assembly.Load("Assembly-CSharp-Editor").GetTypes())//加载了名为"Assembly-CSharp-Editor"的程序集，并遍历该程序集中的所有类型
        {
            if (type.Namespace == "ConfigDefinition")//筛选特定命名空间的类型
            {
                types.Add(type);
            }
        }
        //Excellent库进行导出
        Excellent.Go(new ExportInfo()
        {
            Namespace = "Config",//导出的命名空间设置为Config
            ConfigDefinitions = types.ToArray(),//程序集中筛选出的类型数组
            ExcelDirectory = Application.dataPath + "/../design/config",//Excel文件的导出目录
            SerializeDirectory = Application.dataPath + "/BundleAssets/Config",//序列化数据的导出目录
            CodeDirectory = Application.dataPath + "/Scripts/HotUpdate/Config/Code",//代码文件的导出目录
            WriteExcel = false,//是否写入Excel文件这里设置为false
            WithUnity = true,//是否与Unity一起使用，这里设置为true
            //BundleOffset = BundleLoader.BundleOffset,
            OnLog = OnLog,//一个日志回调函数，当发生某些日志事件时会被调用
        });
        AssetDatabase.Refresh();
        Debug.Log("导出配置完毕");
    }

    [MenuItem("Tools/更新配置结构 &#v")]
    public static void WriteAndExportConfig()
    {
        Directory.Delete(Application.dataPath + "/Scripts/HotUpdate/Config/Code", true);

        List<Type> types = new List<Type>();
        foreach (var type in Assembly.Load("Assembly-CSharp-Editor").GetTypes())
        {
            if (type.Namespace == "ConfigDefinition")
            {
                types.Add(type);
            }
        }

        Excellent.Go(new ExportInfo()
        {
            Namespace = "Config",
            ConfigDefinitions = types.ToArray(),
            ExcelDirectory = Application.dataPath + "/../design/config",
            SerializeDirectory = Application.dataPath + "/BundleAssets/Config",
            CodeDirectory = Application.dataPath + "/Scripts/HotUpdate/Config/Code",
            WriteExcel = true,
            WithUnity = true,
            //BundleOffset = BundleLoader.BundleOffset,
            OnLog = OnLog,
        });
        AssetDatabase.Refresh();
        Debug.Log("更新配置结构，并且导出成功");
    }

    private static void OnLog(string message)
    {
        Debug.Log(message);
    }
}