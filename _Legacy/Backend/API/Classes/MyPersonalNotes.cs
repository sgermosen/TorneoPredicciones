using System;
using Domain;
using System.Data.Entity.Validation;

namespace API.Classes
{
    public class MyPersonalNotes
    {
        private DataContext db = new DataContext();

        //para identificar de forma eficiente el error real, causado por entity, lo cual es un dolor de cabeza
        //aqui un try, super pesado, pero poderoso para ello, solo usar si se esta en modo debug, no recomendable si se esta en modo produccion
        public   void IdentifyModelErrors()
        {

            try
            {
                db.SaveChanges();

            }
            catch (DbEntityValidationException e)
            {
                var message = string.Empty;
                foreach (var eve in e.EntityValidationErrors)
                {

                    //Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                    //    eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    message = string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);

                    foreach (var ve in eve.ValidationErrors)
                    {
                        //Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                        //    ve.PropertyName, ve.ErrorMessage);
                        message += string.Format("\n- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                 BadRequest(message);
            }
            catch (Exception ex)
            {
                    BadRequest(ex.Message);
            }






        }

        private     string BadRequest(string message)
        {
            return message;
        }
    }
}