using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ServiceModel;
using Microsoft.Austria.WcfHelpers.SoapWithAttachments;
using System.Configuration;
using System.ServiceModel.Channels;
using System.Xml;
using System.Xml.Serialization;

namespace AbsCompleteTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //
            // Ask the user what to do?
            //
            Console.WriteLine("What do you want to test?");
            Console.WriteLine("Enter one of the following: absupload, uzeupload, uzedownload");
            string Action = Console.ReadLine();

            //
            // Create a dialog
            //
            Console.WriteLine();
            Console.WriteLine("--------------------------------");
            Console.WriteLine("Creating dialog...");
            string DialogId = CreateDialog();
            Console.WriteLine("Dialog created: {0}", DialogId);
            Console.WriteLine("--------------------------------");
            Console.WriteLine();

            try
            {
                switch (Action)
                {
                    case "absupload":
                        TestAbsUpload(DialogId);
                        break;
                    case "uzeupload":
                        TestUzeUpload(DialogId);
                        break;
                    case "uzedownload":
                        TestUzeDownload(DialogId);
                        break;
                    default:
                        Console.WriteLine("Invalid action {0} entered!", Action);
                        break;
                }
            }
            catch (Exception ex)
            {
                ProcessException(ex);
            }
            finally
            {
                //
                // Close the dialog
                //
                Console.WriteLine("Closing dialog...");
                CloseDialog(DialogId);
                Console.WriteLine("Dialog closed!");
            }

            Console.WriteLine();
            Console.WriteLine("Done!");
            Console.ReadLine();
        }

        #region Abs Interface

        private static void TestAbsUpload(string DialogId)
        {
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("Uploading ABS with attachment...");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine();
            
            //
            // Select ZIP file as attachement
            //
            Console.WriteLine("Loading ZIP file...");
            string ZipFileName = ConfigurationManager.AppSettings["All_ZipFilePath"];
            byte[] AttachmentContents = File.ReadAllBytes(ZipFileName);
            Console.WriteLine("File load successfully!");

            //
            // Create the request for uploading
            //
            Console.WriteLine("Creating Client Proxy!");
            GinaAbs7.AbsServiceClient ClientProxy =
                new AbsCompleteTest.GinaAbs7.AbsServiceClient("SwaAbsEndPoint");

            //
            // If you want to dynamically change the address of the endpoint
            // take a look at this:
            // EndpointAddress ea = new EndpointAddress("http://test");
            // ClientProxy.Endpoint.Address = ea;
            // 

            using (OperationContextScope Scope = new OperationContextScope(ClientProxy.InnerChannel))
            {
                OperationContext.Current.OutgoingMessageProperties.Add
                    (
                        SwaEncoderConstants.AttachmentProperty,
                        AttachmentContents
                    );

                Console.WriteLine("Calling the service...");
                GinaAbs7.AnfrageAntwort Response =
                    ClientProxy.sendenAnfrage
                    (
                        DialogId,
                        new AbsCompleteTest.GinaAbs7.BewilligungsAnfrage()
                        {
                            antragstyp = "R",
                            patientenDaten = new AbsCompleteTest.GinaAbs7.PatientenDaten()
                            {
                                SVNummer = ConfigurationManager.AppSettings["All_SVNummer"],
                                SVTCode = ConfigurationManager.AppSettings["Abs_SVT"],
                                vorname = ConfigurationManager.AppSettings["All_Vorname"],
                                nachname = ConfigurationManager.AppSettings["All_Zuname"],
                                geschlecht = ConfigurationManager.AppSettings["All_Geschlecht"],
                                EKVKNummer = string.Empty
                            },
                            verordnungen = new AbsCompleteTest.GinaAbs7.Verordnung[]
                                                {
                                                    new AbsCompleteTest.GinaAbs7.Verordnung() 
                                                    {
                                                        begruendung = ConfigurationManager.AppSettings["Abs_Begruendung"],
                                                        diagnose = ConfigurationManager.AppSettings["Abs_Diagnose"],
                                                        dosierung = ConfigurationManager.AppSettings["Abs_Dosierung"],
                                                        langzeitverordnung = ConfigurationManager.AppSettings["Abs_Langzeitverordnung"],
                                                        magistraleZubereitung = ConfigurationManager.AppSettings["Abs_MagistraleZubereitung"],
                                                        medikament = new AbsCompleteTest.GinaAbs7.Medikament() 
                                                        {
                                                            name = ConfigurationManager.AppSettings["Abs_Medikament_Name"],
                                                            pharmanummer = ConfigurationManager.AppSettings["Abs_Medikament_Pharmanummer"]
                                                        },
                                                        packungsanzahl = int.Parse(ConfigurationManager.AppSettings["Abs_Packungsanzahl"])
                                                    }
                                                },
                            verordnerinformation = ConfigurationManager.AppSettings["Abs_Verordnungsinformation"]
                        },
                        null
                    );
                Console.WriteLine("Service called successfully!");
                Console.WriteLine("----------------------------");
                XmlTextWriter xtw = new XmlTextWriter(Console.Out);
                xtw.Indentation = 3;
                xtw.IndentChar = ' ';
                xtw.Formatting = Formatting.Indented;
                XmlSerializer Serializer = new XmlSerializer(typeof(GinaAbs7.AnfrageAntwort));
                Serializer.Serialize(xtw, Response);
                Console.WriteLine();
                Console.WriteLine("----------------------------");
            }
        }

        #endregion

        #region Uze Interface

        private static void TestUzeDownload(string DialogId)
        {
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("Downloading UZE with attachment...");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine();

            AbsCompleteTest.GinaUze2.UzeServiceClient Client = 
                new AbsCompleteTest.GinaUze2.UzeServiceClient("SwaUzeEndPoint");
            using (OperationContextScope Scope = new OperationContextScope(Client.InnerChannel))
            {
                AbsCompleteTest.GinaUze2.RetrieveUzeAnlageReq Request = new AbsCompleteTest.GinaUze2.RetrieveUzeAnlageReq()
                {
                    svNummer = ConfigurationManager.AppSettings["All_SVNummer"],
                    uzeCode = ConfigurationManager.AppSettings["Uze_UzeCode"],
                    uzeId = int.Parse(ConfigurationManager.AppSettings["Uze_UzeId"]),
                    version = int.Parse(ConfigurationManager.AppSettings["Uze_UzeVersion"])
                };

                Client.retrieveUzeAnlage(DialogId, Request);

                if (OperationContext.Current.IncomingMessageProperties.ContainsKey(SwaEncoderConstants.AttachmentProperty))
                {
                    Console.WriteLine("Attachment Property received!!");

                    byte[] b = (byte[])OperationContext.Current.IncomingMessageProperties[SwaEncoderConstants.AttachmentProperty];
                    using (FileStream fs = new FileStream(ConfigurationManager.AppSettings["All_ZipFilePathSave"], FileMode.Create))
                    {
                        fs.Write(b, 0, b.Length);
                        fs.Flush();
                    }
                    Console.WriteLine("Written to file {0}!",
                        ConfigurationManager.AppSettings["All_ZipFilePathSave"]);
                }
            }
            Console.WriteLine("Done!");
        }

        private static void TestUzeUpload(string DialogId)
        {
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("Uploading UZE with attachment...");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine(); 

            Console.WriteLine("Loading file...");
            byte[] ZipContents = null;
            using (FileStream fs = new FileStream(ConfigurationManager.AppSettings["All_ZipFilePath"], FileMode.Open))
            {
                ZipContents = new byte[fs.Length];
                fs.Read(ZipContents, 0, ZipContents.Length);
            }

            Console.WriteLine("File loaded, executing request...");
            GinaUze2.UzeServiceClient Client = new GinaUze2.UzeServiceClient("SwaUzeEndPoint");
            using (OperationContextScope Scope = new OperationContextScope(Client.InnerChannel))
            {
                // Add the attachment
                OperationContext.Current.OutgoingMessageProperties
                    [SwaEncoderConstants.AttachmentProperty] = ZipContents;

                // Call the service
                GinaUze2.UZEWeisung Ret =
                    Client.storeUze(DialogId, "",
                        new GinaUze2.StoreUzeReq()
                        {
                            administrativeVermerke = ConfigurationManager.AppSettings["Uze_AdministrativeVermerke"],
                            adressePatient = new GinaUze2.Adresse()
                            {
                                PLZ = ConfigurationManager.AppSettings["Uze_AdressePlz"],
                                landbezeichnung = ConfigurationManager.AppSettings["Uze_AdresseLand"],
                                ort = ConfigurationManager.AppSettings["Uze_AdresseOrt"],
                                strasse = ConfigurationManager.AppSettings["Uze_AdresseStrasse"]
                            },
                            anspruchId = ConfigurationManager.AppSettings["Uze_AnspruchId"],
                            behandlungsKontext = ConfigurationManager.AppSettings["Uze_BehandlungsKontext"],
                            bewilligungsPflichtig = ConfigurationManager.AppSettings["Uze_BewilligungsPflicht"],
                            dringend = bool.Parse(ConfigurationManager.AppSettings["Uze_Dringend"]),
                            emailLa = ConfigurationManager.AppSettings["Uze_EmailLa"],
                            fachabteilungLe = ConfigurationManager.AppSettings["Uze_FachAbtLe"],
                            fachgebietLe = ConfigurationManager.AppSettings["Uze_FachgebietLe"],
                            hausarzt = new GinaUze2.Kontakt()
                            {
                                adresse = new GinaUze2.Adresse()
                                {
                                    PLZ = ConfigurationManager.AppSettings["Uze_AdressePlz"],
                                    landbezeichnung = ConfigurationManager.AppSettings["Uze_AdresseLand"],
                                    ort = ConfigurationManager.AppSettings["Uze_AdresseOrt"],
                                    strasse = ConfigurationManager.AppSettings["Uze_AdresseStrasse"]
                                },
                                telefon = ConfigurationManager.AppSettings["Uze_Tel"],
                                titelHinten = ConfigurationManager.AppSettings["Uze_TitelHinten"],
                                titelVorne = ConfigurationManager.AppSettings["Uze_TitelVorne"],
                                vorname = ConfigurationManager.AppSettings["All_Vorname"],
                                zuname = ConfigurationManager.AppSettings["All_Zuname"]
                            },
                            kblKennzeichen = bool.Parse(ConfigurationManager.AppSettings["Uze_KblKz"]),
                            medVertrauensPerson = new GinaUze2.Kontakt()
                            {
                                adresse = new GinaUze2.Adresse()
                                {
                                    PLZ = ConfigurationManager.AppSettings["Uze_AdressePlz"],
                                    landbezeichnung = ConfigurationManager.AppSettings["Uze_AdresseLand"],
                                    ort = ConfigurationManager.AppSettings["Uze_AdresseOrt"],
                                    strasse = ConfigurationManager.AppSettings["Uze_AdresseStrasse"]
                                },
                                telefon = ConfigurationManager.AppSettings["Uze_Tel"],
                                titelHinten = ConfigurationManager.AppSettings["Uze_TitelHinten"],
                                titelVorne = ConfigurationManager.AppSettings["Uze_TitelVorne"],
                                vorname = ConfigurationManager.AppSettings["All_Vorname"],
                                zuname = ConfigurationManager.AppSettings["All_Zuname"]
                            },
                            medizinischeDaten = new GinaUze2.StoreUzeMedDaten()
                            {
                                akutTherapie = ConfigurationManager.AppSettings["Uze_AkutTherapie"],
                                allergieMuInfo = ConfigurationManager.AppSettings["Uze_AllergieMuInfo"],
                                antikoaguliert = ConfigurationManager.AppSettings["Uze_Antikoaguliert"],
                                gewuenschteLeistung = ConfigurationManager.AppSettings["Uze_GewuenschteLeistung"],
                                medikation = ConfigurationManager.AppSettings["Uze_Medikation"],
                                notizLa = ConfigurationManager.AppSettings["Uze_NotizLa"],
                                symptomBeschreibung = ConfigurationManager.AppSettings["Uze_SymptomBeschreibung"],
                                verdachtsDiagnose = ConfigurationManager.AppSettings["Uze_VerdachtsDiagnose"],
                                vorgeschichte = ConfigurationManager.AppSettings["Uze_Vorgeschichte"],
                                wesentlicheNebenDiagnosen = ConfigurationManager.AppSettings["Uze_WesentlicheNebenDiagnosen"]
                            },
                            ordinationsIdLe = ConfigurationManager.AppSettings["Uze_OrdinationsIdLe"],
                            ordinationszeitenLa = ConfigurationManager.AppSettings["Uze_OrdinationszeitenLa"],
                            patientenVerfuegung = ConfigurationManager.AppSettings["Uze_PatientenVerfuegung"],
                            privateKontaktPerson = new GinaUze2.Kontakt()
                            {
                                adresse = new GinaUze2.Adresse()
                                {
                                    PLZ = ConfigurationManager.AppSettings["Uze_AdressePlz"],
                                    landbezeichnung = ConfigurationManager.AppSettings["Uze_AdresseLand"],
                                    ort = ConfigurationManager.AppSettings["Uze_AdresseOrt"],
                                    strasse = ConfigurationManager.AppSettings["Uze_AdresseStrasse"]
                                },
                                telefon = ConfigurationManager.AppSettings["Uze_Tel"],
                                titelHinten = ConfigurationManager.AppSettings["Uze_TitelHinten"],
                                titelVorne = ConfigurationManager.AppSettings["Uze_TitelVorne"],
                                vorname = ConfigurationManager.AppSettings["All_Vorname"],
                                zuname = ConfigurationManager.AppSettings["All_Zuname"]
                            },
                            svNummer = ConfigurationManager.AppSettings["All_SVNummer"],
                            svtCode = ConfigurationManager.AppSettings["Uze_SVT"],
                            telefonLa = ConfigurationManager.AppSettings["Uze_TelefonLa"],
                            telefonPatient = ConfigurationManager.AppSettings["Uze_TelefonPatient"],
                            weisungsTyp = ConfigurationManager.AppSettings["Uze_WeisungsTyp"]
                        });

                Console.WriteLine("Service called successfully!");
                Console.WriteLine("----------------------------");
                XmlSerializer ser = new XmlSerializer(typeof(GinaUze2.UZEWeisung));
                XmlTextWriter xtw = new XmlTextWriter(Console.Out);
                xtw.IndentChar = ' ';
                xtw.Indentation = 4;
                xtw.Formatting = Formatting.Indented;
                ser.Serialize(xtw, Ret);
                Console.WriteLine();
                Console.WriteLine("----------------------------");
            }
        }

        #endregion

        #region Dialog Handling Functions

        private static string CreateDialog()
        {
            string NewDialogId = string.Empty;

            GinaBase7.BaseServiceVPClient BaseProxy =
                new AbsCompleteTest.GinaBase7.BaseServiceVPClient("BaseServiceEndPoint");

            Console.WriteLine("- create the dialog!");
            NewDialogId = BaseProxy.createDialog
                            (
                                ConfigurationManager.AppSettings["cardreader"],
                                new AbsCompleteTest.GinaBase7.ProduktInfo()
                                {
                                    produktId = Int32.Parse(ConfigurationManager.AppSettings["productId"]),
                                    produktVersion = ConfigurationManager.AppSettings["productVersion"]
                                }, null
                            );

            Console.WriteLine("- authenticate the dialog!");
            GinaBase7.Vertragspartner Partner =
                            BaseProxy.authenticateDialog
                                (
                                    NewDialogId,
                                    string.Empty,
                                    ConfigurationManager.AppSettings["testPin"],
                                    1, null
                                );

            Console.WriteLine("- set the ordination id!");
            BaseProxy.setDialogAddress
                (
                    NewDialogId, Partner.ordination[0].ordinationId
                );

            return NewDialogId;
        }

        private static void CloseDialog(string dialogId)
        {
            GinaBase7.BaseServiceVPClient BaseProxy =
                new AbsCompleteTest.GinaBase7.BaseServiceVPClient();
            BaseProxy.closeDialog(dialogId);
        }

        #endregion

        #region Exception Handling Function(s)

        private static void ProcessException(Exception ex)
        {
            if (ex is FaultException)
            {
                MessageFault Fault = ((FaultException)ex).CreateMessageFault();
                Console.WriteLine();
                Console.WriteLine("----------------------");
                Console.WriteLine("Message Fault arrived!");
                Console.WriteLine("----------------------");
                if (Fault.HasDetail)
                {
                    XmlDictionaryReader FaultReader = Fault.GetReaderAtDetailContents();
                    XmlElement Element = Fault.GetDetail<XmlElement>();

                    //
                    // Query the exception
                    //
                    XmlNamespaceManager NsMgr = new XmlNamespaceManager(Element.OwnerDocument.NameTable);
                    NsMgr.AddNamespace("ns1", "http://soap.uze.client.chipkarte.at");
                    NsMgr.AddNamespace("ns3", "http://exceptions.soap.base.client.chipkarte.at");
                    NsMgr.AddNamespace("ns4", "http://exceptions.soap.base.client.chipkarte.at");
                    XmlNode Result = Element.SelectSingleNode("//ns3:code", NsMgr);
                    if (Result != null)
                        Console.WriteLine("Error Code: {0}", Result.InnerXml);
                    else
                        Console.WriteLine("Error code not found!");
                    Result = Element.SelectSingleNode("//ns4:message", NsMgr);
                    if (Result != null)
                        Console.WriteLine("Message: {0}", Result.InnerXml);
                    else
                        Console.WriteLine("Message not found!");
                    Console.WriteLine("----------------------------------------------");
                    Console.WriteLine("Remaining Error (complete)");
                    Console.WriteLine("--------------------------");
                    Console.WriteLine(Element.OuterXml);
                    Console.WriteLine("----------------------------------------------");
                }
                else
                {
                    Console.WriteLine("---------------------------");
                    Console.WriteLine("Standard Exception occured!");
                    Console.WriteLine("---------------------------");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("---------------------------");
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine("---------------------------");
                }
            }
            else
            {
                Console.WriteLine("---------------------------");
                Console.WriteLine("Standard Exception occured!");
                Console.WriteLine("---------------------------");
                Console.WriteLine(ex.Message);
                Console.WriteLine("---------------------------");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("---------------------------");
            }
        }

        #endregion
    }
}
