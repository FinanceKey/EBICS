/*	
 * 	
 *  This file is part of libfintx.
 *  
 *  Copyright (c) 2016 - 2020 Torsten Klinger
 * 	E-Mail: torsten.klinger@googlemail.com
 * 	
 * 	libfintx is free software; you can redistribute it and/or
 *	modify it under the terms of the GNU Lesser General Public
 * 	License as published by the Free Software Foundation; either
 * 	version 2.1 of the License, or (at your option) any later version.
 *	
 * 	libfintx is distributed in the hope that it will be useful,
 * 	but WITHOUT ANY WARRANTY; without even the implied warranty of
 * 	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 * 	Lesser General Public License for more details.
 *	
 * 	You should have received a copy of the GNU Lesser General Public
 * 	License along with libfintx; if not, write to the Free Software
 * 	Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
 * 	
 */

//#define WINDOWS

using System;
using System.Linq;
using System.IO;
using Xunit;
using Xunit.Abstractions;
using System.Threading.Tasks;
using libfintx.FinTS;
using System.Security.Cryptography.X509Certificates;
using libfintx.EBICS;
using libfintx.EBICSConfig;
using libfintx.EBICS.Parameters;
using System.Xml.Linq;
using System.Security.Cryptography;

#if (DEBUG && WINDOWS)
using hbci = libfintx;

using System.Windows.Forms;
#endif


namespace libfintx.Tests
{
    public class Test
    {
        private readonly ITestOutputHelper output;

        public Test(ITestOutputHelper output)
        {
            this.output = output;
        }




        [Fact]
        public void TestEbics()
        {
            var config = new Config();
            config.Address = "https://isotest.postfinance.ch/ebicsweb/ebicsweb";
            config.TLS = true;
            config.Insecure = true;
            config.Version = EbicsVersion.H005;
            config.Revision = EbicsRevision.Rev1;
            config.User = new UserParams() { HostId = "PFEBICS", UserId = "PFC00484", PartnerId = "PFC00484", SystemId = "PFEBICS" };


            System.Security.Cryptography.X509Certificates.X509Certificate2 cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(@"c:\Users\RonyMeyer\Documents\certificate\SelfSinged2026\financekey.nordea.crt");

            config.User.SignKeys = new SignKeyPair() { TimeStamp = DateTime.UtcNow, Version = SignVersion.A005, Certificate = cert };
            var client = EbicsClient.Factory().Create(config);

            var iniParam = new IniParams();
            iniParam.SecurityMedium = "0100";
            var iniResponse = client.INI(iniParam);



        }

        [Fact]
        public void TestEbicsHIA()
        {
            var authCert = new X509Certificate2(@"c:\Users\RonyMeyer\Documents\certificate\SelfSinged2026\financekey.nordea.crt");
            var encCert = new X509Certificate2(@"c:\Users\RonyMeyer\Documents\certificate\SelfSinged2026\financekey.nordea.crt");

            var client = EbicsClient.Factory().Create(new Config
            {
                Address = "https://isotest.postfinance.ch/ebicsweb/ebicsweb",
                Insecure = true,
                TLS = true,
                User = new UserParams
                {
                    HostId = "PFEBICS",
                    PartnerId = "PFC00484",
                    UserId = "PFC00484",
                    AuthKeys = new AuthKeyPair
                    {
                        Version = AuthVersion.X002,
                        TimeStamp = DateTime.Now,
                        Certificate = authCert
                    },
                    CryptKeys = new CryptKeyPair
                    {
                        Version = CryptVersion.E002,
                        TimeStamp = DateTime.Now,
                        Certificate = encCert
                    }
                }
            });

            var resp = client.HIA(new HiaParams());

        }

