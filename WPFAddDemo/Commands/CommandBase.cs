using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WPFAddDemo.ViewModels
{
    internal class CommandBase : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (CanExcuteFunc == null)
            {
                return true;
            }
            return CanExcuteFunc(parameter);
        }

        public void Execute(object parameter)
        {
            if (ExcecuteAction == null)
            {
                return;
            }
            ExcecuteAction(parameter);
        }

        public Action<object> ExcecuteAction { get; set; }
        public Func<object, bool> CanExcuteFunc { get; set; }
    }
}
