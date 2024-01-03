// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Protocol.Catalog;

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class Program
{
  public static void Main(string[] args)
  {
    MainAsync().GetAwaiter().GetResult();
  }

  private static async Task MainAsync()
  {
    using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
    using var httpClient = new HttpClient();
    var simpleHttpClient = new SimpleHttpClient(httpClient, loggerFactory.CreateLogger<SimpleHttpClient>());

    var fileCursor = new FileCursor("cursor.json", loggerFactory.CreateLogger<FileCursor>());
    var catalogClient = new CatalogClient(simpleHttpClient, loggerFactory.CreateLogger<CatalogClient>());
    var leafProcessor = new LoggerCatalogLeafProcessor(loggerFactory.CreateLogger<LoggerCatalogLeafProcessor>());
    var settings = new CatalogProcessorSettings
    {
      DefaultMinCommitTimestamp = DateTimeOffset.UtcNow.AddHours(-1),
      ExcludeRedundantLeaves = false,
    };

    var catalogProcessor = new CatalogProcessor(
      fileCursor,
      catalogClient,
      leafProcessor,
      settings,
      loggerFactory.CreateLogger<CatalogProcessor>());

    bool success;
    do
    {
      success = await catalogProcessor.ProcessAsync();
      if (!success)
      {
        Console.WriteLine("Processing the catalog leafs failed. Retrying.");
      }
    }
    while (!success);
  }
}