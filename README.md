# dotnet-publicsuffix-data

Wrapper around the Public Suffix List https://publicsuffix.org/.

The Public Suffix List is a community-maintained registry of top-level domains, meaning domains under which
you can directly register names. For example, `.org`, `.com`, and `.uk` are top-level domains, so you can 
register an `example.org`, `example.com`, or `example.uk` domain. However, `.co.uk`, despite consisting of
two tokens, is also a top-level domain, so you cannot register `co.uk` as a domain.

This is a .NET port of https://github.secureserver.net/PC/node-publicsuffix-data


## Simple Usage

Using this is as simple as `new`ing up an instance and calling `GetTldAsync`:

```c#
using GoDaddy.PublicSuffixData;

public class SampleClass
{
    public async Task SampleMethod()
    {
        var dataStore = new PublicSuffixDataStore();
        var tld = await dataStore.GetTldAsync("sample.co.uk");
        Assert.AreEqual(tld, "co.uk");    
    }
}
```


## How it works

Obviously, we don't want to download this database on every request. This library relies on a series of
caches. If the data is available in memory, that is used. Else, a file system cache is checked. Otherwise,
the data is fetched from the Internet and cached in memory and on disk. Caches have two settings: 
`timeToStale` and `timeToExpired`. If data is _stale_, it will still be used, but a newer copy of the data
is asynchronously fetched and cached. If the data is _expired_, we wait on the latest data to be fetched.


## Custom Configuration

Out of the box, this library uses these default settings:

<table>
    <thead>
        <tr>
            <th>Setting</th><th>Value</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>url</td><td>https://publicsuffix.org/list/effective_tld_names.dat</td>
        </tr>
        <tr>
            <td>fileCache</td><td>%APPDATA%\.publicsuffix.org</td>
        </tr>
        <tr>
            <td>timeToStale</td><td>10 days</td>
        </tr>
        <tr>
            <td>timeToExpired</td><td>30 days</td>
        </tr>
    </tbody>
</table>

To customize settings, there are two options. First off, you can add a configuration section to your
application's `web.config` or `app.config`:

```xml
<configuration>
  <configSections>
    <section name="goDaddy.publicSuffixData" type="GoDaddy.PublicSuffixData.PublicSuffixDataConfigSection, GoDaddy.PublicSuffixData"/>
  </configSections>
  <goDaddy.publicSuffixData
    timeToStale="5.00:00:00"
    timeToExpired="10.00:00:00"
    url="http://other.url/"
    cacheFile="C:\Path\To\File.json"/>
</configuration>
```

Alternately, the `PublicSuffixDataStore` constructor can take an object implementing `IPublicSuffixConfig`.
