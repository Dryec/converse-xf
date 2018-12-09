using System;
using System.Threading.Tasks;
using Client;
using Protocol;

namespace Converse.TokenMessages
{
    public interface ITokenMessage
    {
        int Type { get; }
    }
}
