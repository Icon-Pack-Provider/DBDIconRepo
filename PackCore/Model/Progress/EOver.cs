namespace IconPack.Model.Progress;

internal class EOver : ICloningProgress
{
    public EOver(string repoName) { RepositoryName = repoName; }
    public double Percent => -1;

    string _repo = string.Empty;
    public string RepositoryName
    {
        get => _repo;
        private set => _repo = value;
    }
}
