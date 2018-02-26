using Forms.Infrastructure.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Forms.Features.Home
{
    public class DeleteStateProgram
    {
        public class Query : IRequest<Command>
        {
            public int ID { get; set; }
            public int StateProgramID { get; set; }
        }

        public class Command : IRequest
        {
            public int FormID { get; set; }
            public int StateProgramID { get; set; }
            public string StateCode { get; set; }
            public string ProgramCode { get; set; }
            public DateTime EffectiveDate { get; set; }
            public DateTime? ExpirationDate { get; set; }
            public long RowVersion { get; set; }
        }

        public class Model
        {
            public int FormID { get; set; }
            public int StateProgramID { get; set; }
            public string StateCode { get; set; }
            public string ProgramCode { get; set; }
            public DateTime EffectiveDate { get; set; }
            public DateTime? ExpirationDate { get; set; }
            public DateTime UpdateDT { get; set; }
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
                var model = await GetModelAsync(request.ID, request.StateProgramID);

                var command = new Command
                {
                    FormID = model.FormID,
                    StateProgramID = model.StateProgramID,
                    StateCode = model.StateCode,
                    ProgramCode = model.ProgramCode,
                    EffectiveDate = model.EffectiveDate,
                    ExpirationDate = model.ExpirationDate,
                    RowVersion = model.UpdateDT.Ticks
                };

                return command;
            }

            private async Task<Model> GetModelAsync(int ID, int StateProgramID)
            {
                var sql = @"
                SELECT sp.FormID, sp.StateProgramID, fsp.StateCode, fsp.ProgramCode, sp.EffectiveDate, sp.ExpirationDate
                FROM frm.Forms_StateProgram sp
                INNER JOIN frm.StateProgram fsp ON sp.StateProgramID = fsp.StateProgramID
                WHERE FormID = @ID And sp.StateProgramID = @StateProgramID";

                var query = await _context.QueryAsync<Model>(sql, new { ID = ID, StateProgramID = StateProgramID });
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
                var sql = @" DELETE FROM frm.StateProgram_LineOfBusiness WHERE FormID = @ID AND StateProgramID = @StateProgramID
DELETE FROM frm.Forms_StateProgram WHERE FormID = @ID AND StateProgramID = @StateProgramID";

                var result = await _context.ExecuteAsync(
                    sql,
                    new
                    {
                        ID = request.FormID,
                        StateProgramID = request.StateProgramID
                        // OrigUpdateDT = new DateTime(request.RowVersion)
                    }
                );

                if (result == 0)
                    throw new DBConcurrencyException($"A concurrency exception occured in table Forms_StateProgram for ID: {request.FormID}");
            }
        }
    }
}
