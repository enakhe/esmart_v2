#nullable disable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Domain.ViewModels.StoreKepping
{
    public class MenuItems
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string ImagePath { get; set; }
        public string Description { get; set; }
    }

    public class MenuCategories
    {
        public string Name { get; set; }
        public ObservableCollection<MenuItems> Items { get; set; }
    }

    public class ServiceAreas
    {
        public string Name { get; set; }
        public ObservableCollection<MenuCategories> Categories { get; set; }
    }
}
