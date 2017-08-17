using SQLite.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using SQLiteNetExtensions.Extensions;
using TorneoPredicciones.Interfaces;
using TorneoPredicciones.Models;
using Xamarin.Forms;

namespace TorneoPredicciones.Data
{
    public class DataAccess : IDisposable
    {
        private SQLiteConnection connection;

        public DataAccess() //equiva;lente al datacontext
        {
            var config = DependencyService.Get<IConfig>();
            connection = new SQLiteConnection(config.Platform,
                System.IO.Path.Combine(config.DirectoryDB, "TorneoPredicciones.db3"));
            //equivalente al dataset
            connection.CreateTable<Parameter>();
            connection.CreateTable<Team>();
            connection.CreateTable<User>();
            connection.CreateTable<UserType>();
        }

        public void Insert<T>(T model)
        {
            connection.Insert(model);
        }

        public void Update<T>(T model)
        {
            connection.Update(model);
        }

        public void Delete<T>(T model)
        {
            connection.Delete(model);
        }

        public T First<T>(bool WithChildren) where T : class
        {
            if (WithChildren)
            {
                return connection.GetAllWithChildren<T>().FirstOrDefault();
            }
            else
            {
                return connection.Table<T>().FirstOrDefault();
            }
        }

        public List<T> GetList<T>(bool WithChildren) where T : class
        {
            if (WithChildren)
            {
                return connection.GetAllWithChildren<T>().ToList();
            }
            else
            {
                return connection.Table<T>().ToList();
            }
        }

        public T Find<T>(int pk, bool WithChildren) where T : class
        {
            if (WithChildren)
            {
                return connection.GetAllWithChildren<T>().FirstOrDefault(m => m.GetHashCode() == pk);
            }
            else
            {
                return connection.Table<T>().FirstOrDefault(m => m.GetHashCode() == pk);
            }
        }

        public void Dispose()
        {
            connection.Dispose();
        }
    }

}
