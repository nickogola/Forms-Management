
    using Forms.Infrastructure;
    using FluentValidation;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Forms.Infrastructure.Interfaces;

    namespace Forms.Features.Home
    {
        public class Create
        {
            public class Query : IRequest<Command>
            { }

            public class Command : IRequest
            {
                public Command()
                {
                    FormCategories = new List<SelectListItem>();
                    StatePrograms = new List<SelectListItem>();
                    //LinesOfBusiness = new List<SelectListItem>();
                }

                public IFormFile Document { get; set; }
                public int FormID { get; set; }
                public string FormNumber { get; set; }
                public string FormName { get; set; }
                public int? FormCategoryID { get; set; }
                public IList<SelectListItem> FormCategories { get; set; }
                public IList<SelectListItem> StatePrograms { get; set; }
               // public IList<SelectListItem> LinesOfBusiness { get; set; }
            }

            public class CommandValidator : AbstractValidator<Command>
            {
                public CommandValidator()
                {
                    RuleFor(x => x.Document).NotEmpty();
                    RuleFor(x => x.FormNumber).NotEmpty().WithMessage("'Form Number' should not be empty.");
                    RuleFor(x => x.FormName).NotEmpty();
                    RuleFor(x => x.FormCategoryID).NotEmpty().WithMessage("'Form Category' should not be empty.");
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
                    var command = new Command
                    {
                        //Companies = await _builder.BuildCompanySelectListItemsAsync(),
                        StatePrograms = await _builder.BuildStateProgramSelectListItemsAsync(),
                        FormCategories = await _builder.BuildFormCategoriesSelectListItemsAsync()
                        // LinesOfBusiness = await _builder.BuildLineOfBusinessSelectListItemsAsync(),
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
                    var sql = @"INSERT INTO frm.Forms VALUES ( @FormNumber, @FormName, @FormCategoryID, @FormDocumentPath, @UpdateDT, @UpdateUserID)";

                    await _context.ExecuteAsync(
                        sql,
                        new
                        {
                            FormNumber = request.FormNumber,
                            FormCategoryID = request.FormCategoryID,
                            FormName = request.FormName,
                            //TODO
                            FormDocumentPath = request.Document.FileName,
                            UpdateUserID = _userInfo.UserName,
                            UpdateDT = DateTime.Now
                        });
                }
            }
        }
    }


