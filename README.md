<img src="https://github.com/mrklintscher/libfintx/blob/master/res/logo.png" align="right">

# EBICS

An C# based client library for **EBICS H004** and **EBICS H005**.

The Electronic Banking Internet Communication Standard (EBICS) is a German / French transmission protocol developed by the German Banking Industry Committee for sending payment information between banks over the Internet. It grew out of the earlier BCS-FTAM protocol that was developed in 1995, with the aim of being able to use internet connections and TCP/IP. It is mandated for use by German banks and has also been adopted by France and Switzerland. [Wikipedia](https://en.wikipedia.org/wiki/Electronic_Banking_Internet_Communication_Standard).

This client library supports EBICS H004 and H005.

It can be used to read the balance of a bank account, receive an account statement, and make a SEPA payment using **EBICS**.

# Usage

There are many reasons why you need to use a banking library which can exchange data from your application with the bank. One reason for example is to found a [Fintech](https://de.wikipedia.org/wiki/Finanztechnologie).

# Target platforms

* .NET 6 (moved away from Standard 2.0 to use more native code)

# Sample

Look at the demo projects inside the master branch.

# Features

* Get Balance (**HKSAL**)
* Request Transactions (**HKKAZ**)
* Transfer money (**HKCCS**)
* Transfer money at a certain time (**HKCCS**)
* Collective transfer money (**HKCCM**)
* Collective transfer money terminated (**HKCME**)
* Rebook money from one to another account (**HKCUM**)
* Collect money (**HKDSE**)
* Collective collect money (**HKDME**)
* Load mobile phone prepaid card (**HKPPD**)
* Submit banker's order (**HKCDE**)
* Get banker's orders (**HKCSB**)
* Send Credit Transfer Initiation (**CCT**)
* Send Direct Debit Initiation (**CDD**)
* Pick up Swift daily statements (**STA**)

# Specification

For exact information please refer to the [german version of the specification](http://www.hbci-zka.de/spec/spezifikation.htm).

# Tested banks

* Raiffeisenbanken
* Sparkassen
* DKB
* DiBa
* Consorsbank
* Sparda
* Postbank
* Norisbank
* Deutsche Bank
* Unicredit Bank
* Commerzbank

# Sample code

Check account balance.

```csharp
/// <summary>
/// Kontostand abfragen
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
private async void btn_kontostand_abfragen_Click(object sender, EventArgs e)
{
    var connectionDetails = GetConnectionDetails();
    var client = new FinTsClient(connectionDetails);
    var sync = await client.Synchronization();

    HBCIOutput(sync.Messages);

    if (sync.IsSuccess)
    {
        // TAN-Verfahren
        client.HIRMS = txt_tanverfahren.Text;

        if (!await InitTANMedium(client))
            return;

        var balance = await client.Balance(_tanDialog);

        HBCIOutput(balance.Messages);

        if (balance.IsSuccess)
            SimpleOutput("Kontostand: " + Convert.ToString(balance.Data.Balance));
    }
}
```

# SSL verification

The verification process is done by using the default [**WebRequest**](https://msdn.microsoft.com/de-de/library/system.net.webrequest(v=vs.110).aspx) class.

# Limitations

* Usage with certificates has been prepared but not completely implemented yet. It works with private/public keys.
* Only version A005 for signatures can be used. A006 uses PSS padding, which is currently not supported by .NET Core 2.x. Dependency to Bouncy Castle has been removed and replaced with native code.
* Only version E002 for encryption can be used.
* Only version X002 for authentication can be used.
* It was developed using EBICS Version H004. Any new feature will only be tested and developped for H005.

# Copyright & License

Copyright (c) 2016 - 2023 **Torsten Klinger**, 2023 **Rony Meyer**

Licensed under **GNU LESSER GENERAL PUBLIC LICENSE Version 3, 29 June 2007**. Please read the LICENSE file.

# Support

You can contact me via [E-Mail](mailto:rony@financekey.com).
