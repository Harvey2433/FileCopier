using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

// 初始化统计变量
int filesCopied = 0;
int directoriesCreated = 0;
long totalSize = 0;
var stopwatch = new Stopwatch();

// 进度跟踪变量
int totalFiles = 0;
int processedFiles = 0;
var progressWatch = Stopwatch.StartNew();

// 错误统计
int wildcardPathCount = 0;
int successCount = 0;
int invalidPathCount = 0;
var errorLogs = new List<string>();

// 配置控制台
Console.Title = "文件复制工具 - 正在初始化 | by Maple Bamboo Team";
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("=== 文件复制程序准备就绪 ===");
Console.ResetColor();
Console.WriteLine("按回车键开始复制...");
while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }

// 文件列表检测
string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
string fileListPath = Path.Combine(appDirectory, "file.txt");

if (!File.Exists(fileListPath))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("错误：程序目录下未找到 file.txt");
    Console.ResetColor();
    Console.WriteLine("按下回车以退出程序");
    while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
    Environment.Exit(0);
    return 1;   
}

// 预计算总文件数
var sourcePaths = File.ReadAllLines(fileListPath)
    .Where(p => !string.IsNullOrWhiteSpace(p))
    .ToArray();

Console.Title = "正在扫描文件... | by Maple Bamboo Team";
totalFiles = CalculateTotalFiles(sourcePaths);
Console.Title = $"文件复制工具 | 总文件: {totalFiles} | by Maple Bamboo Team";

// 开始处理
stopwatch.Start();
foreach (var originPath in sourcePaths)
{
    try
    {
        if (!Path.IsPathRooted(originPath))
        {
            errorLogs.Add($"{originPath} :: 非绝对路径");
            invalidPathCount++;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"警告：跳过非绝对路径 [{originPath}]");
            Console.ResetColor();
            continue;
        }

        bool hasWildcard = ContainsWildcard(originPath);
        if (hasWildcard)
        {
            wildcardPathCount++;
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"正在展开通配符路径: {originPath}");
            Console.ResetColor();
        }

        foreach (var expandedPath in ExpandWildcardPath(originPath))
        {
            try
            {
                if (File.Exists(expandedPath))
                {
                    ProcessFile(expandedPath, appDirectory);
                    successCount++;
                    processedFiles++;
                    UpdateProgress();
                }
                else if (Directory.Exists(expandedPath))
                {
                    ProcessDirectory(expandedPath, appDirectory);
                }
                else
                {
                    invalidPathCount++;
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"警告：路径不存在 [{expandedPath}]");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                errorLogs.Add($"{expandedPath} :: {ex.Message.Replace(":", "꞉")}");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"操作失败: {expandedPath} - {ex.Message}");
                Console.ResetColor();
            }
        }
    }
    catch (Exception ex)
    {
        errorLogs.Add($"{originPath} :: {ex.Message.Replace(":", "꞉")}");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"错误：处理路径 [{originPath}] 时出错：{ex.Message}");
        Console.ResetColor();
    }
}

stopwatch.Stop();

// 输出统计信息
Console.Title = "文件复制工具 | by Maple Bamboo Team";
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("\n===== 复制结果 =====");
Console.ResetColor();
Console.WriteLine($"复制文件总数: {filesCopied}");
Console.WriteLine($"创建目录总数: {directoriesCreated}");
Console.WriteLine($"总耗时: {stopwatch.Elapsed:mm\\:ss\\.ff}");
Console.WriteLine($"总文件大小: {FormatSize(totalSize)}");

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("\n===== 处理统计 =====");
Console.ResetColor();
Console.WriteLine($"通配符路径数量: {wildcardPathCount}");
Console.WriteLine($"成功处理文件数: {successCount}");
Console.WriteLine($"无效路径数量: {invalidPathCount}");
Console.WriteLine($"失败操作统计: {errorLogs.Count}");

// 输出错误日志
if (errorLogs.Count > 0)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("\n===== 错误日志 =====");
    Console.WriteLine("以下错误发生在处理过程中：\n");

    int errorNumber = 1;
    foreach (var error in errorLogs)
    {
        var parts = error.Split(new[] { " :: " }, 2, StringSplitOptions.None);  
        var path = parts.Length > 0 ? parts[0].Replace("꞉", ":") : "未知路径";
        var reason = parts.Length > 1 ? parts[1] : "未知错误";

        Console.WriteLine($"[错误 #{errorNumber++}]");
        Console.WriteLine($"├ 完整路径: {path}");
        Console.WriteLine($"└ 错误原因: {reason}");
        Console.WriteLine(new string('─', 60));
    }
    Console.ResetColor();
    Console.WriteLine("按下回车以退出程序");
    while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
    Environment.Exit(0);
}

