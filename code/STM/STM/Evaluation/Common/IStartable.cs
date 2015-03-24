using System;
namespace Evaluation.Common
{
    interface IStartable
    {
        System.Threading.Tasks.Task Start();
    }
}
