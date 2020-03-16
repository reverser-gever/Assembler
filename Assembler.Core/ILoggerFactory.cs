namespace Assembler.Core
{
    public interface ILoggerFactory
    {
        ILogger GetLogger(object obj);
    }
}