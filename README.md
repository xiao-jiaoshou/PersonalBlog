# 个人博客系统 - WPF

一个基于 .NET Framework 4.8 开发的简约风格个人博客桌面应用，采用黑金双色主题。

## 项目结构

```
PersonalBlog/
├── App.xaml                    # 应用程序入口
├── App.xaml.cs
├── App.config                  # 配置文件
├── MainWindow.xaml             # 主窗口
├── MainWindow.xaml.cs
├── PersonalBlog.csproj         # 项目文件
├── Themes/
│   └── DarkGoldTheme.xaml      # 黑金主题资源字典
├── ViewModels/
│   ├── MainViewModel.cs        # 主视图模型
│   └── RelayCommand.cs         # 命令基类
├── Views/
│   ├── ProfileView.xaml        # 个人简介视图
│   ├── ArticlesView.xaml       # 文章分享视图
│   ├── NotesView.xaml          # 个人笔记视图
│   ├── LinksView.xaml          # 友情链接视图
│   └── GuestbookView.xaml      # 留言板视图
└── Properties/
    ├── AssemblyInfo.cs
    ├── Resources.resx
    └── Settings.settings
```

## 功能特性

### 界面设计
- **左右分栏布局**：左侧为导航侧边栏，右侧为内容展示区
- **黑金双色主题**：简约优雅的深色背景搭配金色点缀
- **响应式导航**：点击侧边栏按钮切换不同内容页面

### 内容模块
1. **个人简介**
   - 头像和基本信息展示
   - 技术栈标签
   - 个人介绍
   - 联系方式

2. **文章分享**
   - 文章分类标签（全部、技术、随笔、教程、生活）
   - 文章列表展示
   - 文章标题、摘要、发布时间

3. **个人笔记**
   - 笔记分类统计卡片
   - 笔记列表（学习笔记、代码片段、待办事项、灵感记录）

4. **友情链接**
   - 技术博客链接
   - 个人博客链接
   - 开源项目链接

5. **留言板**
   - 留言表单（昵称、邮箱、留言内容）
   - 留言列表展示
   - 博主回复功能

## 主题配色

| 颜色名称 | 色值 | 用途 |
|---------|------|------|
| 背景深黑 | #0A0A0A | 主背景色 |
| 背景卡片 | #141414 | 卡片背景 |
| 金色主色 | #D4AF37 | 强调色、选中状态 |
| 金色浅色 | #F4D03F | 悬停状态 |
| 金色深色 | #B8860B | 按下状态 |
| 文字主色 | #FFFFFF | 主要文字 |
| 文字次要 | #B0B0B0 | 次要文字 |
| 文字淡化 | #707070 | 辅助文字 |

## 运行环境

- **.NET Framework**: 4.8
- **开发工具**: Visual Studio 2019 或更高版本
- **操作系统**: Windows 7 SP1 或更高版本

## 使用方法

1. 使用 Visual Studio 打开 `PersonalBlog.csproj` 文件
2. 按 F5 或点击"开始"按钮运行项目
3. 点击左侧侧边栏按钮切换不同内容页面

## 自定义修改

### 修改主题颜色
编辑 `Themes/DarkGoldTheme.xaml` 文件中的颜色资源：

```xml
<SolidColorBrush x:Key="GoldPrimary" Color="#D4AF37"/>
```

### 添加新页面
1. 在 `Views` 文件夹中创建新的 UserControl
2. 在 `MainViewModel.cs` 中添加新的导航逻辑
3. 在 `MainWindow.xaml` 中添加新的导航按钮

## 截图预览

应用采用黑金配色，界面简洁优雅：
- 左侧深色侧边栏带有金色边框头像
- 导航按钮悬停时显示金色
- 选中项带有金色左边框
- 内容区域使用卡片式布局

## 许可证

MIT License
