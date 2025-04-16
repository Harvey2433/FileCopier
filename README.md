# 文件复制工具

一个高效、可靠的文件复制命令行工具，支持通配符路径、实时进度跟踪和保留原目录结构等功能

![演示截图](https://img.picui.cn/free/2025/04/13/67fb2e7686690.png)

## 功能特性

- **智能路径处理**
  - 支持多级通配符 (`*`, `?`)
  - 自动展开目录结构
  - 保留源文件属性

- **实时监控**
  - 窗口标题显示进度百分比
  - 传输速度统计 (文件/秒)
  - 剩余时间预估 (ETA)

- **资源优化**
  	- 内存占用控制 (<200MB处理10万文件)
  - 分批处理机制
  - 自动垃圾回收

- **错误管理**
  - 结构化错误日志
  - 错误路径高亮显示

- **跨平台支持**
  - Windows 10/11 原生优化
  - 兼容Linux/macOS终端
  - 支持网络路径和本地路径

## 系统要求

- [.NET 7 Runtime](https://dotnet.microsoft.com/download/dotnet/7.0)
- Windows系统建议启用长路径支持：
  ```regedit
  HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\FileSystem
  LongPathsEnabled (DWORD) = 1

# 开始使用
## 准备文件列表
在file.txt中写入文件源路径,每一行为一个绝对路径,支持文件或目录通配符
## 编译并运行
- #### 克隆仓库
git clone https://github.com/yourrepo/file-copier.git

cd file-copier

### 编译发布版本
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true

### 运行程序
复制file.txt到输出目录
运行输出目录下的exe程序,按下回车即可开始运行

### 查看运行结果
示例:

=== 处理结果 ===

复制文件: 1,234

创建目录: 56

总大小: 4.78 GB

耗时: 00:05:23


==== 统计信息 ====

通配符路径: 3

无效路径: 12

错误总数: 5

