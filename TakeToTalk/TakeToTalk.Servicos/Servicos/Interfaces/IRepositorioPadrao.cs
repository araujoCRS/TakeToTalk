using System;
using System.Collections.Generic;

namespace TakeToTalk.Servicos.Servicos.Interfaces
{
    public interface IRepositorioPadrao<T>: IDisposable
    {
        List<T> Consulte();
        T Consulte(string id);
        List<T> Consulte(Func<T, bool> lambda);
        void Salve(T obj);
        bool Delete(T obj);
    }
}
