// ViewModels/LinksViewModel.cs
using PersonalBlog.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PersonalBlog.ViewModels
{
    public class LinksViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<LinkCategory> Categories { get; set; } = new ObservableCollection<LinkCategory>();

        public LinksViewModel()
        {
            LoadData();
        }

        private void LoadData()
        {
            Categories.Add(new LinkCategory
            {
                Title = "技术博客",
                Items = new List<LinkItem>
                {
                    new LinkItem { Icon = "\uE774", Title = "博客园", Description = "开发者的网上家园", Url = "https://cnblogs.com" },
                    new LinkItem { Icon = "\uE774", Title = "CSDN", Description = "专业开发者社区", Url = "https://csdn.net" },
                    new LinkItem { Icon = "\uE774", Title = "掘金", Description = "帮助开发者成长的社区", Url = "https://juejin.cn" }
                }
            });

            Categories.Add(new LinkCategory
            {
                Title = "个人博客",
                Items = new List<LinkItem>
                {
                    new LinkItem { Icon = "\uE77B", Title = "老张的博客", Description = ".NET 技术专家", Url = "https://example.com" },
                    new LinkItem { Icon = "\uE77B", Title = "前端小站", Description = "Vue/React 技术分享", Url = "https://example.com" }
                    // 只有2个，第三列自动空出
                }
            });

            Categories.Add(new LinkCategory
            {
                Title = "开源项目",
                Items = new List<LinkItem>
                {
                    new LinkItem { Icon = "\uE715", Title = "Awesome .NET", Description = "精选 .NET 资源列表", Url = "https://github.com", ButtonText = "访问项目" }
                    // 只有1个，只占1格
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}