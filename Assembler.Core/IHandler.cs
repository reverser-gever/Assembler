namespace Assembler.Core
{
    public interface IHandler
    {
        void Handle(IMessage message);
    }
}