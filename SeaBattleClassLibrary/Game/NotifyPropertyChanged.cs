using System.ComponentModel;
using System.Runtime.Serialization;

namespace SeaBattleClassLibrary.Game
{
    [DataContractAttribute]
    public abstract class NotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
