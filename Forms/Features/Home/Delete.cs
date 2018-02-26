using Forms.Infrastructure.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Forms.Features.Home
{
    public class Delete
    {
        public class Query : IRequest<Command>
        {
            public int ID { get; set; }
        }

        public class Model
        {
            public int FormID { get; set; }
            public string FormNumber { get; set; }
            public string FormName { get; set; }
            public int FormCategoryID { get; set; }
            public DateTime updated_dt { get; set; }
        }

        public class Command : IRequest
        {
            public int FormID { get; set; }
            public string FormNumber { get; set; }
            public string FormName { get; set; }
            public int FormCategoryID { get; set; }
            public DateTime updated_dt { get; set; }
            public long RowVersion { get; set; }
        }

        public class QueryHandler : AsyncRequestHandler<Query, Command>
        {
            private readonly IDbContext _context;

            public QueryHandler(IDbContext context)
            {
                _context = context;
            }

            protected async override Task<Command> HandleCore(Query request)
            {
                var model = await GetModelAsync(request.ID);

                var command = new Command
                {
                    FormID = model.FormID,
                    FormNumber = model.FormNumber,
                    FormName = model.FormName,
                    FormCategoryID = model.FormCategoryID,
                    RowVersion = model.updated_dt.Ticks
                };

                return command;
            }

            private async Task<Model> GetModelAsync(int ID)
            {
                var sql = @"
                    SELECT FormID, FormNumber, FormName, FormCategoryID, updated_dt
                    FROM frm.Forms
                    WHERE FormID = @ID";

                var query = await _context.QueryAsync<Model>(sql, new { ID = ID });
                return query.FirstOrDefault();
            }
        }

        public class CommandHandler : AsyncRequestHandler<Command>
        {
            private readonly IDbContext _context;

            public CommandHandler(IDbContext context)
            {
                _context = context;
            }

            protected async override Task HandleCore(Command request)
            {
                var sql = @"
                        DELETE FROM frm.StateProgram_LineOfBusiness WHERE FormID = @ID
                        DELETE FROM frm.Forms_StateProgram WHERE FormID = @ID
                        DELETE FROM frm.Forms WHERE FormID = @ID";

                var result = await _context.ExecuteAsync(
                    sql,
                    new
                    {
                        ID = request.FormID,
                        OrigUpdateDT = new DateTime(request.RowVersion)
                    }
                );

                if (result == 0)
                    throw new DBConcurrencyException($"A concurrency exception occured in table ExceptionPage for ID: {request.FormID}");
            }

        }



    }
}
