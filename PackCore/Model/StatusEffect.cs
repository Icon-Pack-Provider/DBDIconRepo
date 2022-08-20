using IconPack.Helper;

namespace IconPack.Model
{
    public partial class StatusEffect : Observable, IBasic
    {
        string file;
        public string File
        {
            get => file;
            set => Set(ref file, value);
        }

        //Power name
        string name;
        public string Name
        {
            get => name;
            set => Set(ref name, value);
        }

    }
}
