using Microsoft.EntityFrameworkCore; //EXISTE NESTE PROJETO PARA FUNDAMENTAR  PADRAO UTILIZADO
using System;
using System.Collections.Generic;
using TakeToTalk.Servicos.Negocio;
using TakeToTalk.Servicos.Servicos.Interfaces;

namespace TakeToTalk.Servicos.Servicos.Repositorio
{
    public abstract class RepositorioPadrao<T> : DbContext, IRepositorioPadrao<T> where T : ObjetoPadrao
    {
        protected readonly string StringConexao;
        public RepositorioPadrao(string stringConexao)
            : base()
        {
            StringConexao = stringConexao;
        }

        public virtual List<T> Consulte()
        {
            return new List<T>();
        }
        public virtual List<T> Consulte(Func<T, bool> lambda)
        {
            return new List<T>();
        }

        public virtual T Consulte(string id)
        {
            return null;
        }

        public virtual void Salve(T obj)
        {
        }

        public virtual bool Delete(T obj)
        {
            return true;
        }
    }
}
