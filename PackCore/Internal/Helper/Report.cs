using IconPack.Model.Progress;
using IconPack.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IconPack.Internal.Helper
{
    internal class Report
    {
        public static bool ServerProgress(string serverProc, string name, Action<ICloningProgress> progress)
        {
            //Counting objects:   0% (1/274)
            string[] progresses = serverProc.Split(new char[] { '(', ')', '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (progresses.Length <= 2)
                return true;

            double.TryParse(progresses[1], out double current);
            double.TryParse(progresses[2], out double total);
            if (total <= 0)
                return true;
            double estimateProgress = current / total;

            if (serverProc.StartsWith("Counting"))
                progress.Invoke(new ACounting(estimateProgress, name));
            else if (serverProc.StartsWith("Compressing"))
                progress.Invoke(new BCompressing(estimateProgress, name));

            return true;
        }

        public static bool TransferProgress(string name, LibGit2Sharp.TransferProgress transfer, Action<ICloningProgress>? progress)
        {
            if (transfer.ReceivedObjects < 1)
                return true;
            double estimate = Convert.ToDouble(transfer.ReceivedObjects.ToString()) / Convert.ToDouble(transfer.TotalObjects.ToString());
            progress?.Invoke(new CTransfer(estimate, name));
            return true;
        }

        public static void CheckoutProgress(string name, Action<ICloningProgress>? progress, string path, int complete, int total)
        {
            double estimate = Convert.ToDouble(complete.ToString()) / Convert.ToDouble(total.ToString());
            progress?.Invoke(new DCheckingOut(estimate, name));
        }

    }
}
