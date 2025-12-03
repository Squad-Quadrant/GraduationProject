namespace Core.Log
{
    public interface ILogger
    {
        void Debug(string message, bool format = true);

        void Info(string message, bool format = true);

        void Warning(string message, bool format = true);

        void Error(string message, bool format = true);
    }
}
