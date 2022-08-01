using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    public interface INetSession//接口，是有NetSession实现的
    {
        byte[] GetResponse();
    }
}
