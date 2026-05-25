using System;
using System.Collections.ObjectModel;

namespace PersonalBlog.Models
{
    public class NoteItem
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Category { get; set; }    // e.g. "学习笔记", "代码片段"
        public string Tag { get; set; }         // e.g. "C#", "Docker"
        public DateTime Date { get; set; }
        public string Url { get; set; }
    }

    public class CategorySummary
    {
        public string IconGlyph { get; set; }   // 用于 XAML 中显示图标的字体字形
        public string Title { get; set; }       // 分类标题
        public int Count { get; set; }          // 条目数量
    }

    public class NotesModel
    {
        public ObservableCollection<CategorySummary> Summaries { get; } = new ObservableCollection<CategorySummary>();
        public ObservableCollection<NoteItem> RecentNotes { get; } = new ObservableCollection<NoteItem>();
    }
}