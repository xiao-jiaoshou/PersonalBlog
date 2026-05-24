using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalBlog.Models
{
    public class LinkItem
    {
        public string Icon { get; set; }        // Segoe UI Symbol 字符
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string ButtonText { get; set; } = "访问网站";
    }

    public class LinkCategory
    {
        public string Title { get; set; }         // "技术博客" / "个人博客" / "开源项目"
        public List<LinkItem> Items { get; set; } = new List<LinkItem>();
    }
}
