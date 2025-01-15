using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.SemanticKernel
{
    public interface ISemanticKernelService
    {
        Task<string> Chat(string question);
    }
}
