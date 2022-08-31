using IconPack.Helper;

namespace IconPack.Model.Icon
{
    public partial class Emblem : Observable, IBasic
    {
        string file;
        public string File
        {
            get => file;
            set => Set(ref file, value);
        }

        string name;
        public string Name
        {
            get => name;
            set => Set(ref name, value);
        }
    }
}
