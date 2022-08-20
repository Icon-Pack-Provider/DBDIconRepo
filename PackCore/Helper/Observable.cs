using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IconPack.Helper
{
    public class Observable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
		/// Set a property into container and alert property has been changed
		/// If container and value is the same it will return false as nothing has changed
		/// Else it will return true as the container has been updated
		/// </summary>
		/// <typeparam name="T">Anything</typeparam>
		/// <param name="storage">A container property</param>
		/// <param name="value">A value that will be set into container</param>
		/// <param name="propertyName">Leave it blank and it will use called property</param>
		/// <returns></returns>
		public virtual bool Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(storage, value))
            {
                storage = value;
                OnPropertyChanged(propertyName);
                return true;
            }
            return false;
        }

        public void OnPropertyChanged(string propertyName) 
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
