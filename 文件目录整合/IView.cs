using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 文件目录整合
{
    public interface IView
    {
        System.Windows.Threading.Dispatcher ViewDispathcer { get; }

        void SetSnackBarMessage(string message);
    }
}
