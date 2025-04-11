using ESMART.Application.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ESMART.Presentation.Forms.RoomSetting.Room
{
    /// <summary>
    /// Interaction logic for UpdateRoomDialog.xaml
    /// </summary>
    public partial class UpdateRoomDialog : Window
    {
        private readonly IRoomRepository _roomRepository;
        private readonly Domain.Entities.RoomSettings.Room _room;
        public UpdateRoomDialog(IRoomRepository roomRepository, Domain.Entities.RoomSettings.Room room)
        {
            _roomRepository = roomRepository;
            _room = room;
            InitializeComponent();
        }
    }
}
