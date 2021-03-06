using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TakeToTalk.Servicos.Negocio;
using TakeToTalk.Servicos.Servicos.Interfaces;

namespace TakeToTalk.Servicos.Servicos.Servico
{
    public abstract class ServicoPadrao<T> : IDisposable where T : ObjetoPadrao
    {
        private IRepositorioPadrao<T> repositorio;

        private void AjustarRepositorio(string stringConexao)
        {
            try
            {
                var type = typeof(IRepositorioPadrao<T>);
                var types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(p => type.IsAssignableFrom(p));

                repositorio = (IRepositorioPadrao<T>)Activator.CreateInstance(types.First(), new object[] { stringConexao });
            }
            catch (Exception ex)
            {
                throw new Exception("Houve um erro ao instanciar as dependências indiretas na clase base. Verifique se 'T' é uma instância que herda a classe 'RepositorioPadrao'", ex);
            }
        }

        public ServicoPadrao()
        {
            AjustarRepositorio("");
        }

        public virtual List<T> Consulte()
        {
            return repositorio.Consulte();
        }

        public virtual T Consulte(string id)
        {
            return repositorio.Consulte(id);
        }

        public virtual List<T> Consulte(Func<T, bool> lambda)
        {
            return repositorio.Consulte(lambda);
        }

        public virtual bool Salve(T obj)
        {
            repositorio.Salve(obj);
            return true;
        }

        public virtual bool Delete(T obj)
        {
            return repositorio.Delete(obj);
        }

        public void Dispose()
        {
            repositorio.Dispose();
        }
    }
}
