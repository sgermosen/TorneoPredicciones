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
        private readonly SQLiteConnection _connection;

        public DataAccess() //equiva;lente al datacontext
        {
            var config = DependencyService.Get<IConfig>();
            _connection = new SQLiteConnection(config.Platform,
                System.IO.Path.Combine(config.DirectoryDb, "TPC.db3"));
            //equivalente al dataset
            _connection.CreateTable<Parameter>();
            _connection.CreateTable<Team>();
            _connection.CreateTable<User>();
            _connection.CreateTable<UserType>();
        }

        public void Insert<T>(T model)
        {
            _connection.Insert(model);
        }

        public void Update<T>(T model)
        {
            _connection.Update(model);
        }

        public void Delete<T>(T model)
        {
            _connection.Delete(model);
        }

        public T First<T>(bool withChildren) where T : class
        {
            if (withChildren)
            {
                return _connection.GetAllWithChildren<T>().FirstOrDefault();
            }
            else
            {
                return _connection.Table<T>().FirstOrDefault();
            }
        }

        public List<T> GetList<T>(bool withChildren) where T : class
        {
            if (withChildren)
            {
                return _connection.GetAllWithChildren<T>().ToList();
            }
            else
            {
                return _connection.Table<T>().ToList();
            }
        }

        public T Find<T>(int pk, bool withChildren) where T : class
        {
            if (withChildren)
            {
                return _connection.GetAllWithChildren<T>().FirstOrDefault(m => m.GetHashCode() == pk);
            }
            else
            {
                return _connection.Table<T>().FirstOrDefault(m => m.GetHashCode() == pk);
            }
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }

}
