using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RabbitImageView.entitys
{
    internal class ViewBaseInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropChanged([CallerMemberName]string propName = "")
        {   
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
