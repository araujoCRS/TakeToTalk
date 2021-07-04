using System;
using System.Collections.Generic;
using System.Linq;
using TakeToTalk.Servicos.Negocio;

namespace TakeToTalk.Servicos.Servicos.Repositorio
{
    public class RepositorioUsuario : RepositorioPadrao<Usuario>
    {
        public RepositorioUsuario(string stringConexao) : base(stringConexao)
        {
        }

        //SEGUINDO TODOS OS PADOROES E UTILIZANDO UM 'ORM' NENHUM METADO PRECISA SER ESCRITO AQUI
        //IREI REESCREVER TODOS OS METODOS PARA CRIAR UM MOKE DAS CONSULTAS
        //IREI CRIAR UMA LISTA DE 'USUARIO' PARA MOKAR O BANCO

        private Dictionary<string, Usuario> _dbUsuarios = new Dictionary<string, Usuario>();
        private object lockObject = new object();

        public override List<Usuario> Consulte()
        {
            lock (lockObject)
            {
                return _dbUsuarios.Values.ToList();
            }
        }

        public override List<Usuario> Consulte(Func<Usuario, bool> lambda)
        {
            lock (lockObject)
            {
                return _dbUsuarios.Values.Where(lambda).ToList();
            }
        }

        public override Usuario Consulte(string id)
        {
            lock (lockObject)
            {
                return _dbUsuarios.Values.FirstOrDefault(x => x.Id == id);
            }
        }

        public override bool Delete(Usuario obj)
        {
            lock (lockObject)
            {
                return _dbUsuarios.Remove(obj.Id);
            }
        }

        public override void Salve(Usuario obj)
        {
            lock (lockObject)
            {
                obj.Id = Guid.NewGuid().ToString();
                _dbUsuarios.Add(obj.Id, obj);
            }
        }
    }
}
