namespace Tickets.Services
{
    public class LogService : ILogService
    {
        private readonly string Path = "";

        public LogService(string path)
        {
            Path = path;
        }

        public void Add(string sLog)
        {
            CreateDirectory();
            StreamWriter sw = new StreamWriter(Path + "/" + GetFileName(), true);
            sw.Write(DateTime.Now + " - " + sLog + Environment.NewLine);
            sw.Close();

        }

        private string GetFileName()
        {
            return "log_" + DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + ".txt"; 
        }

        private void CreateDirectory()
        {
            try
            {
                if (!Directory.Exists(Path))
                {
                    Directory.CreateDirectory(Path);
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
