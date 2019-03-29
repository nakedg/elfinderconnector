using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ElFinder.Connector.Command
{
    public interface ICommand
    {
        ElFinder ElFinder { get; }
        string Name { get; }
        Task<ResponseWriter.IResponseWriter> Execute();
    }
}
