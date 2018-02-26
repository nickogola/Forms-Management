using Forms.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forms.Filters
{
    public class UnitOfWorkAttribute : TypeFilterAttribute
    {
        public UnitOfWorkAttribute() : base(typeof(UnitOfWorkAttributeImpl))
        { }

        private class UnitOfWorkAttributeImpl : IActionFilter
        {
            private readonly IDbContext _context;

            public UnitOfWorkAttributeImpl(IDbContext context)
            {
                _context = context;
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                _context.BeginTransaction();
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
                if (context.Exception != null)
                    _context.RollbackTransaction();
                else
                    _context.CommitTransaction();
            }
        }
    }
}
