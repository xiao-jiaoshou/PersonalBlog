using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using PersonalBlog.Models;

namespace PersonalBlog.ViewModels
{
    public class NotesViewModel : INotifyPropertyChanged
    {
        private readonly NotesModel _model = new NotesModel();
        private string _selectedCategory;

        public ObservableCollection<CategorySummary> Summaries => _model.Summaries;
        public ObservableCollection<NoteItem> RecentNotes => _model.RecentNotes;

        // 用于在视图中绑定并显示筛选后的集合
        public ICollectionView RecentNotesView { get; }

        public ICommand SelectCategoryCommand { get; }

        public NotesViewModel()
        {
            // 四大分类摘要（示例数据，按需替换）
            Summaries.Add(new CategorySummary { IconGlyph = "\uE8A9", Title = "学习笔记", Count = 45 });
            Summaries.Add(new CategorySummary { IconGlyph = "\uE70B", Title = "代码片段", Count = 128 });
            Summaries.Add(new CategorySummary { IconGlyph = "\uE8C8", Title = "待办事项", Count = 12 });
            Summaries.Add(new CategorySummary { IconGlyph = "\uE946", Title = "灵感记录", Count = 36 });

            // 最近笔记（示例数据）
            RecentNotes.Add(new NoteItem
            {
                Title = "C# 异步编程最佳实践",
                Summary = "async 和 await 的正确使用方式，避免常见的异步编程陷阱...",
                Category = "学习笔记",
                Tag = "C#",
                Date = new DateTime(2024,1,14),
                Url = ""
            });

            RecentNotes.Add(new NoteItem
            {
                Title = "Docker 常用命令速查",
                Summary = "容器、镜像、网络的常用操作命令整理...",
                Category = "代码片段",
                Tag = "Docker",
                Date = new DateTime(2024,1,12),
                Url = ""
            });

            RecentNotes.Add(new NoteItem
            {
                Title = "设计模式：单例模式的多种实现",
                Summary = "饿汉式、懒汉式、双重检查锁定的优缺点分析...",
                Category = "学习笔记",
                Tag = "设计模式",
                Date = new DateTime(2024,1,8),
                Url = ""
            });

            RecentNotes.Add(new NoteItem
            {
                Title = "Git 分支管理策略",
                Summary = "Git Flow 和 GitHub Flow 的工作流程对比...",
                Category = "学习笔记",
                Tag = "Git",
                Date = new DateTime(2024,1,5),
                Url = ""
            });

            RecentNotes.Add(new NoteItem
            {
                Title = "SQL 性能优化技巧",
                Summary = "索引优化、查询优化的实践经验总结...",
                Category = "代码片段",
                Tag = "SQL",
                Date = new DateTime(2024,1,2),
                Url = ""
            });

            // 创建集合视图并设置筛选回调
            RecentNotesView = CollectionViewSource.GetDefaultView(_model.RecentNotes);
            RecentNotesView.Filter = FilterBySelectedCategory;

            // 命令：选择某个分类（参数为分类 Title）
            SelectCategoryCommand = new RelayCommand(p =>
            {
                // 传 null 或空字符串 表示显示全部
                SelectedCategory = p?.ToString();
            });

            // 可选：初始不筛选（显示全部）或默认选中第一个分类
            // SelectedCategory = Summaries.FirstOrDefault()?.Title;
        }

        private bool FilterBySelectedCategory(object item)
        {
            if (string.IsNullOrEmpty(SelectedCategory))
                return true; // 未选分类则显示全部

            if (item is NoteItem note)
                return string.Equals(note.Category, SelectedCategory, StringComparison.Ordinal);

            return true;
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory == value) return;
                _selectedCategory = value;
                OnPropertyChanged();
                RecentNotesView.Refresh();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}