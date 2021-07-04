using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using TakeToTalk.Servicos.Negocio;

namespace TakeToTalk.Servicos.Servicos.Repositorio
{
    public class RepositorioSala : RepositorioPadrao<Sala>
    {
        public RepositorioSala(string stringConexao) : base(stringConexao)
        {
        }

        //SEGUINDO TODOS OS PADOROES E UTILIZANDO UM ORM NENHUM METADO PRECISA SER ESCRITO AQUI
        //IREI REESCREVER TODOS PARA CRIR UM MOKE DAS CONSULTAS
        //IREI CRIAR UMA LISTA DE 'USUARIO' PARA MOKAR O BANCO

        private Dictionary<string, Sala> _dbSalas = new Dictionary<string, Sala>();
        private object lockObject = new object();

        public override List<Sala> Consulte()
        {
            lock (lockObject)
            {
                return _dbSalas.Values.ToList();
            }
        }

        public override List<Sala> Consulte(Func<Sala, bool> lambda)
        {
            lock (lockObject)
            {
                return _dbSalas.Values.Where(lambda).ToList();
            }
        }

        public override Sala Consulte(string id)
        {
            lock (lockObject)
            {
                return _dbSalas.Values.FirstOrDefault(x => x.Id == id);
            }
        }

        public override bool Delete(Sala obj)
        {
            lock (lockObject)
            {
                return _dbSalas.Remove(obj.Id);
            }
        }

        public override void Salve(Sala obj)
        {
            //CHAMAR UM VALIDADOR ATENS DE SALVAR - FLUENTVALIDATION
            lock (lockObject)
            {
                obj.Id = Guid.NewGuid().ToString();
                _dbSalas.Add(obj.Id, obj);
            }
        }
    }
}
