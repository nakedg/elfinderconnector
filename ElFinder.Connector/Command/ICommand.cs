using System;
using System.Collections.Generic;
using System.Text;

namespace ElFinder.Connector.Command
{
    public interface ICommand
    {
        ElFinder ElFinder { get; }
        string Name { get; }
        ResponseWriter.IResponseWriter Execute();
    }
}
