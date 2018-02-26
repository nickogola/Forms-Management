using FluentValidation;
using Forms.Infrastructure.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Forms.Features.Home
{
    public class CreateStateProgram
    {
        public class Query : IRequest<Command>
        {
            public int ID { get; set; }
        }

        public class Command : IRequest
        {
            public Command()
            {
                StatePrograms = new List<SelectListItem>();
            }

            public int FormID { get; set; }
            public string StateProgramID { get; set; }
            public string LineOfBusinessID { get; set; }
            public DateTime? EffectiveDate { get; set; }
            public DateTime? ExpirationDate { get; set; }
            public List<string> LineOfBusinessIDs { get; set; }
            public byte FormTypeID { get; set; }
            public IList<SelectListItem> StatePrograms { get; set; }
            public IList<SelectListItem> LinesOfBusiness { get; set; }
            public IList<SelectListItem> FormTypes { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.FormID).NotNull().WithMessage("'Form ID' should not be empty.");
                RuleFor(x => x.StateProgramID).NotNull().WithMessage("'State Program' should not be empty.");
                RuleFor(x => x.LineOfBusinessIDs).NotNull().WithMessage("'Line of Business' should not be empty.");
                RuleFor(x => x.EffectiveDate).NotNull().WithMessage("'Effective Date' should not be empty.");

                RuleFor(x => x.ExpirationDate)
                    .GreaterThanOrEqualTo(x => x.EffectiveDate)
                    .When(x => x.ExpirationDate != null)
                    .WithMessage("'Expiration Date' must be greater than or equal to 'Effective Date'.");
            }
        }

        public class QueryHandler : AsyncRequestHandler<Query, Command>
        {
            private ISelectListItemBuilder _builder;

            public QueryHandler(ISelectListItemBuilder builder)
            {
                _builder = builder;
            }

            protected async override Task<Command> HandleCore(Query request)
            {
                var command = new Command
                {
                    FormID = request.ID,
                    StatePrograms = await _builder.BuildStateProgramSelectListItemsAsync(),
                    LinesOfBusiness = await _builder.BuildLineOfBusinessSelectListItemsAsync(),
                    FormTypes = await _builder.BuildFormTypesSelectListItemsAsync()
                };

                return command;
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
                var sql = @"INSERT INTO frm.Forms_StateProgram VALUES ( @FormID, @StateProgramID, @EffectiveDate, @ExpirationDate, @FormTypeID)";

                await _context.ExecuteAsync(
                    sql,
                    new
                    {
                        FormID = request.FormID,
                        StateProgramID = request.StateProgramID,
                        EffectiveDate = request.EffectiveDate,
                        ExpirationDate = request.ExpirationDate,
                        FormTypeID = request.FormTypeID,
                        //UpdateUserID = _userInfo.UserName,
                        //UpdateDT = DateTime.Now
                    }
                );

                // add lines of business

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
