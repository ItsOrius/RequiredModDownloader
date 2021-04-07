using System;
using System.IO;

namespace RequiredModInstaller
{
    public class WebsiteBuilder
    {
        public void CreateWebsite(String[] verifiedNames, String[] verifiedSources, String[] customNames, String[] customSources, String exportPath)
        {
            String websiteSource = $"<!DOCTYPE html>\n<html>\n<head>\n<title>RequiredModInstaller</title>\n</head>\n<body>\n<h1>RequiredModInstaller Sources</h1>\n<h2>Verified Mods</h2>\n";
            for (int i = 0; i < verifiedNames.Length; i++) websiteSource += $"<a href = {'"'}{verifiedSources[i]}{'"'} target = {'"'}_self{'"'}>{verifiedNames[i]}</a>\n";
            websiteSource += "<h2>Custom Mods</h2>\n";
            for (int i = 0; i < customNames.Length; i++) websiteSource += $"<a href = {'"'}{customSources[i]}{'"'} target = {'"'}_self{'"'}>{customNames[i]}</a>\n";
            websiteSource += "</body>\n</html>";
            File.WriteAllText(exportPath, websiteSource);
        }
    }
}