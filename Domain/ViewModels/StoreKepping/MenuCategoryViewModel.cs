#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using ESMART.Domain.Entities.StoreKeeping;
using System.Collections.ObjectModel;

namespace ESMART.Domain.ViewModels.StoreKepping
{
    public class MenuCategoryViewModel : ObservableObject
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IsAvailable { get; set; }
        public string ServiceArea { get; set; }
        public DateTime CreatedAt { get; set; }

        public ObservableCollection<MenuItem> MenuItems { get; set; }
    }
}
