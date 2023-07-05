using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppUTH.Views.Menu
{
    public class PageMenuFlyoutMenuItem
    {
        public PageMenuFlyoutMenuItem()
        {
            TargetType = typeof(PageMenuFlyoutMenuItem);
        }
        public int Id { get; set; }
        public string Title { get; set; }

        public Type TargetType { get; set; }
    }
}