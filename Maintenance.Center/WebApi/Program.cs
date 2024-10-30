using Application;
using Application.PipelineBehaviors;
using Application.Pricebook2;
using Application.Repositories.ControlledUpdates;
using Application.Writing;
using Application.Writing.Pricebook2;
using Data.Common.Contracts;
using Data.Definitions.Pricebook2;
using Data.Definitions.Pricebook2.View;
using FluentValidation;
using Infrastructure.Data.ControlledUpdates;
using Infrastructure.Data.Pricebook2;
using Infrastructure.Data.Pricebook2.View;
using MediatR;
using Serilog;
using WebApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;

services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new DateOnlyConverter());
        options.JsonSerializerOptions.Converters.Add(new TimeOnlyConverter());
    }); ;

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddLogging(b =>
{
    var configuration = builder.Configuration;

    b.AddSerilog(Log.Logger
        = new LoggerConfiguration()
        .Enrich.FromLogContext()
        .MinimumLevel.Information()
        .WriteTo.Console()
        .WriteTo.MSSqlServer(
            connectionString: configuration.GetConnectionString("EcommerceApps"),
            sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions()
            {
                AutoCreateSqlTable = true,
                TableName = "SnrEcommerceMaintenanceCenterLogs"
            },
            sinkOptionsSection: null,
            appConfiguration: null,
            restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning)
        .CreateLogger());
});

#region Pricebook 2

