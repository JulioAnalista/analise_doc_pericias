using System.IO;
using System;
using System.Collections.Generic;
using System.Reflection;
using Adobe.PDFServicesSDK;
using Adobe.PDFServicesSDK.auth;
using Adobe.PDFServicesSDK.pdfops;
using Adobe.PDFServicesSDK.io;
using Adobe.PDFServicesSDK.exception;
using Adobe.PDFServicesSDK.options.extractpdf;
using System.Threading;
using UglyToad.PdfPig.Logging;
using AdobeExecutionContext = Adobe.PDFServicesSDK.ExecutionContext;
using log4net;
using log4net.Repository;
using log4net.Config;

namespace GabIA.WPF
{
    public class AdobePDFExtractor
    {
        private static readonly log4net.ILog log = LogManager.GetLogger(typeof(AdobePDFExtractor));

        public AdobePDFExtractor()
        {
            ConfigureLogging();
        }

        public static string ExtractTextUsingAdobe(string inputFilePath)
        {
            string outputFilePath = string.Empty;

            // Configure the logging.
            ConfigureLogging();

            try
            {
                // Initial setup, create credentials instance.
               // Credentials credentials = Credentials.ServiceAccountCredentialsBuilder()
                    //.FromFile(Directory.GetCurrentDirectory() + "/pdfservices-api-credentials.json")
                    //.Build();

                Credentials credentials = Credentials.ServiceAccountCredentialsBuilder()
                    .FromFile("d:\\Config\\appsettings.json")
                    .Build();

                // Create an ExecutionContext using credentials and create a new operation instance.
                Adobe.PDFServicesSDK.ExecutionContext executionContext = Adobe.PDFServicesSDK.ExecutionContext.Create(credentials);
                ExtractPDFOperation extractPdfOperation = ExtractPDFOperation.CreateNew();

                // Provide an input FileRef for the operation.
                FileRef sourceFileRef = FileRef.CreateFromLocalFile(inputFilePath);
                extractPdfOperation.SetInputFile(sourceFileRef);

                // Build ExtractPDF options and set them into the operation.
                ExtractPDFOptions extractPdfOptions = ExtractPDFOptions.ExtractPDFOptionsBuilder()
                    .AddElementsToExtract(new List<ExtractElementType>(new[] { ExtractElementType.TEXT }))
                    .AddCharsInfo(true)
                    .Build();
                extractPdfOperation.SetOptions(extractPdfOptions);

                // Execute the operation.
                FileRef result = extractPdfOperation.Execute(executionContext);

                //Generating a file name
                outputFilePath = CreateOutputFilePath();

                // Save the result to the specified location.
                result.SaveAs(Directory.GetCurrentDirectory() + outputFilePath);
            }
            catch (ServiceUsageException ex)
            {
                log.Error("Exception encountered while executing operation", ex);
            }
            catch (ServiceApiException ex)
            {
                log.Error("Exception encountered while executing operation", ex);
            }
            catch (SDKException ex)
            {
                log.Error("Exception encountered while executing operation", ex);
            }
            catch (IOException ex)
            {
                log.Error("Exception encountered while executing operation", ex);
            }
            catch (Exception ex)
            {
                log.Error("Exception encountered while executing operation", ex);
            }

            return outputFilePath;
        }



        public string ExtractText(string pdfFilePath)
        {
            string outputFilePath = string.Empty;

            try
            {
                // Initial setup, create credentials instance.
                Credentials credentials = Credentials.ServiceAccountCredentialsBuilder()
                    .FromFile(Directory.GetCurrentDirectory() + "/pdfservices-api-credentials.json")
                    .Build();

                // Create an ExecutionContext using credentials and create a new operation instance.
                AdobeExecutionContext executionContext = AdobeExecutionContext.Create(credentials);
                ExtractPDFOperation extractPdfOperation = ExtractPDFOperation.CreateNew();

                // Provide an input FileRef for the operation.
                FileRef sourceFileRef = FileRef.CreateFromLocalFile(pdfFilePath);
                extractPdfOperation.SetInputFile(sourceFileRef);

                // Build ExtractPDF options and set them into the operation.
                ExtractPDFOptions extractPdfOptions = ExtractPDFOptions.ExtractPDFOptionsBuilder()
                    .AddElementsToExtract(new List<ExtractElementType>(new[] { ExtractElementType.TEXT }))
                    .AddCharsInfo(true)
                    .Build();
                extractPdfOperation.SetOptions(extractPdfOptions);

                // Execute the operation.
                FileRef result = extractPdfOperation.Execute(executionContext);

                // Generating a file name
                outputFilePath = CreateOutputFilePath();

                // Save the result to the specified location.
                result.SaveAs(Directory.GetCurrentDirectory() + outputFilePath);
            }
            catch (ServiceUsageException ex)
            {
                log.Error("Exception encountered while executing operation", ex);
            }
            catch (ServiceApiException ex)
            {
                log.Error("Exception encountered while executing operation", ex);
            }
            catch (SDKException ex)
            {
                log.Error("Exception encountered while executing operation", ex);
            }
            catch (IOException ex)
            {
                log.Error("Exception encountered while executing operation", ex);
            }
            catch (Exception ex)
            {
                log.Error("Exception encountered while executing operation", ex);
            }

            return outputFilePath;
        }

        private static void ConfigureLogging()
        {
            ILoggerRepository logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        }

        // Generates a string containing a directory structure and file name for the output file.
        public static string CreateOutputFilePath()
        {
            String timeStamp = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH'-'mm'-'ss");
            return ("/output/extract" + timeStamp + ".zip");
        }

    }
}
