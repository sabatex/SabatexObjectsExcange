using sabatex.Extensions.ClassExtensions;
using sabatex.V1C77;
using sabatex.V1C77.Models.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
//using System.Runtime.Remoting.Messaging;
//using System.Security.Cryptography.X509Certificates;

namespace Sabatex.ObjectsExchange.V1C77
{

    public delegate void DocumentBeforeSerialized(IDocument1C77 doc1C77, Dictionary<string, object> extData);
    public delegate void CheckDocumentIsSerialized(IDocument1C77 doc1C77, ref bool IsSerialized);
    public delegate void CatalogBeforeSerialized(ICatalog1C77 catalog1C77, Dictionary<string, object> extData);
    public class Adapter : IDisposable
    {
        ApiConnector api;
        IGlobalContext global;
        RootMetadata1C77 rootMetadata;
        int takeObjects = 200;
        ILogger logger;
        Guid destinationId;

        public event CatalogBeforeSerialized OnCatalogSerialized;
        public event DocumentBeforeSerialized OnDocumentSerialized;
        public event CheckDocumentIsSerialized OnCheckDocumentIsSerialized;

        public Adapter(ApiConnector api, IGlobalContext global, RootMetadata1C77 rootMetadata, ILogger logger, Guid destinationId)
        {
            this.api = api;
            this.global = global;
            this.rootMetadata = rootMetadata;
            this.logger = logger;
            this.destinationId = destinationId;
        }

        public void Dispose()
        {
            api = null;
            global = null;
            rootMetadata = null;
        }
        private DateTime ParseDateString(string dateString)
        {
            if (dateString.Length != 8)
                throw new ArgumentException($"The date wrong format {dateString}");
            int year = Convert.ToInt32(dateString.Substring(0, 4));
            int month = Convert.ToInt32(dateString.Substring(4, 2).TrimStart('0'));
            int day = Convert.ToInt32(dateString.Substring(6).TrimStart('0'));
            return new DateTime(year, month, day);
        }

        private string GetCatalogByCode(string catalogName, string code)
        {
            var catalog = global.CreateObjectCatalog(catalogName);
            if (catalog.FindByCode(code))
            {
                Dictionary<string, object> extData = new Dictionary<string, object>();
                if (OnCatalogSerialized != null) OnCatalogSerialized.Invoke(catalog, extData);
                return rootMetadata.SerializeJSON(catalog.CurrentItem, catalogName, extData);
            }
            throw new Exception($"Для Справочник.{catalogName} with code={catalog.Code.Trim()} не існує.");
        }

        private string GetDocumentByNumber(string docName, string code)
        {
            var doc = global.CreateObjectDocument(docName);
            var c = code.Split('=');
            if (c.Length != 2)
                throw new Exception($"Error code {code} for document {docName}");

            if (DateTime.TryParse(c[0], out DateTime date))
            {

                if (doc.FindByNum(c[1], date))
                {
                    Dictionary<string, object> extData = new Dictionary<string, object>();
                    if (OnCatalogSerialized != null) OnDocumentSerialized.Invoke(doc.CurrentDocument, extData);
                    return rootMetadata.SerializeJSON(doc.CurrentDocument, docName, extData);
                }
                throw new Exception($"Не знайдено документ {docName} за параметрами: {code} in document query");
            }

            throw new Exception($"Error parse date: {c[0]} in document query");
        }

        private void PostDocuments(string docName, DateTime date)
        {
            var metadata = rootMetadata.Документы[docName];
            var docs = global.CreateObjectDocument(docName);
            docs.SelectDocuments(date.BeginOfDay(), date.EndOfDay());
            while (docs.GetDocument())
            {
                var doc = docs.CurrentDocument;
                if (OnCheckDocumentIsSerialized != null)
                {
                    bool isSerialized = true;
                    OnCheckDocumentIsSerialized.Invoke(doc, ref isSerialized);
                    if (!isSerialized) continue;
                }

                var docDate = doc.DocDate;
                var docNumber = doc.DocNum;
                string objectId = $"{docDate.Year}{docDate.Month.ToString().PadLeft(2, '0')}{doc.DocNum}";
                string objectKey = $"{docName}-{objectId}";
                Dictionary<string, object> extData = new Dictionary<string, object>();
                if (OnDocumentSerialized != null)
                    OnDocumentSerialized.Invoke(doc, extData);
                string jsonText = rootMetadata.SerializeJSON(doc, docName, extData);
                try
                {
                    api.PostObjects($"Документ.{docName}", objectId, jsonText);
                    logger.Information($"Posted {docName} {doc.DocDate} {doc.DocNum}");
                }
                catch (Exception ex)
                {
                    logger.Error($"Error Posted order {doc.DocDate} {doc.DocNum} with error: {ex.Message}");
                }
                Thread.Sleep(200); // pause for query
            }
        }


        public void Query()
        {
            var queries = api.GetQueries(takeObjects);
            Console.WriteLine($"Queried objects: {queries.Count()}");
            foreach (var query in queries)
            {
                try
                { 
                logger?.Information($"Try answer object type = {query.ObjectType} id = {query.Id}");

                var c = query.ObjectType.Split('.');
                if (c.Length == 0)
                {
                    logger?.Error($"Error query Id={query.ObjectType}");
                    continue;
                }
                string json = string.Empty;
                bool singleObject = true;
                switch (c[0].ToUpper())
                {
                    case "СПРАВОЧНИК":
                        if (c.Length != 2)
                        {
                            logger.Error($"Error query СПРАВОЧНИК Id: {query.ObjectType}");
                            continue;
                        }
                        json = GetCatalogByCode(c[1], query.ObjectId);
                        break;
                    case "ДОКУМЕНТ":
                        if (c.Length != 2)
                        {
                            logger.Error($"Error query ДОКУМЕНТ Id: {query.ObjectType}");
                            continue;
                        }

                        json = GetDocumentByNumber(c[1], query.ObjectId);
                        break;
                    case "QUERYDOCS":
                        // QUERYDOCS.{DocName}.{Date} date format 20230201
                        if (c.Length != 3)
                        {
                            logger.Error($"Error query QUERYDOCS Id: {query.ObjectType}");
                            continue;
                        }
                        var dateDoc = ParseDateString(c[2]).BeginOfDay();
                        PostDocuments(c[1], dateDoc);
                        singleObject = false;
                        Thread.Sleep(1000);
                        break;
                    case "SALDO":
                        // SALDO.{ACC}.{Date} ACC account number withot dot, date format 20230201
                        if (c.Length != 3)
                        {
                            logger.Error($"Error query SALDO Id: {query.ObjectType}");
                            continue;
                        }
                        var date = ParseDateString(c[2]).BeginOfDay();
                        json = rootMetadata.QueryAccountSaldoAsJSON(global, c[1], date);
                        break;

                    default:
                        logger.Error($"Uknown type object {query.ObjectType}");
                        break;

                }

                    if (singleObject)
                    {
                        api.PostObjects(query.ObjectType, query.ObjectId, json);
                        logger.Information($"Posted {query.ObjectType} with code={query.ObjectId}");

                    }
                    api.DeleteQuery(query.Id);
                }
                catch (Exception ex)
                {
                    logger.Error($"Error Posted {query.ObjectType} with code={query.ObjectId} with error: {ex.Message}");
                }
            }
        }


    }
}