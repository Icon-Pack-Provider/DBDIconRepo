using IconPack.Model;

namespace IconRepository.ViewModel
{
    public class PackViewModel : Pack
    {
        public PackViewModel(Pack baseInfo)
        {
            this.Authors = baseInfo.Authors;
            this.ContentInfo = baseInfo.ContentInfo;

        }

        //TODO:Get list of contributors
        public string AuthorDisplay => Repository.Owner;
    }
}
