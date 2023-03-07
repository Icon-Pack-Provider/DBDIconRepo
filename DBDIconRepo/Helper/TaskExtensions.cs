using System;
using System.Threading.Tasks;

namespace DBDIconRepo.Helper;

public static class TaskExtensions
{
	public async static void Await(this Task task, Action? onComplete = null, Action<Exception>? onError = null)
	{
        try
        {
            await task.ConfigureAwait(false);
            onComplete?.Invoke();
        }
        catch (Exception e)
        {
            onError?.Invoke(e);
        }
    }

	public async static void Await(this Task task, bool configAwait = false, Action? onComplete = null, Action<Exception>? onError = null)
	{
		try
		{
			await task.ConfigureAwait(configAwait);
			onComplete?.Invoke();
		}
		catch (Exception e)
		{
			onError?.Invoke(e);
		}
	}
}