        [Fact]
        public void TestEbicsHPB()
        {
            var authCert = new X509Certificate2(@"c:\Users\RonyMeyer\Documents\certificate\SelfSinged2026\financekey.nordea.crt");
            var encCert = new X509Certificate2(@"c:\Users\RonyMeyer\Documents\certificate\SelfSinged2026\financekey.nordea.crt");
            var signCert = new X509Certificate2(@"c:\Users\RonyMeyer\Documents\certificate\SelfSinged2026\financekey.nordea.crt");

            var cert = new X509Certificate2(@"c:\Users\RonyMeyer\Documents\certificate\SelfSinged2026\financekey.nordea.pfx", "i3ElW60&nYI#u@51SM4QB^aMaLdq&tdq", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
            var rsa = cert.GetRSAPrivateKey();

            var client = EbicsClient.Factory().Create(new Config
            {
                Address = "https://isotest.postfinance.ch/ebicsweb/ebicsweb",
                Insecure = true,
                TLS = true,
                User = new UserParams
                {
                    HostId = "PFEBICS",
                    PartnerId = "PFC00484",
                    UserId = "PFC00484",
                    AuthKeys = new AuthKeyPair
                    {
                        Version = AuthVersion.X002,
                        TimeStamp = DateTime.Now,
                        Certificate = authCert,
                        PrivateKey = rsa
                    },
                    CryptKeys = new CryptKeyPair
                    {
                        Version = CryptVersion.E002,
                        TimeStamp = DateTime.Now,
                        Certificate = encCert,
                        PrivateKey = rsa
                    },
                    SignKeys = new SignKeyPair
                    {
                        Version = SignVersion.A005, // only A005 is supported right now
                        TimeStamp = DateTime.Now,
                        Certificate = signCert // internally we work with keys
                    }
                }
            });

            var hpbResp = client.HPB(new HpbParams());
            if (hpbResp.TechnicalReturnCode != 0 || hpbResp.BusinessReturnCode != 0)
            {
                // handle error
                return;
            }

            client.Config.Bank = hpbResp.Bank; // set bank's public keys

            // now issue other commands 
        }

        [Fact]
        public void TestEbicsINI()
        {
            var signCert = new X509Certificate2(@"c:\Users\RonyMeyer\Documents\certificate\SelfSinged2026\financekey.nordea.crt");

            var client = EbicsClient.Factory().Create(new Config
            {
                Address = "https://isotest.postfinance.ch/ebicsweb/ebicsweb",
                Insecure = true,
                TLS = true,
                User = new UserParams
                {
                    HostId = "PFEBICS",
                    PartnerId = "PFC00484",
                    UserId = "PFC00484",
                    SignKeys = new SignKeyPair
                    {
                        Version = SignVersion.A005, // only A005 is supported right now
                        TimeStamp = DateTime.Now,
                        Certificate = signCert // internally we work with keys
                    }
                }
            });

            var resp = client.INI(new IniParams());
        }


        [Fact]
        public void TestEbicsSTA()
        {
            var runSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\..\\..\\..\\.runsettings");
            var runSettingsXml = XDocument.Load(runSettingsPath);
            var cert1password = runSettingsXml.Descendants("TestRunParameters").Elements("Parameter").FirstOrDefault(e => e.Attribute("name").Value == "Cert1-Password")?.Attribute("value").Value;

            var cert1 = BuildSelfSignedServerCertificate("auth.ebics.financekey.com", cert1password, 999);
            var rsa1 = cert1.GetRSAPrivateKey();

            var authCert = new X509Certificate2(@"c:\Users\RonyMeyer\Documents\certificate\SelfSinged2026\financekey.nordea.crt");
            var encCert = new X509Certificate2(@"c:\Users\RonyMeyer\Documents\certificate\SelfSinged2026\financekey.nordea.crt");
            var signCert = new X509Certificate2(@"c:\Users\RonyMeyer\Documents\certificate\SelfSinged2026\financekey.nordea.crt");



            var cert = new X509Certificate2(@"c:\Users\RonyMeyer\Documents\certificate\SelfSinged2026\financekey.nordea.pfx", cert1password, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
            var rsa = cert.GetRSAPrivateKey();

            var client = EbicsClient.Factory().Create(new Config
            {
                Address = "https://isotest.postfinance.ch/ebicsweb/ebicsweb",
                Insecure = true,
                TLS = true,
                User = new UserParams
                {
                    HostId = "PFEBICS",
                    PartnerId = "PFC00484",
                    UserId = "PFC00484",
                    AuthKeys = new AuthKeyPair
                    {
                        Version = AuthVersion.X002,
                        TimeStamp = DateTime.Now,
                        Certificate = authCert,
                        PrivateKey = rsa
                    },
                    CryptKeys = new CryptKeyPair
                    {
                        Version = CryptVersion.E002,
                        TimeStamp = DateTime.Now,
                        Certificate = encCert,
                        PrivateKey = rsa
                    },
                    SignKeys = new SignKeyPair
                    {
                        Version = SignVersion.A005, // only A005 is supported right now
                        TimeStamp = DateTime.Now,
                        Certificate = signCert // internally we work with keys
                    }
                }
            });

            var hpbResp = client.HPB(new HpbParams());
            if (hpbResp.TechnicalReturnCode != 0 || hpbResp.BusinessReturnCode != 0)
            {
                // handle error
                return;
            }

            client.Config.Bank = hpbResp.Bank; // set bank's public keys

            // now issue other commands

//            var staResp = client.STA(new StaParams() { StartDate = DateTime.UtcNow.AddDays(-30).Date, EndDate = DateTime.UtcNow.Date });
            var staResp = client.VMK(new VmkParams());


        }


        private X509Certificate2 BuildSelfSignedServerCertificate(string certificateName, string password, int validityInDays = 365, string dns = null)
        {
            SubjectAlternativeNameBuilder sanBuilder = new SubjectAlternativeNameBuilder();

            sanBuilder.AddDnsName(dns ?? certificateName);

            var distinguishedName = new X500DistinguishedName($"CN={certificateName}");

            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            request.CertificateExtensions.Add(
                new X509KeyUsageExtension(X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature, false));

            request.CertificateExtensions.Add(sanBuilder.Build());

            var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(365));

            return new X509Certificate2(certificate.Export(X509ContentType.Pfx, password), password);
        }

    }

}
