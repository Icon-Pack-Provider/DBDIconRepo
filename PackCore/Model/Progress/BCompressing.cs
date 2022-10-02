using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconPack.Model.Progress
{
    public class BCompressing : ICloningProgress
    {
        public BCompressing(double progress, string repo)
        {
            Percent = progress;
            RepositoryName = repo;
        }

        private double percent = 0;
        public double Percent
        {
            get => percent;
            private set => percent = value;
        }

        string repoName = string.Empty;
        public string RepositoryName
        {
            get => repoName;
            private set => repoName = value;
        }
    }
}
