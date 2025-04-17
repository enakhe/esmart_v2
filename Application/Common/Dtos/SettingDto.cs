using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESMART.Application.Common.Dtos
{
    public class SettingDto
    {
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public string Value { get; set; }
        public string DataType { get; set; }
    }

}
