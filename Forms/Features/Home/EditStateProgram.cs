using FluentValidation;
using Forms.Infrastructure.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static Forms.Features.Home.SelectListItemBuilder;

namespace Forms.Features.Home
{
    public class EditStateProgram
    {
        public class Query : IRequest<Command>
        {
            public int ID { get; set; }
            public int StateProgramID { get; set; }
        }

        public class Command : IRequest
        {
            public Command()
            {
                FormTypes = new List<SelectListItem>();
            }
            public int FormID { get; set; }
            public string StateCode { get; set; }
            public string ProgramCode { get; set; }
            public string StateProgramID { get; set; }
            public string LineOfBusinessID { get; set; }
            public List<string> LineOfBusinessIDs { get; set; }
            public DateTime? EffectiveDate { get; set; }
            public DateTime? ExpirationDate { get; set; }
            public int FormTypeID { get; set; }
            public long RowVersion { get; set; }

            public IList<SelectListItem> LinesOfBusiness { get; set; }
            public IList<SelectListItem> FormTypes { get; set; }
        }

        public class Model
        {
            public int FormID { get; set; }
            public string StateCode { get; set; }
            public string ProgramCode { get; set; }
            public string StateProgramID { get; set; }
            public DateTime EffectiveDate { get; set; }
            public DateTime? ExpirationDate { get; set; }
            public int FormTypeID { get; set; }
            public DateTime UpdateDT { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.EffectiveDate).NotEmpty();

                RuleFor(x => x.ExpirationDate)
                    .GreaterThanOrEqualTo(x => x.EffectiveDate)
                    .When(x => x.ExpirationDate != null)
                    .WithMessage("'Expiration Date' must be greater than or equal to 'Effective Date'.");
            }
        }


        public class QueryHandler : AsyncRequestHandler<Query, Command>
        {
            private readonly IDbContext _context;
            private readonly ISelectListItemBuilder _builder;

            public QueryHandler(IDbContext context, ISelectListItemBuilder builder)
            {
                _context = context;
                _builder = builder;
            }

            protected async override Task<Command> HandleCore(Query request)
            {
                var model = await GetModelAsync(request.ID, request.StateProgramID);

                var command = new Command
                {
                    FormID = model.FormID,
                    StateCode = model.StateCode,
                    ProgramCode = model.ProgramCode,
                    StateProgramID = model.StateProgramID,
                    EffectiveDate = model.EffectiveDate,
                    ExpirationDate = model.ExpirationDate,
                    FormTypeID = model.FormTypeID,
                    RowVersion = model.UpdateDT.Ticks,
                    LinesOfBusiness = await _builder.BuildLineOfBusinessSelectListItemsAsync(),
                    FormTypes = await _builder.BuildFormTypesSelectListItemsAsync()
                };
                command.LineOfBusinessIDs = GetLinesOfBusinessAsync(command.FormID, request.StateProgramID).Result;
                return command;
            }

            private async Task<Model> GetModelAsync(int ID, int StateProgramID)
            {
                var sql = @"
                        SELECT sp.FormID, sp.StateProgramID, fsp.StateCode, fsp.ProgramCode, sp.EffectiveDate, sp.ExpirationDate, sp.FormtypeID
                        FROM frm.Forms_StateProgram sp
                        INNER JOIN frm.StateProgram fsp ON sp.StateProgramID = fsp.StateProgramID
                        WHERE FormID = @ID and sp.StateProgramID = @StateProgramID";

                var query = await _context.QueryAsync<Model>(sql, new { ID = ID, StateProgramID = StateProgramID });
                return query.FirstOrDefault();
            }

            private async Task<List<string>> GetLinesOfBusinessAsync(int ID, int StateProgramID)
            {
                var sql = @"
                        SELECT FormID, LineOfBusinessID, StateProgramID
                        FROM frm.StateProgram_LineOfBusiness 
                        WHERE FormID = @ID and StateProgramID = @StateProgramID";

                var query = await _context.QueryAsync<List<string>>(sql, new { ID = ID, StateProgramID = StateProgramID });
                var lobs = await _context.QueryAsync<StateProgram_LineOfBusiness>(sql, new { ID = ID, StateProgramID = StateProgramID });

                var list = new List<string>();

                list = lobs.Select(l => l.LineOfBusinessID.ToString()).ToList();
                return list;
                //return query.FirstOrDefault();
            }
        }

        public class CommandHandler : AsyncRequestHandler<Command>
        {
            private readonly IDbContext _context;
            private readonly IUserInfo _userInfo;

            public CommandHandler(IDbContext context, IUserInfo userInfo)
            {
                _context = context;
                _userInfo = userInfo;
            }

            protected async override Task HandleCore(Command request)
            {
                var sql = @"
                        UPDATE frm.Forms_StateProgram
                        SET EffectiveDate = @EffectiveDate,
	                        ExpirationDate = @ExpirationDate,
	                       FormTypeID = @FormTypeID
                        WHERE FormID = @FormID";

                var result = await _context.ExecuteAsync(
                    sql,
                    new
                    {
                        FormID = request.FormID,
                        EffectiveDate = request.EffectiveDate,
                        ExpirationDate = request.ExpirationDate,
                        FormTypeID = request.FormTypeID
                    }
                );

                if (result == 0)
                    throw new DBConcurrencyException($"A concurrency exception occured in table Forms_StateProgram for ID: {request.FormID}");

                foreach (var LobID in request.LineOfBusinessIDs)
                {
                    var lobSql = @"INSERT INTO frm.StateProgram_LineOfBusiness VALUES (@FormID, @LineOfBusinessID, @StateProgramID)";

                    var res = await _context.ExecuteAsync(
                        lobSql,
                        new
                        {
                            FormID = request.FormID,
                            LineOfBusinessID = LobID,
                            StateProgramID = request.StateProgramID
                        }
                    );

                    if (res == 0)
                        throw new DBConcurrencyException($"A concurrency exception occured in table Forms_StateProgram for ID: {request.FormID}");
                }
            }
        }
    }
}
