namespace CnSharp.VisualStudio.Extensions.Commands
{
    public interface ICommand
    {
        void Execute(object arg = null);
    }
}