return 0;

#region Helper Methods
int CalculateTotalFiles(string[] paths)
{
    int count = 0;
    foreach (var path in paths)
    {
        if (!Path.IsPathRooted(path)) continue;

        try
        {
            foreach (var expandedPath in ExpandWildcardPath(path))
            {
                if (File.Exists(expandedPath)) count++;
                if (Directory.Exists(expandedPath))
                    count += Directory.GetFiles(expandedPath, "*", SearchOption.AllDirectories).Length;
            }
        }
        catch { /* 忽略无法访问的路径 */ }
    }
    return count;
}

void UpdateProgress()
{
    if (processedFiles % 10 != 0 && progressWatch.ElapsedMilliseconds < 1000) return;

    var elapsed = stopwatch.Elapsed;
    var filesPerSec = processedFiles / elapsed.TotalSeconds;
    var remaining = totalFiles - processedFiles;
    var eta = filesPerSec > 0 ? TimeSpan.FromSeconds(remaining / filesPerSec) : TimeSpan.Zero;

    Console.Title = string.Format(
        "文件复制工具 | 进度: {0:0.0}% | 已处理: {1}/{2} | 速度: {3:0}个/s | 剩余: {4:mm\\:ss} | by Maple Bamboo Team",
        (double)processedFiles / totalFiles * 100,
        processedFiles,
        totalFiles,
        filesPerSec,
        eta
    );

    progressWatch.Restart();
}

IEnumerable<string> ExpandWildcardPath(string path)
{
    var stack = new Stack<string>();
    stack.Push(Path.GetFullPath(path));

    while (stack.Count > 0)
    {
        var current = stack.Pop();

        if (ContainsWildcard(current))
        {
            string dir = Path.GetDirectoryName(current) ?? Directory.GetCurrentDirectory();
            string pattern = Path.GetFileName(current);

            if (!Directory.Exists(dir)) continue;

            foreach (var d in Directory.EnumerateDirectories(dir, pattern))
                stack.Push(Path.Combine(d, "*"));

            foreach (var f in Directory.EnumerateFiles(dir, pattern))
                yield return f;
        }
        else
        {
            if (File.Exists(current)) yield return current;
            if (Directory.Exists(current)) yield return current;
        }
    }
}

bool ContainsWildcard(string path) => path.IndexOfAny(new[] { '*', '?' }) >= 0;

void ProcessDirectory(string sourceDir, string baseDir)
{
    foreach (var file in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories))
    {
        ProcessFile(file, baseDir);
    }
}

void ProcessFile(string sourceFile, string baseDir)
{
    var (destPath, directoriesToCreate) = GetDestinationPath(sourceFile, baseDir);
    CreateDirectories(directoriesToCreate);
    CopyFile(sourceFile, destPath);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"已复制: {sourceFile}");
    Console.ResetColor();
}

(string DestPath, string[] Directories) GetDestinationPath(string sourceFile, string baseDir)
{
    string root = Path.GetPathRoot(sourceFile) ?? string.Empty;
    string relativePath = sourceFile[root.Length..];
    string destFile = Path.Combine(baseDir, relativePath);
    return (destFile, Path.GetDirectoryName(destFile)?.Split(Path.DirectorySeparatorChar) ?? Array.Empty<string>());
}

void CreateDirectories(string[] pathSegments)
{
    string currentPath = Path.GetPathRoot(Environment.CurrentDirectory) ?? string.Empty;
    foreach (var segment in pathSegments)
    {
        currentPath = Path.Combine(currentPath, segment);
        if (!Directory.Exists(currentPath))
        {
            Directory.CreateDirectory(currentPath);
            directoriesCreated++;
        }
    }
}

void CopyFile(string source, string destination)
{
    var info = new FileInfo(source);
    File.Copy(source, destination, true);
    filesCopied++;
    totalSize += info.Length;
}

string FormatSize(long bytes)
{
    string[] sizes = new[] { "B", "KB", "MB", "GB" };
    int order = 0;
    double size = bytes;
    while (size >= 1024 && order < sizes.Length - 1)
    {
        order++;
        size /= 1024;
    }
    return $"{size:0.##} {sizes[order]}";
}
#endregion
Console.WriteLine();