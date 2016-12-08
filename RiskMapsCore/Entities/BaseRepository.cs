using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace RiskTracker.Entities
{
    public abstract class BaseRepository : IDisposable
    {
        private DatabaseContext context_;

        protected BaseRepository()
        {
            context_ = new DatabaseContext();
        } // BaseRepository

        protected DatabaseContext context
        {
            get { return context_; }
        } // context

        protected void Commit<TEntity>(TEntity modifiedObject) where TEntity : class
        {
            context_.Entry(modifiedObject).State = EntityState.Modified;
            Commit();
        } // Commit

        protected void Delete<TEntity>(TEntity objectToDelete) where TEntity : class 
        {
            context_.Entry(objectToDelete).State = EntityState.Deleted;
            Commit();
        } // Delete

        protected void Commit()
        {
            context_.SaveChanges();
        } // Commit

        public virtual void Dispose()
        {
            context_.Dispose();
        } // Dispose
    } // BaseRepository
}