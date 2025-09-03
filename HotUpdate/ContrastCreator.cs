#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

internal static class ContrastCreator {
    static readonly string ResPath =
        Path.GetDirectoryName(Application.dataPath) + "/AssetBundles/Windows";
    const string ServerResPath = @"E:\Visual Studio Files\C# Files\GameResServer\GameRes";

    static void CreateContrast() {
        StringBuilder sb = new();
        foreach (var path in Directory.GetFiles(ResPath).Where(x => !x.Contains('.'))) {
            sb.Append(Path.GetFileName(path)).Append(' ');
            sb.Append(new FileInfo(path).Length).Append(' ');

            using var md5 = MD5.Create();
            using var stream = File.OpenRead(path);
            byte[] hash = md5.ComputeHash(stream);
            string md5Hash = BitConverter.ToString(hash).Replace("-", "");
            sb.Append(md5Hash).AppendLine();
        }
        sb.Remove(sb.Length - 2, 2); //去除末尾换行
        File.WriteAllText(Path.Combine(ResPath, "contrast.txt"), sb.ToString());
    }
    [MenuItem("工具/上传AB包和对比文件到服务器")]
    public static void Upload() {
        CreateContrast();
        foreach (var path in Directory.GetFiles(ResPath).Where(x => !x.EndsWith(".manifest"))) {
            var targetPath = Path.Combine(ServerResPath, Path.GetFileName(path));
            File.Copy(path, targetPath, overwrite: true);
        }
    }
}
#endif