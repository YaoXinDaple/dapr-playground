using System.Diagnostics;
using System.Management;

namespace sample.microservice.order
{
    public static class ProcessExtensions
    {
        //Get all processes that match the specified name

        public static string GetCommandLineArgs(this Process process)
        {
            if (process is null) throw new ArgumentNullException(nameof(process));

            try
            {
                using var searcher = new ManagementObjectSearcher(
                    $"SELECT COMMANDLINE FROM WIN32_PROCESS WHERE PROCESSID = {process.Id}"
                    );
                using var objects = searcher.Get();
                var @object = objects.Cast<ManagementBaseObject>().SingleOrDefault();
                return @object?["CommandLine"]?.ToString() ?? string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
