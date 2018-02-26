using BLG.AspNetCore;
using FluentValidation;
using Forms.Infrastructure.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Forms.Features.Home
{
    public class Edit
    {
        public class Query : IRequest<Command>
        {
            public int ID { get; set; }
        }

        public class Command : IRequest
        {
            public Command()
            {
                FormCategories = new List<SelectListItem>();
               // LinesOfBusiness = new List<SelectListItem>();
            }

            public IFormFile Document { get; set; }
            public int FormID { get; set; }
            public string FormNumber { get; set; }
            public string FormName { get; set; }
            public int? FormCategoryID { get; set; }
            public Guid FileNetDocID { get; set; }
            public long RowVersion { get; set; }
           // public IList<SelectListItem> Companies { get; set; }
            public IList<SelectListItem> FormCategories { get; set; }
        }

        public class Model
        {
            public int FormID { get; set; }
            public string FormName { get; set; }
            public string FormNumber { get; set; }
            public string FormDocumentPath { get; set; }
            public int FormCategoryID { get; set; }
            public Guid FileNetDocID { get; set; }

            public DateTime updated_dt { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                //RuleFor(x => x.Document).NotNull().WithMessage("'Document' should not be empty.");
                RuleFor(x => x.FormID).NotNull();
                RuleFor(x => x.FormName).NotEmpty();
                RuleFor(x => x.FormNumber).NotNull();

                RuleFor(x => x.Document).Custom((x, context) =>
                {
                    if (x != null && x.ContentType != "application/pdf")
                        context.AddFailure("'New Document' type is invalid.");
                });
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
                var model = await GetModelAsync(request.ID);

                //TODO GET DOCUMENT NAME

                var command = new Command
                {
                    FormID = model.FormID,
                    FormNumber = model.FormNumber,
                    FormName = model.FormName,
                    FormCategoryID = model.FormCategoryID,
                    FileNetDocID = model.FileNetDocID,
                    RowVersion = model.updated_dt.Ticks,
                    //   Companies = await _builder.BuildCompanySelectListItemsAsync(),
                    FormCategories = await _builder.BuildFormCategoriesSelectListItemsAsync()
                };

                return command;
            }

            private async Task<Model> GetModelAsync(int ID)
            {
                var sql = @"SELECT * FROM frm.Forms WHERE FormID = @ID";
                var query = await _context.QueryAsync<Model>(sql, new { ID = ID });
                return query.FirstOrDefault();
            }
        }

        public class CommandHandler : AsyncRequestHandler<Command>
        {
            private readonly IDbContext _context;
            private readonly IP8ServicesV1Proxy _proxy;
            private readonly IUserInfo _userInfo;

            public CommandHandler(IDbContext context, IP8ServicesV1Proxy proxy, IUserInfo userInfo)
            {
                _context = context;
                _proxy = proxy;
                _userInfo = userInfo;
            }

            protected async override Task HandleCore(Command command)
            {
                var dbFormNumber = await GetFormNumberAsync(command.FormID);
                var titleChanged = string.Compare(command.FormName, dbFormNumber, StringComparison.OrdinalIgnoreCase) > 0;
                var fileNetDocID = command.FileNetDocID;

                if (command.Document != null || titleChanged)
                    fileNetDocID = await VersionDocumentInFileNet(command);

                await Update(command, fileNetDocID);
            }

            private async Task<Guid> VersionDocumentInFileNet(Command command)
            {
                var client = await _proxy.GetClientAsync();
                var contentItems = GetContentItemsForFileNet(command);
                var properties = new P8ServicesV1.property[]
                {
                    new P8ServicesV1.property { name = "DocumentTitle", value = command.FormNumber },
                    new P8ServicesV1.property { name = "FormNumber", value = command.FormNumber }
                };

                var version = await client.versionDocumentAsync(
                    string.Empty,
                    string.Empty,
                    Constants.FileNetDocClass,
                    contentItems,
                    properties,
                    command.FileNetDocID.ToString(),
                    false,
                    string.Empty,
                    null
                );

                var fileNetDocID = Guid.Parse(version.version.documentId);
                return fileNetDocID;
            }

            private P8ServicesV1.content[] GetContentItemsForFileNet(Command command)
            {
                if (command.Document == null)
                    return null;

                var fileName = string.Format("{0}.pdf", command.FormNumber);

                var contentItems = new P8ServicesV1.content[]
                {
                    new P8ServicesV1.content { content1 = command.Document.ToByteArray(), fileName = fileName }
                };

                return contentItems;
            }

            private async Task<string> GetFormNumberAsync(int ID)
            {
                var sql = @"SELECT FormNumber FROM frm.Forms WHERE FormID = @ID";
                var query = await _context.QueryAsync<string>(sql, new { ID = ID });
                return query.FirstOrDefault();
            }
            protected async Task Update(Command request, Guid fileNetDocID)
            {
                var sql = @"
                UPDATE frm.Forms
                SET FormNumber = @FormNumber,
	                FormName = @FormName,
	                FormCategoryID = @FormCategoryID,
	                FileNetDocID = @FileNetDocID,
	                updated_userid = @UpdateUserID,
	                updated_dt = @UpdateDT
                WHERE FormID = @FormID";
               // WHERE FormID = @ID AND updated_dt = @OrigUpdateDT";

                var result = await _context.ExecuteAsync(
                    sql,
                    new
                    {
                        FormID = request.FormID,
                        FormNumber = request.FormNumber,
                        FormName = request.FormName,
                        FormCategoryID = request.FormCategoryID,
                        FileNetDocID = fileNetDocID,
                        UpdateUserID = _userInfo.UserName,
                        OrigUpdateDT = new DateTime(request.RowVersion),
                        UpdateDT = DateTime.Now
                    }
                );

                if (result == 0)
                    throw new DBConcurrencyException($"A concurrency exception occured in table Forms for ID: {request.FormID}");
            }
        }
    }
}
