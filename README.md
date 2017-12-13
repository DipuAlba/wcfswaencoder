Code cloned from: http://wcfswaencoder.codeplex.com/

# WCF SOAP-With-Attachments Message Encoder
Created as a sample during an Interoperability Lab between Microsoft and SVC GmbH. in Austria!

During the Microsoft Austria Interoperability Council a project was recommended by SVC GmbH. www.svc.co.at to create reference implementations for .NET demonstrating on how-to leverage certain services offered in combination with the Austrian electronic health-care card (e-card) by the national insurance. While close to all e-card interfaces are interoperabile with .NET, at the same time new interoperability-challenges between these e-card services and the Microsoft platform were identified during from the council. Therefore we created an interoperability lab initiative out of the council to analyze these challenges and look for solutions. With this open source sample library we published a solution for doctor software vendors that need to integrate their software with services from the Austrian national insurance and the e-card.

This project is a sample-library that allows you sending and receiving binary attachments along with web service messages based on the W3C SOAP-With-Attachments standard. We have created this encoder as a sample during an interoperability lab with SVC GmbH., the company that is developing, extending and maintaining the electronic health care card services for the Austrian National Insurance.

Primary target of the interoperability lab was providing a better interoperability between Microsoft.NET and the Austrian e-card web services. While for most of the services this interoperability is great, there are two services that use SOAP-With-Attachments for attaching binary data to web services. WCF does not provide SOAP-With-Attachments support out-of-the-box and therefore we have built this message encoder to provide the necessary interoperability between Microsoft .NET through WCF and the e-card services of the Austrian National Insurance.

Specifically for Austria we provide a sample application that demonstrates usage of the encoder for calling the affected Austrian e-card services UZE and ABS. This sample application can be used by doctor software vendors in Austria to learn, how they can extend their software for making it possible to call these services!

Click the following link to download the sample: [WcfSwaEncoder-With-Austrian-e-card-Services-Test.zip](http://www.codeplex.com/Download?ProjectName=wcfswaencoder&DownloadId=87250)

Note that this message encoder is a sample-implementation, only! Feel free to use it as you want, but note that using the encoder is at your own risk and your own responsibility!

Feel free to contribute and extend the encoder or provide further feedback!

Your Microsoft Austria Interoperability Counil team

Members of the e-card interoperability lab in alphabetical order:
Stefan Machura (SVC GmbH), Jovanovic Nenad (SVC GmbH), Roland Pezzei (SVC GmbH), Rainer Schügerl (SVC GmbH.), Mario Szpuszta (Microsoft), Thomas Woisetschläger (SVC GmbH)

Last edited Nov 5, 2009 at 11:28 AM by mszcool, version 5