services.AddTransient<IAsyncQuery<IEnumerable<Data.Definitions.Pricebook2.View.ProductListItem>, Application.Pricebook2.ProductListItemsPagedRequest>>(
    s => new Infrastructure.Data.Pricebook2.View.ProductListItemsPagedQuery(
        builder.Configuration.GetConnectionString("Ecommerce"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout")));
services.AddTransient<IAsyncQuery<IEnumerable<Data.Definitions.Pricebook2.View.PricingHistoryListItem>, Application.Pricebook2.PricingChangeListItemsPagedRequest>>(
    s => new Infrastructure.Data.Pricebook2.View.PricingHistoryListItemsPagedQuery(
        builder.Configuration.GetConnectionString("Ecommerce"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout")));

services.AddTransient<IStreamlinedCollectionWriter<Data.Definitions.Pricebook2.ProductUpdateTemplateItem>, Application.Writing.Pricebook2.ProductUpdateTemplateItemsStreamlinedCollectionWriter>();
services.AddTransient<IStreamlinedCollectionWriterV2<Data.Definitions.Pricebook2.MetromartBuyXTakeYConfigurationTemplateItem>, Application.Writing.Pricebook2.MetromartBuyXTakeYConfigurationTemplateItemsStreamlinedCollectionWriterV2>();

services.AddTransient<IAsyncDataSourceIterator<Data.Definitions.Pricebook2.ProductUpdateTemplateItem>>(
    s => new Infrastructure.Data.Pricebook2.ProductUpdateTemplateItemDataSourceIterator(
        builder.Configuration.GetConnectionString("Ecommerce"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout")));

services.AddTransient<ICollectionReader<Data.Definitions.Pricebook2.ProductUpdateTemplateItem, Stream>, Application.Writing.Pricebook2.ProductUpdateTemplateItemCollectionReader>();

services.AddTransient<IAsyncDataWriter<IEnumerable<Data.Definitions.Pricebook2.ProductUpdateTemplateItem>>>(
    s => new Infrastructure.Data.Pricebook2.ProductUpdateTemplateItemCollectionUpdateWriter(
        builder.Configuration.GetConnectionString("Ecommerce"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Persistence:BatchWriting:PreferredSize")));

#region Metromart Buy X Take Y

services.AddTransient<IAsyncQuery<IEnumerable<Data.Definitions.Pricebook2.View.MetromartBuyXTakeYConfigurationListItem>, Application.Pricebook2.MetromartBuyXTakeYConfigurationListItemsPagedRequest>>(
    s => new Infrastructure.Data.Pricebook2.View.MetromartBuyXTakeYConfigurationListItemsPagedQuery(
        connection: builder.Configuration.GetConnectionString("EcommerceApps"),
        commandTimeout: builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout")));

services.AddTransient<IAsyncEnumerableQuery<Data.Definitions.Pricebook2.MetromartBuyXTakeYConfigurationTemplateItem>>(
    s => new Infrastructure.Data.Pricebook2.MetromartBuyXTakeYConfigurationTemplateItemsAsyncEnumerableQuery(
        connection: builder.Configuration.GetConnectionString("EcommerceApps"),
        commandTimeout: builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout")));

services.AddTransient<IAsyncDataWriter<IEnumerable<Data.Definitions.Pricebook2.MetromartBuyXTakeYConfigurationTemplateItem>>>(
    s => new Infrastructure.Data.Pricebook2.MetromartBuyXTakeYConfigurationTemplateItemCollectionUpdateWriter(
        connection: builder.Configuration.GetConnectionString("EcommerceApps"),
        commandTimeout: builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout"),
        batchSize: builder.Configuration.GetValue<int>("Infrastructure:Data:Persistence:BatchWriting:PreferredSize")));

services.AddTransient<ICollectionReaderV2<Data.Definitions.Pricebook2.MetromartBuyXTakeYConfigurationTemplateItem, Stream>>(
    s => new Application.Writing.Pricebook2.MetromartBuyXTakeYConfigurationTemplateItemCollectionReaderV2(
        dateFormat: builder.Configuration["Application:Pricebook2:Metromart:BuyXTakeY:DateFormat"]));


#endregion

#endregion

#region Pricebook 3 

services.AddTransient<IAsyncQuery<IEnumerable<Data.Definitions.Pricebook3.View.ProductListItem>, Application.Pricebook3.ProductListItemsPagedRequest>>(
    s => new Infrastructure.Data.Pricebook3.View.ProductListItemsPagedQuery(
        builder.Configuration.GetConnectionString("Ecommerce"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout")));
services.AddTransient<IAsyncQuery<IEnumerable<Data.Definitions.Pricebook3.View.PricingHistoryListItem>, Application.Pricebook3.PricingChangeListItemsPagedRequest>>(
    s => new Infrastructure.Data.Pricebook3.View.PricingHistoryListItemsPagedQuery(
        builder.Configuration.GetConnectionString("Ecommerce"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout")));
services.AddTransient<IAsyncQuery<IEnumerable<Data.Definitions.Pricebook3.View.PromotionListItem>, Application.Pricebook3.PromotionListItemsPagedRequest>>(
    s => new Infrastructure.Data.Pricebook3.View.PromotionListItemsPagedQuery(
        builder.Configuration.GetConnectionString("Ecommerce"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout"),
        builder.Configuration.GetValue<int>("Application:Pricebook3:Promotions:CurrentMargin")));

services.AddTransient<IStreamlinedCollectionWriter<Data.Definitions.Pricebook3.ProductUpdateTemplateItem>, Application.Writing.Pricebook3.ProductUpdateTemplateItemsStreamlinedCollectionWriter>();
services.AddTransient<IStreamlinedCollectionWriter<Data.Definitions.Pricebook3.PromotionReviewTemplateItem>, Application.Writing.Pricebook3.PromotionReviewTemplateItemsStreamlinedCollectionWriter>();

services.AddTransient<IAsyncDataSourceIterator<Data.Definitions.Pricebook3.ProductUpdateTemplateItem>>(
    s => new Infrastructure.Data.Pricebook3.ProductUpdateTemplateItemDataSourceIterator(
        builder.Configuration.GetConnectionString("Ecommerce"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout")));
services.AddTransient<IAsyncDataSourceIterator<Data.Definitions.Pricebook3.PromotionReviewTemplateItem>>(
    s => new Infrastructure.Data.Pricebook3.PromotionReviewTemplateItemDataSourceIterator(
        builder.Configuration.GetConnectionString("Ecommerce"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout"),
        builder.Configuration.GetValue<int>("Application:Pricebook3:Promotions:CurrentMargin")));

services.AddTransient<ICollectionReader<Data.Definitions.Pricebook3.ProductUpdateTemplateItem, Stream>, Application.Writing.Pricebook3.ProductUpdateTemplateItemCollectionReader>();
services.AddTransient<ICollectionReader<Data.Definitions.Pricebook3.PromotionUpdateItem, Stream>, Application.Writing.Pricebook3.PromotionUpdateItemCollectionReader>();

services.AddTransient<IAsyncDataWriter<IEnumerable<Data.Definitions.Pricebook3.ProductUpdateTemplateItem>>>(
    s => new Infrastructure.Data.Pricebook3.ProductUpdateTemplateItemCollectionUpdateWriter(
        builder.Configuration.GetConnectionString("Ecommerce"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Persistence:BatchWriting:PreferredSize")));
services.AddTransient<IAsyncDataWriter<IEnumerable<Data.Definitions.Pricebook3.PromotionUpdateItem>>>(
    s => new Infrastructure.Data.Pricebook3.PromotionUpdateItemCollectionUpdateWriter(
        builder.Configuration.GetConnectionString("Ecommerce"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Persistence:BatchWriting:PreferredSize")));

#endregion

#region Pricebook 4

services.AddTransient<IAsyncQuery<IEnumerable<Data.Definitions.Pricebook4.View.ProductListItem>, Application.Pricebook4.ProductListItemsPagedRequest>>(
    s => new Infrastructure.Data.Pricebook4.View.ProductListItemsPagedQuery(
        builder.Configuration.GetConnectionString("Ecommerce"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout")));
services.AddTransient<IAsyncQuery<IEnumerable<Data.Definitions.Pricebook4.View.PricingHistoryListItem>, Application.Pricebook4.PricingChangeListItemsPagedRequest>>(
    s => new Infrastructure.Data.Pricebook4.View.PricingHistoryListItemsPagedQuery(
        builder.Configuration.GetConnectionString("Ecommerce"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout")));
services.AddTransient<IAsyncQuery<IEnumerable<Data.Definitions.Pricebook4.View.PromotionListItem>, Application.Pricebook4.PromotionListItemsPagedRequest>>(
    s => new Infrastructure.Data.Pricebook4.View.PromotionListItemsPagedQuery(
        builder.Configuration.GetConnectionString("Ecommerce"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout"),
        builder.Configuration.GetValue<int>("Application:Pricebook4:Promotions:CurrentMargin")));
services.AddTransient<IAsyncQuery<IEnumerable<Data.Definitions.Pricebook4.View.WeightedSkuListItem>, Application.Pricebook4.WeightedSkuListItemsPagedRequest>>(
    s => new Infrastructure.Data.Pricebook4.View.WeightedSkuListItemsPagedQuery(
        builder.Configuration.GetConnectionString("Ecommerce"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout")));

services.AddTransient<IStreamlinedCollectionWriter<Data.Definitions.Pricebook4.ProductUpdateTemplateItem>, Application.Writing.Pricebook4.ProductUpdateTemplateItemsStreamlinedCollectionWriter>();
services.AddTransient<IStreamlinedCollectionWriter<Data.Definitions.Pricebook4.PromotionReviewTemplateItem>, Application.Writing.Pricebook4.PromotionReviewTemplateItemsStreamlinedCollectionWriter>();
services.AddTransient<IStreamlinedCollectionWriter<Data.Definitions.Pricebook4.WeightedSkuUpdateTemplateItem>, Application.Writing.Pricebook4.WeightedSkuUpdateTemplateItemsStreamlinedCollectionWriter>();

services.AddTransient<IAsyncDataSourceIterator<Data.Definitions.Pricebook4.ProductUpdateTemplateItem>>(
    s => new Infrastructure.Data.Pricebook4.ProductUpdateTemplateItemDataSourceIterator(
        builder.Configuration.GetConnectionString("Ecommerce"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout")));
services.AddTransient<IAsyncDataSourceIterator<Data.Definitions.Pricebook4.PromotionReviewTemplateItem>>(
    s => new Infrastructure.Data.Pricebook4.PromotionReviewTemplateItemDataSourceIterator(
        builder.Configuration.GetConnectionString("Ecommerce"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout"),
        builder.Configuration.GetValue<int>("Application:Pricebook4:Promotions:CurrentMargin")));
services.AddTransient<IAsyncDataSourceIterator<Data.Definitions.Pricebook4.WeightedSkuUpdateTemplateItem>>(
    s => new Infrastructure.Data.Pricebook4.WeightedSkuUpdateTemplateItemDataSourceIterator(
        builder.Configuration.GetConnectionString("Ecommerce"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout")));

services.AddTransient<ICollectionReader<Data.Definitions.Pricebook4.ProductUpdateTemplateItem, Stream>, Application.Writing.Pricebook4.ProductUpdateTemplateItemCollectionReader>();
services.AddTransient<ICollectionReader<Data.Definitions.Pricebook4.PromotionUpdateItem, Stream>, Application.Writing.Pricebook4.PromotionUpdateItemCollectionReader>();
services.AddTransient<ICollectionReader<Data.Definitions.Pricebook4.WeightedSkuUpdateTemplateItem, Stream>, Application.Writing.Pricebook4.WeightedSkuUpdateTemplateItemCollectionReader>();

services.AddTransient<IAsyncDataWriter<IEnumerable<Data.Definitions.Pricebook4.ProductUpdateTemplateItem>>>(
    s => new Infrastructure.Data.Pricebook4.ProductUpdateTemplateItemCollectionUpdateWriter(
        builder.Configuration.GetConnectionString("Ecommerce"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Persistence:BatchWriting:PreferredSize")));
services.AddTransient<IAsyncDataWriter<IEnumerable<Data.Definitions.Pricebook4.PromotionUpdateItem>>>(
    s => new Infrastructure.Data.Pricebook4.PromotionUpdateItemCollectionUpdateWriter(
        builder.Configuration.GetConnectionString("Ecommerce"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Persistence:BatchWriting:PreferredSize")));
services.AddTransient<IAsyncDataWriter<IEnumerable<Data.Definitions.Pricebook4.WeightedSkuUpdateTemplateItem>>>(
    s => new Infrastructure.Data.Pricebook4.WeightedSkuUpdateTemplateItemCollectionUpdateWriter(
        builder.Configuration.GetConnectionString("Ecommerce"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Persistence:BatchWriting:PreferredSize")));

#endregion

#region Pricebook 5 

services.AddTransient<IAsyncQuery<IEnumerable<Data.Definitions.Pricebook5.View.ProductListItem>, Application.Pricebook5.ProductListItemsPagedRequest>>(
    s => new Infrastructure.Data.Pricebook5.View.ProductListItemsPagedQuery(
        builder.Configuration.GetConnectionString("Ecommerce"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout")));
services.AddTransient<IAsyncQuery<IEnumerable<Data.Definitions.Pricebook5.View.PricingHistoryListItem>, Application.Pricebook5.PricingChangeListItemsPagedRequest>>(
    s => new Infrastructure.Data.Pricebook5.View.PricingHistoryListItemsPagedQuery(
        builder.Configuration.GetConnectionString("Ecommerce"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout")));
services.AddTransient<IAsyncQuery<IEnumerable<Data.Definitions.Pricebook5.View.PromotionListItem>, Application.Pricebook5.PromotionListItemsPagedRequest>>(
    s => new Infrastructure.Data.Pricebook5.View.PromotionListItemsPagedQuery(
        builder.Configuration.GetConnectionString("Ecommerce"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout"),
        builder.Configuration.GetValue<int>("Application:Pricebook5:Promotions:CurrentMargin")));

services.AddTransient<IStreamlinedCollectionWriter<Data.Definitions.Pricebook5.ProductUpdateTemplateItem>, Application.Writing.Pricebook5.ProductUpdateTemplateItemsStreamlinedCollectionWriter>();
services.AddTransient<IStreamlinedCollectionWriter<Data.Definitions.Pricebook5.PromotionReviewTemplateItem>, Application.Writing.Pricebook5.PromotionReviewTemplateItemsStreamlinedCollectionWriter>();

services.AddTransient<IAsyncDataSourceIterator<Data.Definitions.Pricebook5.ProductUpdateTemplateItem>>(
    s => new Infrastructure.Data.Pricebook5.ProductUpdateTemplateItemDataSourceIterator(
        builder.Configuration.GetConnectionString("Ecommerce"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout")));
services.AddTransient<IAsyncDataSourceIterator<Data.Definitions.Pricebook5.PromotionReviewTemplateItem>>(
    s => new Infrastructure.Data.Pricebook5.PromotionReviewTemplateItemDataSourceIterator(
        builder.Configuration.GetConnectionString("Ecommerce"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout"),
        builder.Configuration.GetValue<int>("Application:Pricebook5:Promotions:CurrentMargin")));

services.AddTransient<ICollectionReader<Data.Definitions.Pricebook5.ProductUpdateTemplateItem, Stream>, Application.Writing.Pricebook5.ProductUpdateTemplateItemCollectionReader>();
services.AddTransient<ICollectionReader<Data.Definitions.Pricebook5.PromotionUpdateItem, Stream>, Application.Writing.Pricebook5.PromotionUpdateItemCollectionReader>();

services.AddTransient<IAsyncDataWriter<IEnumerable<Data.Definitions.Pricebook5.ProductUpdateTemplateItem>>>(
    s => new Infrastructure.Data.Pricebook5.ProductUpdateTemplateItemCollectionUpdateWriter(
        builder.Configuration.GetConnectionString("Ecommerce"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Persistence:BatchWriting:PreferredSize")));
services.AddTransient<IAsyncDataWriter<IEnumerable<Data.Definitions.Pricebook5.PromotionUpdateItem>>>(
    s => new Infrastructure.Data.Pricebook5.PromotionUpdateItemCollectionUpdateWriter(
        builder.Configuration.GetConnectionString("Ecommerce"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Persistence:BatchWriting:PreferredSize")));

#endregion

#region Controlled Updates

services.AddSingleton<IAsyncQuery<IEnumerable<Data.Definitions.ControlledUpdates.Scope>>>(
    s => new Infrastructure.Data.Projections.ControlledUpdates.AllScopesQuery(
        builder.Configuration.GetConnectionString("EcommerceApps"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout")));

services.AddSingleton<IAsyncQuery<IEnumerable<Data.Definitions.ControlledUpdates.Warehouse>>>(
    s => new Infrastructure.Data.Projections.ControlledUpdates.AllWarehousesQuery(
        builder.Configuration.GetConnectionString("EcommerceApps"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout")));

services.AddSingleton<IAsyncQuery<IEnumerable<Core.ControlledUpdates.Scope>, IEnumerable<Application.ControlledUpdates.CampaignScope>>> (
    s => new Infrastructure.Data.ControlledUpdates.CampaignScopesByIdsQuery(
        builder.Configuration.GetConnectionString("EcommerceApps"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout")));

services.AddSingleton<IAsyncQuery<IEnumerable<Core.ControlledUpdates.Warehouse>, IEnumerable<Application.ControlledUpdates.WarehouseCode>>>(
    s => new Infrastructure.Data.ControlledUpdates.WarehousesByCodesQuery(
        builder.Configuration.GetConnectionString("EcommerceApps"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout")));

services.AddSingleton<ICampaignRepository>(
    s => new CampaignRepository(
        builder.Configuration.GetConnectionString("EcommerceApps"),
        builder.Configuration.GetValue<int>("Infrastructure:Data:Sql:CommandTimeout")));

#endregion

services.AddMediatR(typeof(Program).Assembly, typeof(ApplicationLogicException).Assembly);

services.AddTransient<ExceptionHandlingMiddleware>();

services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed((string origin) => true)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

services.AddScoped<Foo>();

services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

services.AddValidatorsFromAssembly(typeof(ApplicationLogicException).Assembly);

var app = builder.Build();

app.UseRouting();
app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web Api V1");
    });
}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();